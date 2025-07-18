using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OCPP.Core.Server.Entities;
using OCPP.Core.Server.Messages_OCPP16;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPP.Core.Server
{
    public partial class ControllerOCPP16
    {
        public string HandleRemoteStopTransaction(OCPPMessage msgIn, OCPPMessage msgOut)
        {
            string errorCode = null;
            RemoteStopTransactionRequest remoteStopTransactionRequest = new RemoteStopTransactionRequest();

            int connectorId = 0;
            bool msgWritten = false;

            try
            {
                StatusNotificationRequest statusNotificationRequest = JsonConvert.DeserializeObject<StatusNotificationRequest>(msgIn.JsonPayload);
                connectorId = statusNotificationRequest.ConnectorId;

                if (statusNotificationRequest.Status != StatusNotificationRequestStatus.Finishing)
                {
                    msgOut = null;
                    return errorCode;
                }

                var chargePoint = DbContext.TblChargers.Where(x => x.FShortName == ChargePointStatus.Id).FirstOrDefault() ?? new TblCharger();
                var connector = DbContext.TblConnectorStatuses.Where(x => x.FChargerId == chargePoint.FId && x.FConnectorId == connectorId).FirstOrDefault();
                if(connector != null)
                {
                    TblTransaction transaction = DbContext.TblTransactions.Where(x => x.FChargerId == chargePoint.FId && x.FConnectorId == connector.FId &&
                !x.FEndTime.HasValue).FirstOrDefault();

                    if (transaction != null)
                    {
                        TblChargingTag chargeTags = DbContext.TblChargingTags.Where(x => x.FChargerId == chargePoint.FId && x.FAuthorize == 'Y').FirstOrDefault();
                        chargeTags.FAuthorize = 'N';
                        chargeTags.FChargerId = Guid.Empty;
                        DbContext.Update<TblChargingTag>(chargeTags);
                        DbContext.SaveChanges();

                        remoteStopTransactionRequest.TransactionId = transaction.FTransactionNo ?? 0;
                        Logger.LogInformation("RemoteStopTransaction => Save ConnectorStatus: ID={0} / Connector={1} / Meter={2}", ChargePointStatus.Id, connectorId, 0);

                        msgOut.JsonPayload = JsonConvert.SerializeObject(remoteStopTransactionRequest);
                        Logger.LogTrace("RemoteStopTransaction => Response serialized");
                    }
                    else
                    {
                        if (msgOut.TaskCompletionSource != null)
                        {
                            // Set API response as TaskCompletion-result
                            string apiResult = "{\"status\": " + JsonConvert.ToString("Rejected") + "}";
                            Logger.LogTrace("HandleReset => API response: {0}", apiResult);
                            msgIn.Action = "RemoteStopTransaction";
                            msgOut.TaskCompletionSource.SetResult(apiResult);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                try
                {
                    RemoteStopTransactionResponse remoteStopTransactionResponse = JsonConvert.DeserializeObject<RemoteStopTransactionResponse>(msgIn.JsonPayload);
                    if (msgOut.TaskCompletionSource != null)
                    {
                        // Set API response as TaskCompletion-result
                        string apiResult = "{\"status\": " + JsonConvert.ToString(remoteStopTransactionResponse.Status.ToString()) + "}";
                        Logger.LogTrace("HandleReset => API response: {0}", apiResult);
                        msgIn.Action = "RemoteStopTransaction";
                        msgOut.TaskCompletionSource.SetResult(apiResult);
                    }
                }
                catch (Exception exp1)
                {
                    Logger.LogError(exp, "RemoteStopTransaction => ChargePoint={0} / ConnectorId: {1} / Exception: {2}", ChargePointStatus.Id, connectorId, exp1.Message);
                    errorCode = ErrorCodes.InternalError;
                }
            }

            if (!msgWritten)
            {
                if (!string.IsNullOrEmpty(msgIn.Action))
                    WriteMessageLog(ChargePointStatus.Id, connectorId, "SV", "Request", msgIn.Action, null, errorCode);
            }
            return errorCode;
        }
    }
}
