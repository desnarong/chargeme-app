using chargeme_app.Server.DataContext;
using chargeme_app.Server.Models;
using chargeme_app.Server.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace chargeme_app.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class PaymentCallBackController : ControllerBase
    {
        private readonly NpgsqlDbContext _context;
        private readonly string _merchantId;
        private readonly MemoryCacheService _cachService;
        private readonly OCPPService _ocppService;
        private readonly IConfiguration _configuration;
        public PaymentCallBackController(NpgsqlDbContext context, MemoryCacheService cachService, OCPPService ocppService, IConfiguration configuration)
        {
            _context = context;
            _cachService = cachService;
            _ocppService = ocppService;
            _configuration = configuration;

            var api = _context.TblPaymentApis.FirstOrDefault();
            _merchantId = api.FMerchantId;
        }
        // กำหนด Route โดยใช้ค่า TransactionPath จากการตั้งค่า
        [HttpPost("{_merchantId}")]
        //[Route("/{_merchantId}")]
        public async Task<IActionResult> PostTransaction([FromForm] PaymentGwRequest request)
        {
            // ประมวลผลข้อมูลที่ได้รับ
            var payment = await _context.TblPayments.FirstOrDefaultAsync(x => x.FPaymentCode == request.RefNo);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found." });
            }

            // อัปเดตสถานะการชำระเงิน
            payment.FPaymentStatus = "Paid";
            payment.FUpdated = DateTime.UtcNow;
            _context.TblPayments.Update(payment);
            await _context.SaveChangesAsync(); // ✅ รอให้การบันทึกเสร็จ

            // ดึงข้อมูล transaction ที่เกี่ยวข้อง
            var trans = await _context.TblTransactions.FirstOrDefaultAsync(x => x.FId == payment.FTransactionId);
            if (trans == null)
            {
                return NotFound(new { message = "Transaction not found." });
            }

            // อัปเดต connector
            var connector = await _context.TblConnectorStatuses.FirstOrDefaultAsync(x => x.FId == trans.FConnectorId);
            if (connector != null)
            {
                connector.FTransactionId = trans.FId;
                _context.TblConnectorStatuses.Update(connector);
                await _context.SaveChangesAsync(); // ✅ รอให้การบันทึกเสร็จ
            }

            // อัปเดตหมายเลขธุรกรรม
            trans.FTransactionNo = long.Parse(payment.FOrderNo);
            _context.TblTransactions.Update(trans);
            await _context.SaveChangesAsync(); // ✅ รอให้การบันทึกเสร็จ

            // อัปเดตแคช
            await _cachService.RefreshCachePaymentIfDatabaseUpdated(payment.FId);

            try
            {
                var charger = await _context.TblChargers.FirstOrDefaultAsync(x => x.FId == trans.FChargerId);
                var station = await _context.TblStations.FirstOrDefaultAsync(x => x.FId == trans.FStationId);
                if (charger != null && station != null)
                {
                    await _ocppService.StartChargingSession(trans.FChargerId, charger.FCode, (int)connector.FConnectorId, station.FRfid);
                }

                // ส่งสถานะตอบกลับไปยัง client
                return Ok(new { message = "Transaction received successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception happened: " + ex.ToString());
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        [HttpPost("{_merchantId}/start-transaction")]
        public async Task<IActionResult> StartTransaction(Guid FChargerId, string FCode, int FConnectorId, string FRfid)
        {
            try
            {
                var message = await _ocppService.StartTransaction(FChargerId, FCode, (int)FConnectorId, FRfid);
                return Ok(new { message = "Transaction start successfully. " + message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.ToString() });
            }
        }
    }
}
