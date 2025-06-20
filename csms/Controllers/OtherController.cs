using Microsoft.AspNetCore.Mvc;

namespace csms.Controllers
{
    public class OtherController : Controller
    {
        IConfiguration _config;
        public OtherController(IConfiguration _configuration) {
            _config = _configuration;
        }
        public IActionResult Index()
        {
            ViewData["url"] = _config.GetValue<string>("OtherUrl");
            return View();
        }
    }
}
