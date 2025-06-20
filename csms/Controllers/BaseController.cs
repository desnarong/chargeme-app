using System;
using csms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace csms.Controllers
{
    public partial class BaseController : Controller
    {
        protected UserManager UserManager { get; private set; }

        protected ILogger Logger { get; set; }

        protected IConfiguration Config { get; private set; }

        public BaseController(
            UserManager userManager,
            ILoggerFactory loggerFactory,
            IConfiguration config)
        {
            UserManager = userManager;
            Config = config;
        }

    }
    public class JsonResponse<T>
    {
        public T Data { get; set; }
    }
}
