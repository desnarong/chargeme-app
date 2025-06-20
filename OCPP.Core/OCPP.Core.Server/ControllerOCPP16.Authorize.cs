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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OCPP.Core.Server.Entities;
using OCPP.Core.Server.Messages_OCPP16;

namespace OCPP.Core.Server
{
    public partial class ControllerOCPP16
    {
        public string HandleAuthorize(OCPPMessage msgIn, OCPPMessage msgOut)
        {
            string errorCode = null;
            AuthorizeResponse authorizeResponse = new AuthorizeResponse();

            string idTag = null;
            try
            {
                Logger.LogTrace("Processing authorize request...");
                AuthorizeRequest authorizeRequest = DeserializeMessage<AuthorizeRequest>(msgIn);
                Logger.LogTrace("Authorize => Message deserialized");
                idTag = CleanChargeTagId(authorizeRequest.IdTag, Logger);

                authorizeResponse.IdTagInfo.ParentIdTag = string.Empty;
                authorizeResponse.IdTagInfo.ExpiryDate = DateTimeOffset.Now.AddMinutes(5);   // default: 5 minutes
                try
                {
                    TblChargingTag ct = DbContext.TblChargingTags.FirstOrDefault(x => x.FCode == idTag);
                    if (ct != null)
                    {
                        if (ct.FExpiryDate.HasValue)
                        {
                            authorizeResponse.IdTagInfo.ExpiryDate = ct.FExpiryDate.Value;
                        }
                        //authorizeResponse.IdTagInfo.ParentIdTag = ct.FParentTagId;
                        if (ct.FBlocked.HasValue && ct.FBlocked.Value == 'Y')
                        {
                            authorizeResponse.IdTagInfo.Status = IdTagInfoStatus.Blocked;
                        }
                        else if (ct.FExpiryDate.HasValue && ct.FExpiryDate.Value < DateTime.UtcNow)
                        {
                            authorizeResponse.IdTagInfo.Status = IdTagInfoStatus.Expired;
                        }
                        else
                        {
                            authorizeResponse.IdTagInfo.Status = IdTagInfoStatus.Accepted;
                        }
                    }
                    else
                    {
                        authorizeResponse.IdTagInfo.Status = IdTagInfoStatus.Invalid;
                    }
                    DbContext.Update<TblChargingTag>(ct);
                    DbContext.SaveChanges();
                    Logger.LogInformation("Authorize => Status: {0}", authorizeResponse.IdTagInfo.Status);
                }
                catch (Exception exp)
                {
                    Logger.LogError(exp, "Authorize => Exception reading charge tag ({0}): {1}", idTag, exp.Message);
                    authorizeResponse.IdTagInfo.Status = IdTagInfoStatus.Invalid;
                }

                msgOut.JsonPayload = JsonConvert.SerializeObject(authorizeResponse);
                Logger.LogTrace("Authorize => Response serialized");
            }
            catch (Exception exp)
            {
                Logger.LogError(exp, "Authorize => Exception: {0}", exp.Message);
                errorCode = ErrorCodes.FormationViolation;
            }

            //form cp
            WriteMessageLog(ChargePointStatus?.Id, null, "CP", "Request", msgIn.Action, $"TagID=>{idTag}", errorCode);
            //to cp
            WriteMessageLog(ChargePointStatus?.Id, null, "SV", "Response", msgIn.Action, $"'{idTag}'=>{authorizeResponse.IdTagInfo?.Status}", errorCode);
            return errorCode;
        }
    }
}
