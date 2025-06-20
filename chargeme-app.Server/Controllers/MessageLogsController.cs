using chargeme_app.Server.DataContext;
using chargeme_app.Server.Models;
using chargeme_app.Server.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCPP.Core.Lib;
using System.Security.Claims;

namespace chargeme_app.Server.Controllers
{
    [ApiController]
    [Route("messagelogs")]
    public class MessageLogsController : ControllerBase
    {
        private readonly NpgsqlDbContext _context;
        public MessageLogsController(NpgsqlDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult MessageLogs([FromBody] MessageLog request)
        {
            //_context.TblMessageLogs.Add(new Entities.TblMessageLog()
            //{
            //    FChargingMachine = request.ChargePointId,
            //    FConnectorId = request.ConnectorId,
            //    FMessage = request.Message,
            //    FResult = request.Result,
            //    FErrorCode = request.ErrorCode,
            //    FLogType = request.LogType,
            //    FLogState = request.LogState
            //});
            return Ok();
        }
    }
}
