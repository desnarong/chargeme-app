using manager.Messages_OCPP16;
using manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        public IActionResult CancelReservation()
        {
            
            OCPPViewModel model = new OCPPViewModel();
            model.ChargePoints = ChargerModel.GetChargers();
            return View(model);
        }

        public IActionResult GetCancelReservationJson(string id)
        {
            CancelReservationRequest request = new CancelReservationRequest();
            if (!string.IsNullOrEmpty(id))
                request.ReservationId = id;
            else
                request.ReservationId = "0";

            string json = JsonConvert.SerializeObject(request);

            return new JsonResult(json);
        }

        public async Task<IActionResult> CancelReservation2ChargePoint(string id, int connectorid, int reservationId)
        {
            dynamic jsonObject = null;
            string jsonResult = null;
            if (reservationId > 0)
            {
                CancelReservationRequest request = new CancelReservationRequest();
                request.ReservationId = reservationId.ToString();

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
                        uri = new Uri(uri, $"CancelReservation/{Uri.EscapeUriString(id)}/{connectorid}");
                        httpClient.Timeout = new TimeSpan(0, 0, 30); // use short timeout

                        // API-Key authentication?
                        if (!string.IsNullOrWhiteSpace(apiKeyConfig))
                        {
                            httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKeyConfig);
                        }
                        else
                        {
                            _logger.LogWarning("CancelReservation: No API-Key configured!");
                        }

                        //HttpResponseMessage response = await httpClient.GetAsync(uri,);
                        var json = JsonConvert.SerializeObject(request);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await httpClient.PostAsync(uri, content);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonResult = await response.Content.ReadAsStringAsync();
                        }
                    }
                }
                catch (Exception exp)
                {
                }
            }

            return new JsonResult(jsonResult);
        }
    }
}
