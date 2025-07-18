using System.ComponentModel.DataAnnotations;

namespace chargeme_app.Server.Models
{
    public class PaymentGwRequest
    {
        public required string RefNo { get; set; }

        public required string CardType { get; set; } // เช่น "PP" หรือ "V"

        public required string MerchantId { get; set; }

        public required string ProductDetail { get; set; }

        public required decimal Total { get; set; } // ใช้ decimal สำหรับจำนวนเงิน
    }
}
