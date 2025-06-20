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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OCPP.Core.Server.Entities;
using OCPP.Core.Server.Messages_OCPP16;
using OCPP.Core.Server.Models;

namespace OCPP.Core.Server
{
    public partial class ControllerOCPP16
    {
        public string HandleStopTransaction(OCPPMessage msgIn, OCPPMessage msgOut)
        {
            string errorCode = null;
            StopTransactionResponse stopTransactionResponse = new StopTransactionResponse();
            stopTransactionResponse.IdTagInfo = new IdTagInfo();
            StopTransactionRequest stopTransactionRequest = DeserializeMessage<StopTransactionRequest>(msgIn);

            try
            {
                Logger.LogTrace("Processing stopTransaction request...");
                Logger.LogTrace("StopTransaction => Message deserialized");

                string idTag = CleanChargeTagId(stopTransactionRequest.IdTag, Logger);
                TblChargingTag ct = DbContext.TblChargingTags.FirstOrDefault(x => x.FCode == idTag);

                if (string.IsNullOrWhiteSpace(idTag))
                {
                    // no RFID-Tag => accept request
                    stopTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Accepted;
                    Logger.LogInformation("StopTransaction => no charge tag => Status: {0}", stopTransactionResponse.IdTagInfo.Status);
                }
                else
                {
                    stopTransactionResponse.IdTagInfo = new IdTagInfo();
                    stopTransactionResponse.IdTagInfo.ExpiryDate = MaxExpiryDate;

                    try
                    {
                        if (ct != null)
                        {
                            if (ct.FExpiryDate.HasValue) stopTransactionResponse.IdTagInfo.ExpiryDate = ct.FExpiryDate.Value;
                            //stopTransactionResponse.IdTagInfo.ParentIdTag = ct.ParentTagId;
                            if (ct.FBlocked.HasValue && ct.FBlocked.Value == 'Y')
                            {
                                stopTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Blocked;
                            }
                            else if (ct.FExpiryDate.HasValue && ct.FExpiryDate.Value < DateTime.UtcNow)
                            {
                                stopTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Expired;
                            }
                            else
                            {
                                stopTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Accepted;
                            }
                        }
                        else
                        {
                            stopTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Invalid;
                        }

                        Logger.LogInformation("StopTransaction => RFID-tag='{0}' => Status: {1}", idTag, stopTransactionResponse.IdTagInfo.Status);

                    }
                    catch (Exception exp)
                    {
                        Logger.LogError(exp, "StopTransaction => Exception reading charge tag ({0}): {1}", idTag, exp.Message);
                        stopTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Invalid;
                    }
                }

                if (stopTransactionResponse.IdTagInfo.Status == IdTagInfoStatus.Accepted)
                {
                    try
                    {
                        var charger = DbContext.TblChargers.Where(x => x.FShortName == ChargePointStatus.Id).FirstOrDefault() ?? new TblCharger();
                        TblTransaction transaction = DbContext.TblTransactions.FirstOrDefault(x => x.FTransactionNo == stopTransactionRequest.TransactionId);
                        var connectorStatus = DbContext.TblConnectorStatuses.Where(x => x.FChargerId == charger.FId && x.FId == transaction.FConnectorId).FirstOrDefault();
                        if(connectorStatus != null)
                        {
                            if (transaction != null &&
                            transaction.FChargerId == charger.FId &&
                            !transaction.FEndTime.HasValue)
                            {

                                if (connectorStatus.FConnectorId > 0)
                                {
                                    // Update meter value in db connector status 
                                    UpdateConnectorStatus((connectorStatus.FConnectorId ?? 0), ConnectorStatusEnum.Available.ToString(), null, (double)stopTransactionRequest.MeterStop / 1000, null, null, stopTransactionRequest.Timestamp);
                                }

                                // check current tag against start tag
                                bool valid = true;
                                //if (transaction.StartTagId.Equals(ct.TagId, StringComparison.InvariantCultureIgnoreCase))
                                //{
                                //    // tags are different => same group?
                                //    ChargeTag startTag = DbContext.Find<ChargeTag>(transaction.StartTagId);
                                //    if (startTag != null)
                                //    {
                                //        if (!string.Equals(startTag.ParentTagId, stopTransactionResponse.IdTagInfo.ParentIdTag, StringComparison.InvariantCultureIgnoreCase))
                                //        {
                                //            Logger.LogInformation("StopTransaction => Start-Tag ('{0}') and End-Tag ('{1}') do not match: Invalid!", transaction.StartTagId, idTag);
                                //            stopTransactionResponse.IdTagInfo.Status = IdTagInfoStatus.Invalid;
                                //            valid = false;
                                //        }
                                //        else
                                //        {
                                //            Logger.LogInformation("StopTransaction => Different RFID-Tags but matching group ('{0}')", stopTransactionResponse.IdTagInfo.ParentIdTag);
                                //        }
                                //    }
                                //    else
                                //    {
                                //        Logger.LogError("StopTransaction => Start-Tag not found: '{0}'", transaction.StartTagId);
                                //        // assume "valid" and allow to end the transaction
                                //    }
                                //}

                                if (valid)
                                {
                                    //transaction.StopTagId = ct.TagId;
                                    transaction.FMeterEnd = stopTransactionRequest.MeterStop / 1000; // Meter value here is always Wh
                                    transaction.FEndResult = stopTransactionRequest.Reason.ToString();
                                    transaction.FEndTime = stopTransactionRequest.Timestamp.DateTime.ToLocalTime();
                                    DbContext.TblTransactions.Update(transaction);
                                    DbContext.SaveChanges();
                                }
                            }
                            else
                            {
                                Logger.LogError("StopTransaction => Unknown or not matching transaction: id={0} / chargepoint={1} / tag={2}", stopTransactionRequest.TransactionId, ChargePointStatus?.Id, idTag);
                                WriteMessageLog(ChargePointStatus?.Id, connectorStatus.FConnectorId, "CP", "Request", msgIn.Action, string.Format("UnknownTransaction:ID={0}/Meter={1}", stopTransactionRequest.TransactionId, stopTransactionRequest.MeterStop), errorCode);
                                errorCode = ErrorCodes.PropertyConstraintViolation;
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        Logger.LogError(exp, "StopTransaction => Exception writing transaction: chargepoint={0} / tag={1}", ChargePointStatus?.Id, idTag);
                        errorCode = ErrorCodes.InternalError;
                    }
                }

                msgOut.JsonPayload = JsonConvert.SerializeObject(stopTransactionResponse);
                Logger.LogTrace("StopTransaction => Response serialized");
            }
            catch (Exception exp)
            {
                Logger.LogError(exp, "StopTransaction => Exception: {0}", exp.Message);
                errorCode = ErrorCodes.FormationViolation;
            }

            //Request from charg point
            WriteMessageLog(ChargePointStatus?.Id, null, "CP", "Request", msgIn.Action, $"TransactionId=>{stopTransactionRequest.TransactionId}", errorCode);
            //Response to charg point
            WriteMessageLog(ChargePointStatus?.Id, null, "SV", "Response", msgIn.Action, stopTransactionResponse.IdTagInfo?.Status.ToString(), errorCode);
            return errorCode;
        }
    }
}
