using chargeme_app.Server.DataContext;
using chargeme_app.Server.Models;
using chargeme_app.Server.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace chargeme_app.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly NpgsqlDbContext _context;
        private readonly MemoryCacheService _cachService;
        public PaymentController(NpgsqlDbContext context, MemoryCacheService cachService)
        {
            _context = context;
            _cachService = cachService;
        }
        [HttpPost("status")]
        public async Task<IActionResult> GetStatusAsync([FromBody] TransactionStatusRequestModel request)
        {
            //var payment = _context.TblPayments.First(x => x.FId == Guid.Parse(request.Paymentid));
            //return Ok(new { data = new { status = payment.FPaymenyStatus.ToLower() } });
            var payment = await _cachService.GetPaymentAsync(Guid.Parse(request.Paymentid));
            if(payment.FPaymentStatus.ToLower() == "paid")
            {
                _cachService.ClearPayment(payment.FId);
            }

            return Ok(new { data = new { status = payment.FPaymentStatus.ToLower() } });
        }
        [HttpPost("cancel")]
        public async Task<IActionResult> GetCancelAsync([FromBody] TransactionStatusRequestModel request)
        {
            var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            var payment = await _cachService.GetPaymentAsync(Guid.Parse(request.Paymentid));
            //var payment = _context.TblPayments.First(x => x.FId == Guid.Parse(request.Paymentid));
            payment.FPaymentStatus = "Cancel";
            payment.FUpdated = DateTime.UtcNow;
            payment.FUpdateby = Guid.Parse(userID);
            _context.TblPayments.Update(payment);
            await _context.SaveChangesAsync();

            var trans = _context.TblTransactions.First(x => x.FId == payment.FTransactionId);
            trans.FTransactionStatus = "Cancel";
            trans.FUpdated = DateTime.UtcNow;
            _context.TblTransactions.Update(trans);
            await _context.SaveChangesAsync();

            _cachService.ClearPayment(payment.FId);

            return Ok();
        }
    }
}
