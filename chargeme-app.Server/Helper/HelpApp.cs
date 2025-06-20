namespace chargeme_app.Server.Helper
{
    public class HelpApp
    {
        public static bool IsOnPeak(DateTime time)
        {
            // กำหนดช่วงเวลา On-Peak (9:00 - 22:00)
            TimeSpan startOnPeak = new TimeSpan(9, 0, 0); // 9:00
            TimeSpan endOnPeak = new TimeSpan(22, 0, 0);  // 22:00

            return time.TimeOfDay >= startOnPeak && time.TimeOfDay <= endOnPeak;
        }
        // คำนวณจำนวนหน่วยจากจำนวนเงินและอัตราค่าชาร์จ
        public static decimal CalculateUnits(decimal amount, decimal rate)
        {
            return amount / rate;
        }
        public static decimal CalculatePrice(decimal unit, decimal rate)
        {
            return unit * rate;
        }
    }
}
