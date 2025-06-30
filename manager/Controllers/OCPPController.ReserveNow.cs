using manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using manager.Messages_OCPP16;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace manager.Controllers
{
    public partial class OCPPController
    {
        public IActionResult ReserveNow()
        {
            OCPPViewModel model = new OCPPViewModel();
            model.ChargePoints = ChargerModel.GetChargers();
            return View(model);
        }

        public IActionResult GetReserveNowJson(int connectorid, string chargetag)
        {
            ReserveNowRequest request = new ReserveNowRequest();
            request.ConnectorId = connectorid;
            request.IdTag = chargetag;

            string json = JsonConvert.SerializeObject(request);

            return new JsonResult(json);
        }

        public async Task<IActionResult> ReserveNow2ChargePoint(string id, int connectorid, string chargetagid)
        {
            dynamic jsonObject = null;
            string jsonResult = null;

            _logger.LogTrace("ReserveNow: Request to restart chargepoint '{0}'", id);
            ReserveNowRequest request = new ReserveNowRequest();
            request.ConnectorId = connectorid;
            request.IdTag = chargetagid;

            try
            {
                string serverApiUrl = _config.GetValue<string>("ServerApiUrl");
                string apiKeyConfig = _config.GetValue<string>("ApiKey");
                using (var httpClient = new HttpClient())
                {
                    if (!serverApiUrl.EndsWith('/'))
                    {
                        serverApiUrl += "/";
                    }
                    Uri uri = new Uri(serverApiUrl);
                    uri = new Uri(uri, $"ReserveNow/{Uri.EscapeUriString(id)}/{connectorid}");
                    httpClient.Timeout = new TimeSpan(0, 0, 30); // use short timeout

                    // API-Key authentication?
                    if (!string.IsNullOrWhiteSpace(apiKeyConfig))
                    {
                        httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKeyConfig);
                    }
                    else
                    {
                        _logger.LogWarning("ReserveNow: No API-Key configured!");
                    }

                    //HttpResponseMessage response = await httpClient.GetAsync(uri,);
                    var json = JsonConvert.SerializeObject(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(uri, content);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        jsonResult = await response.Content.ReadAsStringAsync();
                        
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Chargepoint offline
                        // httpStatuscode = (int)HttpStatusCode.OK;
                        //resultContent = "The charging station is offline and cannot be restarted.";
                    }
                    else
                    {
                        _logger.LogError("ReserveNow: Result of API  request => httpStatus={0}", response.StatusCode);
                        //httpStatuscode = (int)HttpStatusCode.OK;
                        // resultContent = "An error has occurred.";
                    }
                }
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "ReserveNow: Error in API request => {0}", exp.Message);
                //httpStatuscode = (int)HttpStatusCode.OK;
                //resultContent = "An error has occurred.";
            }

            return new JsonResult(jsonResult);
        }
    }
}
