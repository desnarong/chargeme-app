using csms.Entities;
using csms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace csms.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _logger = logger;
            _config = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail(string Charger, string ConectorId)
        {

            var model = new ConnectorStatusDataModel();
            model.connectorStatusViewDatas = ChargePointModel.GetConnectorStatusViewDatas(Guid.Parse(Charger)).Where(x => x.Id == Guid.Parse(ConectorId)).ToList();
            return View(model);
        }

        public IActionResult GetChargeLogsTable(string ChargePoint, string ConectorId)
        {
            int rows = Convert.ToInt32(_config.GetValue<string>("MessagelogRow"));

            string draw = Request.Form["draw"][0];
            int startRec = Convert.ToInt32(Request.Form["start"][0]);
            int pageSize = Convert.ToInt32(Request.Form["length"][0]);
            string order = Request.Form["order[0][column]"][0];
            string orderDir = Request.Form["order[0][dir]"][0];
            
            var model = ChargePointModel.GetMessageLogDatasWithRows(Guid.Parse(ChargePoint), Guid.Parse(ConectorId), rows);
            switch (order)
            {
                case "0":
                    model = orderDir == "asc" ? model.OrderBy(x => x.LogState).ToList() : model.OrderByDescending(x => x.LogState).ToList();
                    break;
                case "1":
                    model = orderDir == "asc" ? model.OrderBy(x => x.LogType).ToList() : model.OrderByDescending(x => x.LogType).ToList();
                    break;
                case "2":
                    model = orderDir == "asc" ? model.OrderBy(x => x.ConnectorId).ToList() : model.OrderByDescending(x => x.ConnectorId).ToList();
                    break;
                case "3":
                    model = orderDir == "asc" ? model.OrderBy(x => x.Message).ToList() : model.OrderByDescending(x => x.Message).ToList();
                    break;
                case "4":
                    model = orderDir == "asc" ? model.OrderBy(x => x.LogDate).ToList() : model.OrderByDescending(x => x.LogDate).ToList();
                    break;
                case "5":
                    model = orderDir == "asc" ? model.OrderBy(x => x.Result).ToList() : model.OrderByDescending(x => x.Result).ToList();
                    break;
            }

            int totalRecords = model.Count;
            if (pageSize > -1)
            {
                model = model.Skip(startRec).Take(rows).ToList();
            }
            else
            {
                model = model.Skip(startRec).ToList();
            }

            return new JsonResult(new { draw = draw, iTotalRecords = totalRecords, iTotalDisplayRecords = totalRecords, data = model });
        }
        public async Task<IActionResult> Reset(string Id)
        {
            int httpStatuscode = (int)HttpStatusCode.OK;
            string resultContent = string.Empty;
            var chargePoint = ChargePointModel.GetChargePoint(Guid.Parse(Id));
            if (chargePoint != null)
            {

                string serverApiUrl = _config.GetValue<string>("ServerApiUrl");
                string apiKeyConfig = _config.GetValue<string>("ApiKey");
                if (!string.IsNullOrEmpty(serverApiUrl))
                {
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            if (!serverApiUrl.EndsWith('/'))
                            {
                                serverApiUrl += "/";
                            }
                            Uri uri = new Uri(serverApiUrl);
                            uri = new Uri(uri, $"Reset/{Uri.EscapeUriString(Id)}");
                            httpClient.Timeout = new TimeSpan(0, 0, 4); // use short timeout

                            // API-Key authentication?
                            if (!string.IsNullOrWhiteSpace(apiKeyConfig))
                            {
                                httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKeyConfig);
                            }
                            else
                            {
                                _logger.LogWarning("Reset: No API-Key configured!");
                            }

                            HttpResponseMessage response = await httpClient.GetAsync(uri);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                string jsonResult = await response.Content.ReadAsStringAsync();
                                if (!string.IsNullOrEmpty(jsonResult))
                                {
                                    try
                                    {
                                        dynamic jsonObject = JsonConvert.DeserializeObject(jsonResult);
                                        _logger.LogInformation("Reset: Result of API request is '{0}'", jsonResult);
                                        string status = jsonObject.status;
                                        switch (status)
                                        {
                                            case "Accepted":
                                                resultContent = "The charging station is being restarted.";
                                                break;
                                            case "Rejected":
                                                resultContent = "The charging station has rejected the request.";
                                                break;
                                            case "Scheduled":
                                                resultContent = "The charging station has scheduled the restart.";
                                                break;
                                            default:
                                                resultContent = string.Format("The charging station returned an unexpected result: '{0}'", status);
                                                break;
                                        }
                                    }
                                    catch (Exception exp)
                                    {
                                        _logger.LogError(exp, "Reset: Error in JSON result => {0}", exp.Message);
                                        httpStatuscode = (int)HttpStatusCode.OK;
                                        resultContent = "An error has occurred.";
                                    }
                                }
                                else
                                {
                                    _logger.LogError("Reset: Result of API request is empty");
                                    httpStatuscode = (int)HttpStatusCode.OK;
                                    resultContent = "An error has occurred.";
                                }
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                // Chargepoint offline
                                httpStatuscode = (int)HttpStatusCode.OK;
                                resultContent = "The charging station is offline and cannot be restarted.";
                            }
                            else
                            {
                                _logger.LogError("Reset: Result of API  request => httpStatus={0}", response.StatusCode);
                                httpStatuscode = (int)HttpStatusCode.OK;
                                resultContent = "An error has occurred.";
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        _logger.LogError(exp, "Reset: Error in API request => {0}", exp.Message);
                        httpStatuscode = (int)HttpStatusCode.OK;
                        resultContent = "An error has occurred.";
                    }
                }
            }
            else
            {
                _logger.LogWarning("Reset: Error loading charge point '{0}' from database", Id);
                httpStatuscode = (int)HttpStatusCode.OK;
                resultContent = "The charging station was not found.";
            }
            return StatusCode(httpStatuscode, resultContent);
        }

        public async Task<IActionResult> RemoteStartTransaction(string id, string connectorId)
        {
            int httpStatuscode = (int)HttpStatusCode.OK;
            string resultContent = string.Empty;
            
            _logger.LogTrace("RemoteStartTransaction: Request to unlock chargepoint '{0}'", id);
            var chargePoint = ChargePointModel.GetChargePoint(Guid.Parse(id));
            var station = StationInfoModel.GetStationInfo(chargePoint.FStationId);
            if (chargePoint != null)
            {
                string serverApiUrl = _config.GetValue<string>("ServerApiUrl");
                string apiKeyConfig = _config.GetValue<string>("ApiKey");
                if (!string.IsNullOrEmpty(serverApiUrl))
                {
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            if (!serverApiUrl.EndsWith('/'))
                            {
                                serverApiUrl += "/";
                            }
                            Uri uri = new Uri(serverApiUrl);
                            uri = new Uri(uri, $"RemoteStartTransaction/{Uri.EscapeUriString(id)}/{Uri.EscapeUriString(connectorId)}/{Uri.EscapeUriString(station.FRfid)}");
                            httpClient.Timeout = new TimeSpan(0, 0, 4); // use short timeout

                            // API-Key authentication?
                            if (!string.IsNullOrWhiteSpace(apiKeyConfig))
                            {
                                httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKeyConfig);
                            }
                            else
                            {
                                _logger.LogWarning("RemoteStartTransaction: No API-Key configured!");
                            }

                            HttpResponseMessage response = await httpClient.GetAsync(uri);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                string jsonResult = await response.Content.ReadAsStringAsync();
                                if (!string.IsNullOrEmpty(jsonResult))
                                {
                                    try
                                    {
                                        dynamic jsonObject = JsonConvert.DeserializeObject(jsonResult);
                                        _logger.LogInformation("RemoteStartTransaction: Result of API request is '{0}'", jsonResult);
                                        string status = jsonObject.status;
                                        switch (status)
                                        {
                                            case "Accepted":
                                                resultContent = "The charging station has been start.";
                                                break;
                                            case "Rejected":
                                                resultContent = "The charging station could NOT be start.";
                                                break;
                                            default:
                                                resultContent = string.Format("The charging station returned an unexpected result: '{0}'", status);
                                                break;
                                        }
                                    }
                                    catch (Exception exp)
                                    {
                                        _logger.LogError(exp, "RemoteStartTransaction: Error in JSON result => {0}", exp.Message);
                                        httpStatuscode = (int)HttpStatusCode.OK;
                                        resultContent = "An error has occurred.";
                                    }
                                }
                                else
                                {
                                    _logger.LogError("RemoteStartTransaction: Result of API request is empty");
                                    httpStatuscode = (int)HttpStatusCode.OK;
                                    resultContent = "An error has occurred.";
                                }
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                // Chargepoint offline
                                httpStatuscode = (int)HttpStatusCode.OK;
                                resultContent = "The charging station is offline and cannot be start.";
                            }
                            else
                            {
                                _logger.LogError("RemoteStartTransaction: Result of API  request => httpStatus={0}", response.StatusCode);
                                httpStatuscode = (int)HttpStatusCode.OK;
                                resultContent = "An error has occurred.";
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        _logger.LogError(exp, "RemoteStartTransaction: Error in API request => {0}", exp.Message);
                        httpStatuscode = (int)HttpStatusCode.OK;
                        resultContent = "An error has occurred.";
                    }
                }
            }
            else
            {
                _logger.LogWarning("RemoteStartTransaction: Error loading charge point '{0}' from database", id);
                httpStatuscode = (int)HttpStatusCode.OK;
                resultContent = "The charging station returned an unexpected result: '{0}'";
            }

            return StatusCode(httpStatuscode, resultContent);
        }

        public async Task<IActionResult> RemoteStopTransaction(string id, string connectorId)
        {
            int httpStatuscode = (int)HttpStatusCode.OK;
            string resultContent = string.Empty;

            _logger.LogTrace("RemoteStopTransaction: Request to unlock chargepoint '{0}'", id);
            var chargePoint = ChargePointModel.GetChargePoint(Guid.Parse(id));
            if (chargePoint != null)
            {
                string serverApiUrl = _config.GetValue<string>("ServerApiUrl");
                string apiKeyConfig = _config.GetValue<string>("ApiKey");
                if (!string.IsNullOrEmpty(serverApiUrl))
                {
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            if (!serverApiUrl.EndsWith('/'))
                            {
                                serverApiUrl += "/";
                            }
                            Uri uri = new Uri(serverApiUrl);
                            uri = new Uri(uri, $"RemoteStopTransaction/{Uri.EscapeUriString(id)}/{Uri.EscapeUriString(connectorId)}");
                            httpClient.Timeout = new TimeSpan(0, 0, 4); // use short timeout

                            // API-Key authentication?
                            if (!string.IsNullOrWhiteSpace(apiKeyConfig))
                            {
                                httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKeyConfig);
                            }
                            else
                            {
                                _logger.LogWarning("RemoteStopTransaction: No API-Key configured!");
                            }

                            HttpResponseMessage response = await httpClient.GetAsync(uri);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                string jsonResult = await response.Content.ReadAsStringAsync();
                                if (!string.IsNullOrEmpty(jsonResult))
                                {
                                    try
                                    {
                                        dynamic jsonObject = JsonConvert.DeserializeObject(jsonResult);
                                        _logger.LogInformation("RemoteStopTransaction: Result of API request is '{0}'", jsonResult);
                                        string status = jsonObject.status;
                                        switch (status)
                                        {
                                            case "Accepted":
                                                resultContent = "The charging station has been stop.";
                                                break;
                                            case "Rejected":
                                                resultContent = "The charging station could NOT be stop.";
                                                break;
                                            default:
                                                resultContent = string.Format("The charging station returned an unexpected result: '{0}'", status);
                                                break;
                                        }
                                    }
                                    catch (Exception exp)
                                    {
                                        _logger.LogError(exp, "RemoteStopTransaction: Error in JSON result => {0}", exp.Message);
                                        httpStatuscode = (int)HttpStatusCode.OK;
                                        resultContent = "An error has occurred.";
                                    }
                                }
                                else
                                {
                                    _logger.LogError("RemoteStopTransaction: Result of API request is empty");
                                    httpStatuscode = (int)HttpStatusCode.OK;
                                    resultContent = "An error has occurred.";
                                }
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                // Chargepoint offline
                                httpStatuscode = (int)HttpStatusCode.OK;
                                resultContent = "The charging station is offline and cannot be stop.";
                            }
                            else
                            {
                                _logger.LogError("RemoteStopTransaction: Result of API  request => httpStatus={0}", response.StatusCode);
                                httpStatuscode = (int)HttpStatusCode.OK;
                                resultContent = "An error has occurred.";
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        _logger.LogError(exp, "RemoteStopTransaction: Error in API request => {0}", exp.Message);
                        httpStatuscode = (int)HttpStatusCode.OK;
                        resultContent = "An error has occurred.";
                    }
                }
            }
            else
            {
                _logger.LogWarning("RemoteStopTransaction: Error loading charge point '{0}' from database", id);
                httpStatuscode = (int)HttpStatusCode.OK;
                resultContent = "The charging station was not found.";
            }

            return StatusCode(httpStatuscode, resultContent);
        }
    }
}