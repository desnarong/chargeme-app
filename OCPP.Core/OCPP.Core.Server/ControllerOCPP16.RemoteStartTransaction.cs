using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OCPP.Core.Server.Messages_OCPP16;

namespace OCPP.Core.Server
{
    public partial class ControllerOCPP16
    {
        public string HandleRemoteStartTransaction(OCPPMessage msgIn, OCPPMessage msgOut)
        {
            string errorCode = null;
            RemoteStartTransactionRequest remoteStartTransactionRequest = new RemoteStartTransactionRequest();

            int connectorId = 0;
            bool msgWritten = false;

            try
            {
                StatusNotificationRequest statusNotificationRequest = JsonConvert.DeserializeObject<StatusNotificationRequest>(msgIn.JsonPayload);
                connectorId = statusNotificationRequest.ConnectorId;
                remoteStartTransactionRequest.ConnectorId = Convert.ToInt32(msgIn.ConnectorId);
                remoteStartTransactionRequest.IdTag = Configuration.GetSection("TagIDTest").Value;

                Logger.LogInformation("RemoteStartTransaction => Save ConnectorStatus: ID={0} / Connector={1} / Meter={2}", ChargePointStatus.Id, connectorId, 0);

                msgOut.JsonPayload = JsonConvert.SerializeObject(remoteStartTransactionRequest);

                Logger.LogTrace("RemoteStartTransaction => Response serialized Data:"+ msgOut.JsonPayload);
                
            }
            catch (Exception exp)
            {
                try
                {
                    RemoteStartTransactionResponse remoteStopTransactionResponse = JsonConvert.DeserializeObject<RemoteStartTransactionResponse>(msgIn.JsonPayload);
                    if (msgOut.TaskCompletionSource != null)
                    {
                        // Set API response as TaskCompletion-result
                        string apiResult = "{\"status\": " + JsonConvert.ToString(remoteStopTransactionResponse.Status.ToString()) + "}";
                        Logger.LogTrace("HandleReset => API response: {0}", apiResult);
                        
                        msgOut.TaskCompletionSource.SetResult(apiResult);
                    }
                }
                catch (Exception exp1)
                {
                    Logger.LogError(exp, "RemoteStartTransaction => ChargePoint={0} / ConnectorId: {1} / Exception: {2}", ChargePointStatus.Id, connectorId, exp1.Message);
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
