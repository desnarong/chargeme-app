using csms.Hubs;
using csms.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Net.WebSockets;

namespace csms.Services
{
    public class ChargePointService: IHostedService
    {
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        private CancellationTokenSource _ts;
        private CancellationTokenSource _tslog;
        protected ILogger Logger { get; set; }
        private IConfiguration _config;
        private IHubContext<ChargePointHub> _hub;
        public ChargePointService(IConfiguration configuration, ILoggerFactory loggerFactory, IHubContext<ChargePointHub> hub)
        {
            try
            {
                _config = configuration;
                Logger = loggerFactory.CreateLogger<ChargePointService>();
                _hub= hub;
            }
            catch (Exception ex)
            {
               
            }
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            #region delete log
            _tslog = new CancellationTokenSource();
            _ = Task.Factory.StartNew(async () =>
            {
                var MessagelogDay = Convert.ToInt32(_config.GetValue<string>("MessagelogDay")) * (-1);
                while (true)
                {
                    if (_tslog.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    try
                    {
                        var connectors = ChargePointModel.GetConnectorStatus();
                        foreach (var item in connectors)
                        {
                            var logs = ChargePointModel.GetMessageLogs(item.FChargerId, item.FId).OrderByDescending(x => x.FDate).ToList();
                            logs = logs.Where(x => x.FDate < DateTime.UtcNow.AddDays(MessagelogDay)).ToList();
                            ChargePointModel.DeleteMessgaeLog(logs);
                        }
                    }
                    catch { }
                    await Task.Delay(60000);
                }
            });
            #endregion

            #region Load online status from OCPP server
            string serverApiUrl = _config.GetValue<string>("ServerApiUrl");
            string apiKeyConfig = _config.GetValue<string>("ApiKey");
            int HeartbeatTimeout = Convert.ToInt32(_config.GetValue<string>("HeartbeatTimeout"));

            _hub.Clients.All.SendAsync("ChargePointMessage", JsonConvert.SerializeObject(ChargePointModel.GetConnectorStatusViews()));

            if (!string.IsNullOrEmpty(serverApiUrl))
            {
                _ts = new CancellationTokenSource();
                _ = Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (_ts.Token.IsCancellationRequested)
                        {
                            break;
                        }

                        bool serverError = false;
                        var connectorstatus = ChargePointModel.GetConnectorStatusViews();
                        //var connectorstatus = new List<ConnectorStatusViewData>(); //ChargePointModel.GetConnectorStatusViewDatas();
                        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                        try
                        {
                            ChargePointStatus[] onlineStatusList = null;

                            using (var httpClient = new HttpClient())
                            {
                                if (!serverApiUrl.EndsWith('/'))
                                {
                                    serverApiUrl += "/";
                                }
                                Uri uri = new Uri(serverApiUrl);
                                uri = new Uri(uri, "Status");
                                httpClient.Timeout = new TimeSpan(0, 0, Convert.ToInt32(_config.GetValue<string>("HttpTimeout"))); // use short timeout

                                // API-Key authentication?
                                if (!string.IsNullOrWhiteSpace(apiKeyConfig))
                                {
                                    httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKeyConfig);
                                }
                                else
                                {
                                    Logger.LogWarning("Index: No API-Key configured!");
                                }

                                HttpResponseMessage response = await httpClient.GetAsync(uri);
                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    string jsonData = await response.Content.ReadAsStringAsync();
                                    if (!string.IsNullOrEmpty(jsonData))
                                    {
                                        onlineStatusList = JsonConvert.DeserializeObject<ChargePointStatus[]>(jsonData);

                                        foreach (var item in connectorstatus)
                                        {
                                            var status = onlineStatusList.Where(x => x.Id == item.ChargerCode).ToList();
                                            if (status.Any())
                                            {
                                                var charger = status.FirstOrDefault();
                                                item.WebSocketStatus = status.FirstOrDefault().WebSocketStatus;

                                                if (status.FirstOrDefault().OnlineConnectors.ContainsKey(item.ConnectorId ?? 0))
                                                {
                                                    var connect = status.FirstOrDefault().OnlineConnectors[item.ConnectorId ?? 0];
                                                    //item.LastMeter = connect.MeterKWH;
                                                    //item.LastStatus = connect.Status.ToString();
                                                    //item.StateOfCharge = connect.SoC;
                                                    //item.CurrentChargeKw = connect.ChargeRateKW;
                                                    if (connect != null)
                                                    {
                                                        //item.IsOnline = "<i class='fa-sharp fa-cloud fa-2x color-online'></i>";
                                                        item.IsOnline = connect.Status == ConnectorStatusEnum.Undefined ?
                                                        "<i class='fa-sharp fa-cloud fa-2x color-offline'></i>" : connect.Status == ConnectorStatusEnum.Unavailable ?
                                                        "<i class='fa-sharp fa-cloud fa-2x color-offline'></i>" : "<i class='fa-sharp fa-cloud fa-2x color-online'></i>";
                                                    }

                                                    item.LastStatus = connect.Status == ConnectorStatusEnum.Available ?
                                                    "<span class='color-online'>ว่าง</span>" : connect.Status == ConnectorStatusEnum.Charging ?
                                                    "<span class='color-working'>กำลังชาร์จ</span>" : connect.Status == ConnectorStatusEnum.Occupied ?
                                                    "<span class='color-working'>กำลังชาร์จ</span>" : connect.Status == ConnectorStatusEnum.Preparing ?
                                                    "<span class='color-all'>เตรียมชาร์จ</span>" : connect.Status == ConnectorStatusEnum.Faulted ?
                                                    "<span class='color-broken'>มีปัญหา</span>" : connect.Status == ConnectorStatusEnum.Finishing ?
                                                    "<span class='color-broken'>ชาร์จจบแล้ว</span>" : "";

                                                    item.Status = connect.Status.ToString();

                                                    //<span>24.56kW 1.23kWh Soc 32%</span>
                                                    if (connect.ChargeRateKW > 0 && connect.MeterKWH > 0 && connect.Status == ConnectorStatusEnum.Charging)
                                                    {
                                                        item.LastMeter = connect.MeterKWH;
                                                        //item.LastStatus = connect.Status.ToString();
                                                        item.StateOfCharge = connect.SoC;
                                                        item.CurrentChargeKw = connect.ChargeRateKW;
                                                        item.Reason = $"<span>{connect.ChargeRateKW}kW  {(connect.MeterKWH ?? 0).ToString("#,0.00#")}kWh{(connect.SoC > 0 ? $"  Soc {connect.SoC}%" : "")}</span>";
                                                    }
                                                    else
                                                        item.Reason = "";

                                                    if (!string.IsNullOrEmpty(charger.Heartbeat))
                                                    {
                                                        Logger.LogInformation($"Heartbeat=====> {charger.Heartbeat}");

                                                        var lastDate = TimeZoneInfo.ConvertTimeFromUtc(
                                                            DateTime.ParseExact(charger.Heartbeat, "o", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind)
                                                            , timeZone
                                                        );

                                                        Logger.LogInformation($"Heartbeat lastDate =====> {lastDate}");

                                                        item.IsHeartBeat = (now - lastDate).TotalSeconds > HeartbeatTimeout ? "color-offline" : "color-online";
                                                        item.HeartBeatlastDate = lastDate.ToString("dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("en-US"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Logger.LogError("Index: Result of status web request is empty");
                                        serverError = true;
                                    }
                                }
                                else
                                {
                                    Logger.LogError("Index: Result of status web request => httpStatus={0}", response.StatusCode);
                                    serverError = true;
                                }
                            }

                            //Logger.LogInformation("Index: Result of status web request => Length={0}", onlineStatusList?.Length);
                        }
                        catch (Exception exp)
                        {
                            Logger.LogError(exp, "Index: Error in status web request => {0}", exp.Message);
                            serverError = true;
                        }



                        if (serverError)
                        {
                            //ViewBag.ErrorMsg = _localizer["ErrorOCPPServer"];
                            foreach (var item in connectorstatus)
                            {
                                item.LastStatus = "";
                                item.IsOnline = "<i class='fa-sharp fa-cloud fa-2x color-offline'></i>";
                                item.IsServerOnlone = false;
                            }
                        }

                        foreach (var item in connectorstatus)
                        {
                            item.IsOnline = (item.WebSocketStatus ?? "").ToLower().Contains("open") ? "<i class='fa-sharp fa-cloud fa-2x color-online'></i>" : item.IsOnline;
                            //var logs = ChargePointModel.GetMessageLogs(item.ChargerId);
                            //if (logs != null)
                            //{
                            //    var lastDate = TimeZoneInfo.ConvertTimeFromUtc(logs.FDate.Value, timeZone);
                            //    item.IsHeartBeat = (now - lastDate).TotalSeconds > HeartbeatTimeout ? "color-offline" : "color-online";
                            //    item.HeartBeatlastDate = lastDate.ToString("dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("en-US"));
                            //}
                           

                            item.ActionStartStop = $"<a href='javascript:RemoteStartTransaction(\"{item.ChargerId}\", \"{item.ConnectorId}\");' class='color-online'>START</a> | <a href='javascript:RemoteStopTransaction(\"{item.ChargerId}\", \"{item.ConnectorId}\");' class='color-broken'>STOP</a>";
                            var _json = JsonConvert.SerializeObject(item);
                            //_hub.Clients.All.SendAsync($"ChargePointDetailMessage_{item.ChargerId}_{item.ConnectorId}", _json);
                            _ = _hub.Clients.All.SendAsync($"ChargePointDetailMessage_{item.ChargerId}", _json);
                        }

                        var json = JsonConvert.SerializeObject(connectorstatus);
                        _hub.Clients.All.SendAsync("ChargePointMessage", json);
                        await Task.Delay(2000);
                    }
                }, _ts.Token);
            }
            #endregion
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _ts.Cancel();
            _tslog.Cancel();
            return Task.CompletedTask;
        }
    }
}
