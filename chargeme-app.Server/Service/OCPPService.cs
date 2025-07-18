using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace chargeme_app.Server.Service
{
    public class OCPPService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private ClientWebSocket _webSocket;

        public OCPPService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _configuration["ApiKey"]);
        }

        public async Task<bool> StartChargingSession(Guid chargePointId, string chargePointCode, int connectorId, string rfidTag)
        {
            try
            {
                //var connected = await OpenWebSocketConnection(chargePointCode);

                var transactionStarted = await RemoteStartTransaction(chargePointId, chargePointCode, connectorId, rfidTag);
                return transactionStarted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartChargingSession: {ex.Message}");
                return false;
            }
        }

        public async Task<string> StartTransaction(Guid chargePointId, string chargePointCode, int connectorId, string rfidTag)
        {
            try
            {
                var transactionStarted = await RemoteStartTransaction(chargePointId, chargePointCode, connectorId, rfidTag);
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> StopTransaction(Guid chargePointId, string chargePointCode, int connectorId, string rfidTag)
        {
            try
            {
                var transactionStarted = await RemoteStopTransaction(chargePointId, chargePointCode, connectorId, rfidTag);
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<bool> OpenWebSocketConnection(string chargePointCode)
        {
            try
            {
                string wsUrl = _configuration["OCPPWebSocketUrl"];
                string fullWsUrl = $"{wsUrl}/{chargePointCode}";

                _webSocket = new ClientWebSocket();
                _webSocket.Options.AddSubProtocol("ocpp1.6");
                _webSocket.Options.AddSubProtocol("ocpp1.5");

                if (!string.IsNullOrEmpty(_configuration["ApiKey"]))
                {
                    _webSocket.Options.SetRequestHeader("X-API-Key", _configuration["ApiKey"]);
                }

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await _webSocket.ConnectAsync(new Uri(fullWsUrl), cts.Token);
                Console.WriteLine($"WebSocket connected to {fullWsUrl}");

                // Send BootNotification with full payload like JS version
                var bootId = Guid.NewGuid().ToString();

                var bootNotificationPayload = new
                {
                    chargePointVendor = "AVT-Company",
                    chargePointModel = "AVT-Express",
                    chargePointSerialNumber = "avt.001.13.1",
                    chargeBoxSerialNumber = "avt.001.13.1.01",
                    firmwareVersion = "0.9.87",
                    iccid = "",
                    imsi = "",
                    meterType = "AVT NQC-ACDC",
                    meterSerialNumber = "avt.001.13.1.01"
                };

                var bootNotificationArray = new object[]
                {
            2,
            bootId,
            "BootNotification",
            bootNotificationPayload
                };

                var bootNotificationJson = JsonSerializer.Serialize(bootNotificationArray);

                await SendMessageAsync(bootNotificationJson);

                // Start background receive loop
                _ = Task.Run(() => ReceiveLoop());

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening WebSocket connection: {ex.Message}");
                return false;
            }
        }


        private async Task SendMessageAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];
            while (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"WebSocket message received: {msg}");

                    var json = JsonDocument.Parse(msg);
                    var arr = json.RootElement.EnumerateArray().ToList();

                    int msgType = arr[0].GetInt32();
                    string msgId = arr[1].GetString();

                    if (msgType == 2) // CALL
                    {
                        string action = arr[2].GetString();
                        var payload = arr[3];
                        await HandleIncomingCall(msgId, action, payload);
                    }
                    else if (msgType == 3) // CALLRESULT
                    {
                        Console.WriteLine($"CALLRESULT: {arr[2]}");
                    }
                    else if (msgType == 4) // CALLERROR
                    {
                        Console.WriteLine($"CALLERROR: {arr[2]}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in ReceiveLoop: {ex.Message}");
                }
            }
        }

        private async Task HandleIncomingCall(string msgId, string action, JsonElement payload)
        {
            Console.WriteLine($"Received action: {action}");

            switch (action)
            {
                case "RemoteStartTransaction":
                    await SendMessageAsync(JsonSerializer.Serialize(new object[] {
                        3, msgId, new { status = "Accepted" }
                    }));
                    Console.WriteLine("→ Accepted RemoteStartTransaction");
                    break;

                case "Reset":
                    await SendMessageAsync(JsonSerializer.Serialize(new object[] {
                        3, msgId, new { status = "Accepted" }
                    }));
                    Console.WriteLine("→ Accepted Reset");
                    break;

                case "UnlockConnector":
                    int connectorId = payload.GetProperty("connectorId").GetInt32();
                    await SendMessageAsync(JsonSerializer.Serialize(new object[] {
                        3, msgId, new { status = "Unlocked" }
                    }));
                    Console.WriteLine($"→ Unlocked connector {connectorId}");
                    break;

                default:
                    Console.WriteLine($"Unknown action: {action}");
                    await SendMessageAsync(JsonSerializer.Serialize(new object[] {
                        4, msgId, "NotSupported", $"Action {action} not supported"
                    }));
                    break;
            }
        }

        public async Task<string> CheckStatus()
        {
            try
            {
                string apiUrl = _configuration["OCPPApiUrl"];
                string startUrl = $"{apiUrl}/Status";

                var request = new HttpRequestMessage(HttpMethod.Get, startUrl);
                request.Headers.Add("X-API-Key", _configuration["ApiKey"]);
                using var httpClient = new HttpClient();
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private async Task<bool> RemoteStartTransaction(Guid chargePointId, string chargePointCode, int connectorId, string rfidTag)
        {
            try
            {
                string apiUrl = _configuration["OCPPApiUrl"];
                string startUrl = $"{apiUrl}/RemoteStartTransaction/{chargePointCode}/{connectorId}/{rfidTag}";

                var request = new HttpRequestMessage(HttpMethod.Post, startUrl);
                request.Headers.Add("X-API-Key", _configuration["ApiKey"]);
                using var httpClient = new HttpClient();
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonDoc = JsonDocument.Parse(responseBody);
                if (jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
                {
                    var status = statusElement.GetString();
                    return status?.Equals("Accepted", StringComparison.OrdinalIgnoreCase) == true;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<bool> RemoteStopTransaction(Guid chargePointId, string chargePointCode, int connectorId, string rfidTag)
        {
            try
            {
                string apiUrl = _configuration["OCPPApiUrl"];
                string startUrl = $"{apiUrl}/RemoteStopTransaction/{chargePointCode}/{connectorId}/{rfidTag}";

                var request = new HttpRequestMessage(HttpMethod.Post, startUrl);
                request.Headers.Add("X-API-Key", _configuration["ApiKey"]);
                using var httpClient = new HttpClient();
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonDoc = JsonDocument.Parse(responseBody);
                if (jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
                {
                    var status = statusElement.GetString();
                    return status?.Equals("Accepted", StringComparison.OrdinalIgnoreCase) == true;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _webSocket?.Dispose();
        }
    }
}
