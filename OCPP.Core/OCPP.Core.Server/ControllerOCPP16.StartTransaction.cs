/*
 * OCPP.Core - https://github.com/dallmann-consulting/OCPP.Core
 * Copyright (C) 2020-2021 dallmann consulting GmbH.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OCPP.Core.Server.Entities;
using OCPP.Core.Server.Messages_OCPP16;
using OCPP.Core.Server.Models;

namespace OCPP.Core.Server
{
    public partial class ControllerOCPP16
    {
        public async Task<string> HandleStartTransaction(OCPPMessage msgIn, OCPPMessage msgOut)
        {
            string errorCode = null;
            StartTransactionResponse startTransactionResponse = new StartTransactionResponse();
            StartTransactionRequest startTransactionRequest = DeserializeMessage<StartTransactionRequest>(msgIn);

            int connectorId = -1;
            bool denyConcurrentTx = Configuration.GetValue<bool>("DenyConcurrentTx", false);

            try
            {
                Logger.LogTrace("Processing startTransaction request...");
               
                Logger.LogTrace("StartTransaction => Message deserialized");

                string idTag = CleanChargeTagId(startTransactionRequest.IdTag, Logger);
                TblChargingTag ct = DbContext.TblChargingTags.FirstOrDefault(x => x.FCode == idTag);
                connectorId = startTransactionRequest.ConnectorId;

                startTransactionResponse.IdTagInfo.ParentIdTag = string.Empty;
                startTransactionResponse.IdTagInfo.ExpiryDate = MaxExpiryDate;

                if (string.IsNullOrWhiteSpace(idTag))
                {
                    // no RFID-Tag => accept request
                    startTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Accepted;
                    Logger.LogInformation("StartTransaction => no charge tag => Status: {0}", startTransactionResponse.IdTagInfo.Status);
                }
                else
                {
                    try
                    {
                        
                        if (ct != null)
                        {
                            if (ct.FExpiryDate.HasValue) startTransactionResponse.IdTagInfo.ExpiryDate = ct.FExpiryDate.Value;
                            //startTransactionResponse.IdTagInfo.ParentIdTag = ct.FParentTagId;
                            if (ct.FBlocked.HasValue && ct.FBlocked.Value == 'Y')
                            {
                                startTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Blocked;
                            }
                            else if (ct.FExpiryDate.HasValue && ct.FExpiryDate.Value < DateTime.UtcNow)
                            {
                                startTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Expired;
                            }
                            else
                            {
                                startTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Accepted;

                                if (denyConcurrentTx)
                                {
                                    // Check that no open transaction with this idTag exists
                                    //.Where(t => !t.FEndTime.HasValue && t.StartTagId == ct.TagId)
                                    TblTransaction tx = DbContext.TblTransactions
                                        .Where(t => !t.FEndTime.HasValue)
                                        .OrderByDescending(t => t.FId)
                                        .FirstOrDefault();

                                    if (tx != null)
                                    {
                                        startTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.ConcurrentTx;
                                    }
                                }
                            }
                        }
                        else
                        {
                            startTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Invalid;
                        }

                        Logger.LogInformation("StartTransaction => Charge tag='{0}' => Status: {1}", idTag, startTransactionResponse.IdTagInfo.Status);
                    }
                    catch (Exception exp)
                    {
                        Logger.LogError(exp, "StartTransaction => Exception reading charge tag ({0}): {1}", idTag, exp.Message);
                        startTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Invalid;
                    }
                }

                //if (connectorId > 0)
                //{
                //    // Update meter value in db connector status 
                //    UpdateConnectorStatus(connectorId, ConnectorStatusEnum.Occupied.ToString(), startTransactionRequest.Timestamp, (double)startTransactionRequest.MeterStart / 1000, startTransactionRequest.Timestamp);
                //}

                if (startTransactionResponse.IdTagInfo.Status == IdTagInfoStatus.Accepted)
                {
                    try
                    {
                        var chargePoint = DbContext.TblChargers.AsNoTracking().Where(x => x.FShortName == ChargePointStatus.Id).FirstOrDefault() ?? new TblCharger();
                        //var dd = DbContext.TblConnectorStatuses.Where(x => x.FChargerId == chargePoint.FId && x.FConnectorId == startTransactionRequest.ConnectorId).ToList();
                        var connector = DbContext.TblConnectorStatuses.Where(x => x.FChargerId == chargePoint.FId && x.FConnectorId == startTransactionRequest.ConnectorId).FirstOrDefault();
                        if(connector != null)
                        {
                            var trans = DbContext.TblTransactions.AsNoTracking().FirstOrDefault(x => x.FId == connector.FTransactionId);
                            
                            if(trans == null)
                            {
                                trans = new TblTransaction();
                                trans.FCardId = ct.FId;
                                trans.FChargerId = chargePoint.FId;
                                trans.FConnectorId = connector.FId;
                                trans.FStartTime = startTransactionRequest.Timestamp.DateTime.ToUniversalTime();
                                trans.FMeterStart = startTransactionRequest.MeterStart / 1000; // Meter value here is always Wh
                                trans.FStartResult = startTransactionResponse.IdTagInfo.Status.ToString();
                                trans.FTransactionStatus = "Charging";
                                trans.FStatus = 'Y';
                                trans.FMeterEnd = 0;
                                trans.FStationId = chargePoint.FStationId;
                                trans.FUserId = Guid.Empty;
                                trans.FTransactionNo = long.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
                                DbContext.TblTransactions.Add(trans);
                                await DbContext.SaveChangesAsync();

                                connector.FTransactionId = trans.FId;
                                DbContext.TblConnectorStatuses.Update(connector);
                                await DbContext.SaveChangesAsync();
                            }
                            else
                            {
                                trans.FStartTime = startTransactionRequest.Timestamp.DateTime.ToUniversalTime();
                                trans.FMeterStart = startTransactionRequest.MeterStart / 1000; // Meter value here is always Wh
                                trans.FStartResult = startTransactionResponse.IdTagInfo.Status.ToString();
                                trans.FTransactionStatus = "Charging";
                                DbContext.TblTransactions.Update(trans);
                                await DbContext.SaveChangesAsync();
                            }

                            // Return DB-ID as transaction ID
                            startTransactionResponse.TransactionId = trans.FTransactionNo ?? 0;
                        }
                    }
                    catch (Exception exp)
                    {
                        Logger.LogError(exp, "StartTransaction => Exception writing transaction: chargepoint={0} / tag={1}", ChargePointStatus?.Id, idTag);
                        errorCode = ErrorCodes.InternalError;
                    }
                }

                msgOut.JsonPayload = JsonConvert.SerializeObject(startTransactionResponse);
                Logger.LogTrace("StartTransaction => Response serialized");
            }
            catch (Exception exp)
            {
                Logger.LogError(exp, "StartTransaction => Exception: {0}", exp.Message);
                errorCode = ErrorCodes.FormationViolation;
            }

            WriteMessageLog(ChargePointStatus?.Id, connectorId, "CP", "Request", msgIn.Action, $"TagID={startTransactionRequest.IdTag}", errorCode);
            WriteMessageLog(ChargePointStatus?.Id, connectorId, "SV", "Response", msgIn.Action, $"TransactionID={startTransactionResponse.TransactionId} / Status={startTransactionResponse.IdTagInfo?.Status.ToString()}", errorCode);
            return errorCode;
        }
    }
}
