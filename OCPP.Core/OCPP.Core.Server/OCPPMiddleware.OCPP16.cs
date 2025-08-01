﻿/*
 * OCPP.Core - https://github.com/dallmann-consulting/OCPP.Core
 * Copyright (C) 2020-2024 dallmann consulting GmbH.
 * All Rights Reserved.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OCPP.Core.Server.Entities;
using OCPP.Core.Server.Extensions.Interfaces;
using OCPP.Core.Server.Messages_OCPP16;
using OCPP.Core.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OCPP.Core.Server
{
    public partial class OCPPMiddleware
    {
        /// <summary>
        /// Waits for new OCPP V1.6 messages on the open websocket connection and delegates processing to a controller
        /// </summary>
        private async Task Receive16(ChargePointStatus chargePointStatus, HttpContext context, NpgsqlDbContext dbContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            ControllerOCPP16 controller16 = new ControllerOCPP16(_configuration, _logFactory, chargePointStatus, dbContext);

            int maxMessageSizeBytes = _configuration.GetValue<int>("MaxMessageSize", 0);

            byte[] buffer = new byte[1024 * 4];
            MemoryStream memStream = new MemoryStream(buffer.Length);

            // ส่งข้อความต้อนรับ
            await chargePointStatus.WebSocket.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { status = "Accepted" })), WebSocketMessageType.Text, true, CancellationToken.None);

            try
            {
                while (chargePointStatus.WebSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await chargePointStatus.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    //if (result.CloseStatus.HasValue)
                    //{
                    //    await chargePointStatus.WebSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    //    logger.LogInformation("OCPPMiddleware.Receive16 => Websocket closed: State={0} / CloseStatus={1}", chargePointStatus.WebSocket.State, chargePointStatus.WebSocket.CloseStatus);
                    //    _chargePointStatusDict.Remove(chargePointStatus.Id);
                    //    return;
                    //}
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await chargePointStatus.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        logger.LogInformation("OCPPMiddleware.Receive16 => Websocket disconnected: State={0} / CloseStatus={1}", chargePointStatus.WebSocket.State, chargePointStatus.WebSocket.CloseStatus);
                        break;
                    }

                    if (result != null && result.MessageType != WebSocketMessageType.Close)
                    {
                        logger.LogTrace("OCPPMiddleware.Receive16 => Receiving segment: {0} bytes (EndOfMessage={1} / MsgType={2})", result.Count, result.EndOfMessage, result.MessageType);
                        memStream.Write(buffer, 0, result.Count);

                        // max. allowed message size NOT exceeded - or limit deactivated?
                        if (maxMessageSizeBytes == 0 || memStream.Length <= maxMessageSizeBytes)
                        {
                            if (result.EndOfMessage)
                            {
                                // read complete message into byte array
                                byte[] bMessage = memStream.ToArray();
                                // reset memory stream für next message
                                memStream = new MemoryStream(buffer.Length);

                                string ocppMessage = UTF8Encoding.UTF8.GetString(bMessage);

                                // write message (async) to dump directory
                                _ = Task.Run(() =>
                                {
                                    DumpMessage("ocpp16-in", ocppMessage);
                                });   

                                Match match = Regex.Match(ocppMessage, MessageRegExp);
                                if (match != null && match.Groups != null && match.Groups.Count >= 3)
                                {
                                    string messageTypeId = match.Groups[1].Value;
                                    string uniqueId = match.Groups[2].Value;
                                    string action = match.Groups[3].Value;
                                    string jsonPaylod = match.Groups[4].Value;
                                    logger.LogInformation("OCPPMiddleware.Receive16 => OCPP-Message: Type={0} / ID={1} / Action={2})", messageTypeId, uniqueId, action);

                                    string[] urlParts = context.Request.Path.Value.Split('/');
                                    string urlConnectorId = (urlParts.Length >= 5) ? urlParts[4] : null;
                                    string urlChargeTagId = (urlParts.Length >= 6) ? urlParts[5] : null;

                                    for (int i = 0; i< match.Groups.Count;i++) {
                                        Console.WriteLine($"match.Groups[{i}] : " + match.Groups[i].Value);
                                    }
                           
                                    //OCPPMessage msgIn = new OCPPMessage(messageTypeId, uniqueId, action, jsonPaylod);
                                    OCPPMessage msgIn = new OCPPMessage(chargePointStatus.Id, urlConnectorId, urlChargeTagId, messageTypeId, uniqueId, action, jsonPaylod);
                                    // Send raw incoming messages to extensions

                                    _ = Task.Run(() =>
                                    {
                                        ProcessRawIncomingMessageSinks(chargePointStatus.Protocol, chargePointStatus.Id, msgIn);
                                    });

                                    if (msgIn.MessageType == "2")
                                    {
                                        // Request from chargepoint to OCPP server
                                        OCPPMessage msgOut = await controller16.ProcessRequest(msgIn);

                                        // Send OCPP message with optional logging/dump
                                        await SendOcpp16Message(msgOut, logger, chargePointStatus);

                                        if (msgIn.Action == "StatusNotification")
                                        {
                                            try
                                            {
                                                StatusNotificationRequest statusNotificationRequest = JsonConvert.DeserializeObject<StatusNotificationRequest>(msgIn.JsonPayload);
                                                var chargePoint = dbContext.TblChargers.Where(x => x.FShortName == msgIn.ChargePointId).FirstOrDefault() ?? new TblCharger();
                                                var connector = dbContext.TblConnectorStatuses.Where(x => x.FChargerId == chargePoint.FId && x.FConnectorId == statusNotificationRequest.ConnectorId).FirstOrDefault();

                                                ConnectorStatusEnum newStatus = ConnectorStatusEnum.Undefined;
                                                switch (statusNotificationRequest.Status)
                                                {
                                                    case StatusNotificationRequestStatus.Available:
                                                        newStatus = ConnectorStatusEnum.Available;
                                                        break;
                                                    case StatusNotificationRequestStatus.Preparing:
                                                        newStatus = ConnectorStatusEnum.Preparing;
                                                        break;
                                                    case StatusNotificationRequestStatus.Charging:
                                                        newStatus = ConnectorStatusEnum.Charging;
                                                        break;
                                                    case StatusNotificationRequestStatus.Reserved:
                                                        newStatus = ConnectorStatusEnum.Reserved;
                                                        break;
                                                    case StatusNotificationRequestStatus.SuspendedEVSE:
                                                    case StatusNotificationRequestStatus.SuspendedEV:
                                                        newStatus = ConnectorStatusEnum.Occupied;
                                                        break;
                                                    case StatusNotificationRequestStatus.Finishing:
                                                        newStatus = ConnectorStatusEnum.Finishing;
                                                        break;
                                                    case StatusNotificationRequestStatus.Unavailable:
                                                        newStatus = ConnectorStatusEnum.Unavailable;
                                                        break;
                                                    case StatusNotificationRequestStatus.Faulted:
                                                        newStatus = ConnectorStatusEnum.Faulted;
                                                        break;
                                                }
                                                _chargePointStatusDict[msgIn.ChargePointId].OnlineConnectors[statusNotificationRequest.ConnectorId].Status = newStatus;

                                                var payment = await dbContext.TblPayments.FirstOrDefaultAsync(x => x.FTransactionId == connector.FTransactionId);
                                                var trans = await dbContext.TblTransactions.FirstOrDefaultAsync(x => x.FChargerId == chargePoint.FId && x.FConnectorId == connector.FId && x.FId == connector.FTransactionId);
                                                if (trans != null)
                                                {
                                                    if (newStatus == ConnectorStatusEnum.Charging)
                                                    {
                                                        trans.FTransactionStatus = newStatus.ToString();
                                                        trans.FStartTime = DateTime.UtcNow;
                                                        dbContext.TblTransactions.Update(trans);
                                                        _ = await dbContext.SaveChangesAsync();
                                                    }
                                                    else if (newStatus == ConnectorStatusEnum.Finishing)
                                                    {
                                                        trans.FTransactionStatus = newStatus.ToString();
                                                        trans.FEndTime = DateTime.UtcNow;
                                                        trans.FCost = payment.FPaymentAmount ?? 0;
                                                        dbContext.TblTransactions.Update(trans);
                                                        _ = await dbContext.SaveChangesAsync();

                                                        connector.FTransactionId = null;
                                                        dbContext.TblConnectorStatuses.Update(connector);
                                                        _ = await dbContext.SaveChangesAsync();
                                                    }
                                                }
                                            }
                                            catch (Exception ex) { logger.LogError(ex.Message); }
                                        }
                                        else if (msgIn.Action == "Heartbeat")
                                        {
                                            _chargePointStatusDict[msgIn.ChargePointId].Heartbeat = DateTime.UtcNow.ToString("o");
                                        }
                                        else if (msgIn.Action == "MeterValues")
                                        {
                                            MeterValuesRequest meterValueRequest = JsonConvert.DeserializeObject<MeterValuesRequest>(msgIn.JsonPayload);

                                            var chargePoint = dbContext.TblChargers.Where(x => x.FShortName == msgIn.ChargePointId).FirstOrDefault() ?? new TblCharger();
                                            var connector = dbContext.TblConnectorStatuses.Where(x => x.FChargerId == chargePoint.FId && x.FConnectorId == meterValueRequest.ConnectorId).FirstOrDefault();

                                            if (connector != null)
                                            {
                                                _chargePointStatusDict[msgIn.ChargePointId].OnlineConnectors[meterValueRequest.ConnectorId].ChargeRateKW = Convert.ToDouble(connector.FCurrentChargeKw);
                                                _chargePointStatusDict[msgIn.ChargePointId].OnlineConnectors[meterValueRequest.ConnectorId].MeterKWH = Convert.ToDouble(connector.FCurrentMeter);
                                                _chargePointStatusDict[msgIn.ChargePointId].OnlineConnectors[meterValueRequest.ConnectorId].SoC = Convert.ToDouble(connector.FStateOfCharge);
                                            }
                                            else
                                            {
                                                logger.LogError("MeterValues Transaction RemoteStopTransaction => Exception writing connector not found : {0}", msgIn.JsonPayload);
                                            }
                                            
                                            try
                                            {
                                                var trans = dbContext.TblTransactions.FirstOrDefault(x => x.FId == connector.FTransactionId);
                                                if (trans != null)
                                                {
                                                    if (trans.FTransactionStatus == "Charging" || trans.FTransactionStatus == "Finishing")
                                                    {
                                                        double? MeterStart = 0;
                                                        var connectorStatusView = dbContext.ConnectorStatusViews.FirstOrDefault(x => x.FChargerId == chargePoint.FId && x.FConnectorId == meterValueRequest.ConnectorId);
                                                        if (connectorStatusView != null)
                                                        {
                                                            MeterStart = Convert.ToDouble(connectorStatusView.FMeterStart);
                                                        }
                                                        double? startMeeter = _chargePointStatusDict[msgIn.ChargePointId].OnlineConnectors[meterValueRequest.ConnectorId].MeterKWH;
                                                        double? meter = startMeeter <= 0 ? MeterStart : startMeeter - MeterStart;
                                                        if (Convert.ToDouble(trans.FPreMeter) <= meter)
                                                        {
                                                            trans.FMeterStart = Convert.ToDecimal(_chargePointStatusDict[msgIn.ChargePointId].OnlineConnectors[meterValueRequest.ConnectorId].MeterKWH);
                                                            trans.FMeterEnd = Convert.ToDecimal(MeterStart);
                                                            trans.FTransactionStatus = "Finishing";
                                                            trans.FUpdated = DateTime.UtcNow;
                                                            trans.FEndTime = DateTime.UtcNow;
                                                            trans.FEndResult = "";
                                                            dbContext.TblTransactions.Update(trans);
                                                            _ = await dbContext.SaveChangesAsync();

                                                            var connectorStatus = dbContext.TblConnectorStatuses.Where(x => x.FChargerId == trans.FChargerId && x.FConnectorId == meterValueRequest.ConnectorId).ToList();
                                                            foreach (var _status in connectorStatus)
                                                            {
                                                                _status.FStateOfCharge = 0;
                                                                _status.FCurrentChargeKw = 0;
                                                                _status.FCurrentMeter = 0;
                                                                _status.FCurrentMeterTime = DateTime.UtcNow;

                                                                dbContext.TblConnectorStatuses.Update(_status);
                                                                _ = await dbContext.SaveChangesAsync();
                                                            }

                                                            RemoteStopTransactionRequest request = new RemoteStopTransactionRequest();
                                                            request.TransactionId = trans.FTransactionNo ?? 0;
                                                            OCPPMessage msgStoptrans = new OCPPMessage();
                                                            msgStoptrans.MessageType = "2";
                                                            msgStoptrans.Action = "RemoteStopTransaction";
                                                            msgStoptrans.UniqueId = Guid.NewGuid().ToString("N");
                                                            msgStoptrans.JsonPayload = JsonConvert.SerializeObject(request);
                                                            msgStoptrans.TaskCompletionSource = new TaskCompletionSource<string>();

                                                            await SendOcpp16Message(msgStoptrans, logger, chargePointStatus);
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception exp)
                                            {
                                                logger.LogError(exp, "Transaction RemoteStopTransaction => Exception writing connector status (ID={0} / Connector={1}): {2}", msgIn.ChargePointId, meterValueRequest.ConnectorId, exp.Message);
                                            }
                                        }
                                    }
                                    else if (msgIn.MessageType == "3" || msgIn.MessageType == "4")
                                    {
                                        // Process answer from chargepoint
                                        if (_requestQueue.ContainsKey(msgIn.UniqueId))
                                        {
                                            controller16.ProcessAnswer(msgIn, _requestQueue[msgIn.UniqueId]);
                                            _requestQueue.Remove(msgIn.UniqueId);
                                        }
                                        else
                                        {
                                            logger.LogError("OCPPMiddleware.Receive16 => HttpContext from caller not found / Msg: {0}", ocppMessage);
                                        }
                                    }
                                    else
                                    {
                                        // Unknown message type
                                        logger.LogError("OCPPMiddleware.Receive16 => Unknown message type: {0} / Msg: {1}", msgIn.MessageType, ocppMessage);
                                    }
                                }
                                else
                                {
                                    logger.LogWarning("OCPPMiddleware.Receive16 => Error in RegEx-Matching: Msg={0})", ocppMessage);
                                }
                            }
                            await BroadcastMessage(JsonConvert.SerializeObject(new { status = "Accepted" }));
                        }
                        else
                        {
                            // max. allowed message size exceeded => close connection (DoS attack?)
                            logger.LogInformation("OCPPMiddleware.Receive16 => Allowed message size exceeded - close connection");
                            await chargePointStatus.WebSocket.CloseOutputAsync(WebSocketCloseStatus.MessageTooBig, string.Empty, CancellationToken.None);
                            break;
                        }
                    }
                    else
                    {
                        logger.LogInformation("OCPPMiddleware.Receive16 => WebSocket Closed: CloseStatus={0} / MessageType={1}", result?.CloseStatus, result?.MessageType);
                        await chargePointStatus.WebSocket.CloseOutputAsync((WebSocketCloseStatus)3001, string.Empty, CancellationToken.None);
                        break;
                    }
                }
                
            }
            catch (Exception exp)
            {
                logger.LogError("OCPPMiddleware.Receive16 Error => Websocket disconnect: State={0} / CloseStatus={1} : {2}", chargePointStatus.WebSocket.State, chargePointStatus.WebSocket.CloseStatus, exp.Message);
                //_chargePointStatusDict.Remove(chargePointStatus.Id);
            }
            finally
            {
                //logger.LogInformation("OCPPMiddleware.Receive16 => Websocket closed: State={0} / CloseStatus={1}", chargePointStatus.WebSocket.State, chargePointStatus.WebSocket.CloseStatus);
                //_chargePointStatusDict.Remove(chargePointStatus.Id);
            }
        }

        /// <summary>
        /// Waits for new OCPP V1.6 messages on the open websocket connection and delegates processing to a controller
        /// </summary>
        private async Task Reset16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext, NpgsqlDbContext dbContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            ControllerOCPP16 controller16 = new ControllerOCPP16(_configuration, _logFactory, chargePointStatus, dbContext);

            Messages_OCPP16.ResetRequest resetRequest = new Messages_OCPP16.ResetRequest();
            resetRequest.Type = Messages_OCPP16.ResetRequestType.Soft;
            string jsonResetRequest = JsonConvert.SerializeObject(resetRequest);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "Reset";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }

        /// <summary>
        /// Sends a Unlock-Request to the chargepoint
        /// </summary>
        private async Task UnlockConnector16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext, NpgsqlDbContext dbContext, string urlConnectorId)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            ControllerOCPP16 controller16 = new ControllerOCPP16(_configuration, _logFactory, chargePointStatus, dbContext);

            Messages_OCPP16.UnlockConnectorRequest unlockConnectorRequest = new Messages_OCPP16.UnlockConnectorRequest();
            unlockConnectorRequest.ConnectorId = 0;

            if (!string.IsNullOrEmpty(urlConnectorId))
            {
                if (int.TryParse(urlConnectorId, out int iConnectorId))
                {
                    unlockConnectorRequest.ConnectorId = iConnectorId;
                }
            }
            logger.LogTrace("OCPPMiddleware.OCPP16 => UnlockConnector16: ChargePoint='{0}' / ConnectorId={1}", chargePointStatus.Id, unlockConnectorRequest.ConnectorId);

            string jsonResetRequest = JsonConvert.SerializeObject(unlockConnectorRequest);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "UnlockConnector";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }

        /// <summary>
        /// Sends a SetChargingProfile-Request to the chargepoint
        /// </summary>
        private async Task SetChargingProfile16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext, NpgsqlDbContext dbContext, string urlConnectorId, double power, string unit)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            ControllerOCPP16 controller16 = new ControllerOCPP16(_configuration, _logFactory, chargePointStatus, dbContext);

            // Parse connector id (int value)
            int connectorId = 0;
            if (!string.IsNullOrEmpty(urlConnectorId))
            {
                int.TryParse(urlConnectorId, out connectorId);
            }

            Messages_OCPP16.SetChargingProfileRequest setChargingProfileRequest = new Messages_OCPP16.SetChargingProfileRequest();
            setChargingProfileRequest.ConnectorId = connectorId;
            setChargingProfileRequest.CsChargingProfiles = new Messages_OCPP16.CsChargingProfiles();
            // Default values
            setChargingProfileRequest.CsChargingProfiles.ChargingProfileId = 100;
            setChargingProfileRequest.CsChargingProfiles.StackLevel = 1;
            setChargingProfileRequest.CsChargingProfiles.ChargingProfilePurpose = CsChargingProfilesChargingProfilePurpose.TxDefaultProfile;
            setChargingProfileRequest.CsChargingProfiles.ChargingProfileKind = CsChargingProfilesChargingProfileKind.Absolute;
            setChargingProfileRequest.CsChargingProfiles.ValidFrom = DateTime.UtcNow;
            setChargingProfileRequest.CsChargingProfiles.ValidTo = DateTime.UtcNow.AddYears(1);
            setChargingProfileRequest.CsChargingProfiles.ChargingSchedule = new ChargingSchedule()
            {
                ChargingRateUnit = (ChargingRateUnit)(string.Equals(unit, "A", StringComparison.InvariantCultureIgnoreCase) ? ChargingScheduleChargingRateUnit.A : ChargingScheduleChargingRateUnit.W),
                ChargingSchedulePeriod = new List<ChargingSchedulePeriod>()
                {
                    new ChargingSchedulePeriod()
                    {
                        StartPeriod = 0,    // Start 0:00h
                        Limit = power,
                        NumberPhases = 0
                    }
                }
            };

            logger.LogInformation ("OCPPMiddleware.OCPP16 => SetChargingProfile16: ChargePoint='{0}' / ConnectorId={1} / Power='{2}{3}'", chargePointStatus.Id, setChargingProfileRequest.ConnectorId, power, unit);

            string jsonResetRequest = JsonConvert.SerializeObject(setChargingProfileRequest);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "SetChargingProfile";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }

        /// <summary>
        /// Sends a ClearChargingProfile-Request to the chargepoint
        /// </summary>
        private async Task ClearChargingProfile16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext, NpgsqlDbContext dbContext, string urlConnectorId)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            ControllerOCPP16 controller16 = new ControllerOCPP16(_configuration, _logFactory, chargePointStatus, dbContext);

            Messages_OCPP16.ClearChargingProfileRequest clearChargingProfileRequest = new Messages_OCPP16.ClearChargingProfileRequest();
            // Default values
            clearChargingProfileRequest.Id = 100;
            clearChargingProfileRequest.StackLevel = 1;
            clearChargingProfileRequest.ChargingProfilePurpose = (ChargingProfilePurpose)ClearChargingProfileRequestChargingProfilePurpose.TxDefaultProfile;

            clearChargingProfileRequest.ConnectorId = 0;
            if (!string.IsNullOrEmpty(urlConnectorId))
            {
                if (int.TryParse(urlConnectorId, out int iConnectorId))
                {
                    clearChargingProfileRequest.ConnectorId = iConnectorId;
                }
            }
            logger.LogTrace("OCPPMiddleware.OCPP16 => ClearChargingProfile16: ChargePoint='{0}' / ConnectorId={1}", chargePointStatus.Id, clearChargingProfileRequest.ConnectorId);

            string jsonResetRequest = JsonConvert.SerializeObject(clearChargingProfileRequest);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "ClearChargingProfile";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }

        public async Task CancelReservation16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");

            CancelReservationRequest request = new CancelReservationRequest();

            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<CancelReservationRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string _apiResult = "{\"status\": " + JsonConvert.ToString("Faulted") + "}";
                apiCallerContext.Response.StatusCode = 200;
                apiCallerContext.Response.ContentType = "application/json";
                await apiCallerContext.Response.WriteAsync(_apiResult);
                return;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "CancelReservation";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }
        private async Task RemoteStartTransaction16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            RemoteStartTransactionRequest remoteStartTransactionRequest = new RemoteStartTransactionRequest();
            string apiResult = string.Empty;
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                remoteStartTransactionRequest = JsonConvert.DeserializeObject<RemoteStartTransactionRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (remoteStartTransactionRequest == null || string.IsNullOrEmpty(remoteStartTransactionRequest.IdTag))
            {
                string[] urlParts = apiCallerContext.Request.Path.Value.Split('/');
                string urlConnectorId = (urlParts.Length >= 5) ? urlParts[4] : "0";
                string urlTagId = (urlParts.Length >= 6) ? urlParts[5] : _configuration.GetSection("TagIDTest").Value;

                remoteStartTransactionRequest = new RemoteStartTransactionRequest();
                remoteStartTransactionRequest.ConnectorId = Convert.ToInt32(urlConnectorId);
                remoteStartTransactionRequest.IdTag = urlTagId;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(remoteStartTransactionRequest);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.ChargePointId = chargePointStatus.Id;
            msgOut.ConnectorId = remoteStartTransactionRequest.ConnectorId.ToString();
            msgOut.ChargeTagId = remoteStartTransactionRequest.IdTag;
            msgOut.MessageType = "2";
            msgOut.Action = "RemoteStartTransaction";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();


            logger.LogInformation("RemoteStartTransaction => Save ConnectorStatus: ID={0} / Connector={1} ", chargePointStatus.Id, remoteStartTransactionRequest.ConnectorId);


            logger.LogTrace("RemoteStartTransaction => Response serialized Data:" + msgOut.JsonPayload);

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";

            await apiCallerContext.Response.WriteAsync(apiResult);
        }

        private async Task RemoteStopTransaction16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext, NpgsqlDbContext dbContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            RemoteStopTransactionRequest remoteStopTransactionRequest = new RemoteStopTransactionRequest();

            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                remoteStopTransactionRequest = JsonConvert.DeserializeObject<RemoteStopTransactionRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (remoteStopTransactionRequest == null || remoteStopTransactionRequest.TransactionId <= 0)
            {
                string[] urlParts = apiCallerContext.Request.Path.Value.Split('/');
                string urlConnectorId = (urlParts.Length >= 5) ? urlParts[4] : "0";

                remoteStopTransactionRequest = new RemoteStopTransactionRequest();


                var charger = dbContext.TblChargers.FirstOrDefault(x => x.FCode == chargePointStatus.Id);
                var connector = dbContext.TblConnectorStatuses.FirstOrDefault(x => x.FConnectorId == int.Parse(urlConnectorId) && x.FChargerId == charger.FId);

                var chargeTags = dbContext.TblTransactions.Where(x => x.FChargerId == charger.FId && x.FConnectorId == connector.FId && x.FEndTime == null && x.FTransactionStatus == "Charging").FirstOrDefault();
                remoteStopTransactionRequest.TransactionId = chargeTags.FTransactionNo ?? 0;
            }
            string jsonResetRequest = JsonConvert.SerializeObject(remoteStopTransactionRequest);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "RemoteStopTransaction";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }

        private async Task ClearCache16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            ClearCacheRequest request = new ClearCacheRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<ClearCacheRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                request = new ClearCacheRequest();
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "ClearCache";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }

        private async Task ChangeAvailability16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            ChangeAvailabilityRequest request = new ChangeAvailabilityRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<ChangeAvailabilityRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string[] urlParts = apiCallerContext.Request.Path.Value.Split('/');
                string urlConnectorId = (urlParts.Length >= 5) ? urlParts[4] : "0";

                request = new ChangeAvailabilityRequest();
                request.Type = ChangeAvailabilityRequestType.Operative.ToString();
                request.ConnectorId = Convert.ToInt32(urlConnectorId);
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "ChangeAvailability";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }

        private async Task GetConfiguration16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            GetConfigurationRequest request = new GetConfigurationRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<GetConfigurationRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string _apiResult = "{\"status\": " + JsonConvert.ToString("Faulted") + "}";
                apiCallerContext.Response.StatusCode = 200;
                apiCallerContext.Response.ContentType = "application/json";
                await apiCallerContext.Response.WriteAsync(_apiResult);
                return;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "GetConfiguration";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }
        private async Task GetDiagnostics16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            GetDiagnosticsRequest request = new GetDiagnosticsRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<GetDiagnosticsRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string _apiResult = "{\"status\": " + JsonConvert.ToString("Faulted") + "}";
                apiCallerContext.Response.StatusCode = 200;
                apiCallerContext.Response.ContentType = "application/json";
                await apiCallerContext.Response.WriteAsync(_apiResult);
                return;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "GetDiagnostics";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }
        private async Task GetLocalListVersion16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            GetLocalListVersionRequest request = new GetLocalListVersionRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<GetLocalListVersionRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string _apiResult = "{\"status\": " + JsonConvert.ToString("Faulted") + "}";
                apiCallerContext.Response.StatusCode = 200;
                apiCallerContext.Response.ContentType = "application/json";
                await apiCallerContext.Response.WriteAsync(_apiResult);
                return;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "GetLocalListVersion";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }
        private async Task SendLocalList16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            SendLocalListRequest request = new SendLocalListRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<SendLocalListRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string _apiResult = "{\"status\": " + JsonConvert.ToString("Faulted") + "}";
                apiCallerContext.Response.StatusCode = 200;
                apiCallerContext.Response.ContentType = "application/json";
                await apiCallerContext.Response.WriteAsync(_apiResult);
                return;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "SendLocalList";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }
        private async Task SetChargingProfile16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            SetChargingProfileRequest request = new SetChargingProfileRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<SetChargingProfileRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string _apiResult = "{\"status\": " + JsonConvert.ToString("Faulted") + "}";
                apiCallerContext.Response.StatusCode = 200;
                apiCallerContext.Response.ContentType = "application/json";
                await apiCallerContext.Response.WriteAsync(_apiResult);
                return;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "SetChargingProfile";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }
        private async Task UpdateFirmware16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            UpdateFirmwareRequest request = new UpdateFirmwareRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<UpdateFirmwareRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string _apiResult = "{\"status\": " + JsonConvert.ToString("Faulted") + "}";
                apiCallerContext.Response.StatusCode = 200;
                apiCallerContext.Response.ContentType = "application/json";
                await apiCallerContext.Response.WriteAsync(_apiResult);
                return;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "UpdateFirmware";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }
        private async Task GetCompositeSchedule16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            GetCompositeScheduleRequest request = new GetCompositeScheduleRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<GetCompositeScheduleRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string _apiResult = "{\"status\": " + JsonConvert.ToString("Faulted") + "}";
                apiCallerContext.Response.StatusCode = 200;
                apiCallerContext.Response.ContentType = "application/json";
                await apiCallerContext.Response.WriteAsync(_apiResult);
                return;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "GetCompositeSchedule";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }
        private async Task ChangeConfiguration16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            ChangeConfigurationRequest request = new ChangeConfigurationRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<ChangeConfigurationRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string _apiResult = "{\"status\": " + JsonConvert.ToString("Faulted") + "}";
                apiCallerContext.Response.StatusCode = 200;
                apiCallerContext.Response.ContentType = "application/json";
                await apiCallerContext.Response.WriteAsync(_apiResult);
                return;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "ChangeConfiguration";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }
        private async Task ClearChargingProfile16(ChargePointStatus chargePointStatus, HttpContext apiCallerContext)
        {
            ILogger logger = _logFactory.CreateLogger("OCPPMiddleware.OCPP16");
            ClearChargingProfileRequest request = new ClearChargingProfileRequest();
            try
            {
                if (apiCallerContext.Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    apiCallerContext.Request.Body.Position = 0;
                }

                var requestBodyText = new StreamReader(apiCallerContext.Request.Body).ReadToEnd();
                request = JsonConvert.DeserializeObject<ClearChargingProfileRequest>(requestBodyText);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't rewind body stream. " + ex.Message);
            }

            if (request == null)
            {
                string _apiResult = "{\"status\": " + JsonConvert.ToString("Faulted") + "}";
                apiCallerContext.Response.StatusCode = 200;
                apiCallerContext.Response.ContentType = "application/json";
                await apiCallerContext.Response.WriteAsync(_apiResult);
                return;
            }

            string jsonResetRequest = JsonConvert.SerializeObject(request);

            OCPPMessage msgOut = new OCPPMessage();
            msgOut.MessageType = "2";
            msgOut.Action = "ClearChargingProfile";
            msgOut.UniqueId = Guid.NewGuid().ToString("N");
            msgOut.JsonPayload = jsonResetRequest;
            msgOut.TaskCompletionSource = new TaskCompletionSource<string>();

            // store HttpContext with MsgId for later answer processing (=> send anwer to API caller)
            _requestQueue.Add(msgOut.UniqueId, msgOut);

            // Send OCPP message with optional logging/dump
            await SendOcpp16Message(msgOut, logger, chargePointStatus);

            // Wait for asynchronous chargepoint response and processing
            string apiResult = await msgOut.TaskCompletionSource.Task;

            // 
            apiCallerContext.Response.StatusCode = 200;
            apiCallerContext.Response.ContentType = "application/json";
            await apiCallerContext.Response.WriteAsync(apiResult);
        }
        
        private async Task SendOcpp16Message(OCPPMessage msg, ILogger logger, ChargePointStatus chargePointStatus)
        {
            // Send raw outgoing messages to extensions
            _ = Task.Run(() =>
            {
                ProcessRawOutgoingMessageSinks(chargePointStatus.Protocol, chargePointStatus.Id, msg);
            });

            string ocppTextMessage = null;

            if (string.IsNullOrEmpty(msg.ErrorCode))
            {
                if (msg.MessageType == "2")
                {
                    // OCPP-Request
                    ocppTextMessage = string.Format("[{0},\"{1}\",\"{2}\",{3}]", msg.MessageType, msg.UniqueId, msg.Action, msg.JsonPayload);
                }
                else
                {
                    // OCPP-Response
                    ocppTextMessage = string.Format("[{0},\"{1}\",{2}]", msg.MessageType, msg.UniqueId, msg.JsonPayload);
                }
            }
            else
            {
                ocppTextMessage = string.Format("[{0},\"{1}\",\"{2}\",\"{3}\",{4}]", msg.MessageType, msg.UniqueId, msg.ErrorCode, msg.ErrorDescription, "{}");
            }
            logger.LogTrace("OCPPMiddleware.OCPP16 => SendOcppMessage: {0}", ocppTextMessage);

            if (string.IsNullOrEmpty(ocppTextMessage))
            {
                // invalid message
                ocppTextMessage = string.Format("[{0},\"{1}\",\"{2}\",\"{3}\",{4}]", "4", string.Empty, Messages_OCPP16.ErrorCodes.ProtocolError, string.Empty, "{}");
            }

            // write message (async) to dump directory
            _ = Task.Run(() =>
            {
                DumpMessage("ocpp16-out", ocppTextMessage);
            });

            byte[] binaryMessage = UTF8Encoding.UTF8.GetBytes(ocppTextMessage);
            await chargePointStatus.WebSocket.SendAsync(new ArraySegment<byte>(binaryMessage, 0, binaryMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
