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
        public IActionResult ClearChargingProfile()
        {
            OCPPViewModel model = new OCPPViewModel();
            model.ChargePoints = ChargerModel.GetChargers();
            model.ChargingProfilePurposes = Enum.GetValues(typeof(ChargingProfilePurpose)).Cast<ChargingProfilePurpose>().ToList();
            return View(model);
        }

        public IActionResult GetClearChargingProfileJson(int connectorId, string chargingprofilepurpose, int stacklevel, int id)
        {
            ClearChargingProfileRequest request = new ClearChargingProfileRequest();
            request.ConnectorId = connectorId;
            request.StackLevel = stacklevel;
            request.Id = id;
            Enum.TryParse(chargingprofilepurpose, out ChargingProfilePurpose myStatus);
            request.ChargingProfilePurpose = myStatus;

            string json = JsonConvert.SerializeObject(request);

            return new JsonResult(json);
        }

        public async Task<IActionResult> ClearChargingProfile2ChargePoint(string id, int connectorId, string chargingprofilepurpose, int stacklevel, int chargingid)
        {
            dynamic jsonObject = null;
            string jsonResult = null;
            ClearChargingProfileRequest request = new ClearChargingProfileRequest();
            request.ConnectorId = connectorId;
            request.StackLevel = stacklevel;
            request.Id = chargingid;
            Enum.TryParse(chargingprofilepurpose, out ChargingProfilePurpose myStatus);
            request.ChargingProfilePurpose = myStatus;

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
                    uri = new Uri(uri, $"ClearChargingProfile/{Uri.EscapeUriString(id)}");
                    httpClient.Timeout = new TimeSpan(0, 0, 30); // use short timeout

                    // API-Key authentication?
                    if (!string.IsNullOrWhiteSpace(apiKeyConfig))
                    {
                        httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKeyConfig);
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

            return new JsonResult(jsonResult);
        }
    }
}
