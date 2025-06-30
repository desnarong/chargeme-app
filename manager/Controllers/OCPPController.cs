using manager.Entities;
using manager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace manager.Controllers
{
    public partial class OCPPController : Controller
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<OCPPController> _logger;

        public OCPPController(IHttpClientFactory clientFactory, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _client = clientFactory.CreateClient("ApiClient");
            _config = config;
            _logger = loggerFactory.CreateLogger<OCPPController>();
        }
    
        public IActionResult GetChargeTags()
        {
            var data = CardModel.GetChargeTags();
            string json = JsonConvert.SerializeObject(data);
            return new JsonResult(json);
        }

        public IActionResult GetConnectors(string id, int connectorid)
        {
            var data = ChargerModel.GetConnectorStatuses(Guid.Parse(id)); 
            string json = JsonConvert.SerializeObject(data);
            return new JsonResult(json);
        }

        public IActionResult GetTransactions(string id, int connectorid)
        {
            var data = TransactionModel.GetTransactions(Guid.Parse(id), connectorid);
            string json = JsonConvert.SerializeObject(data);
            return new JsonResult(json);
        }
    }
}
