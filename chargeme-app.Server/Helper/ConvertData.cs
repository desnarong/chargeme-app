using Microsoft.VisualBasic;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace chargeme_app.Server.Helper
{
    public class ConvertData
    {
        public static string ConvertBuddhistDateToChristianDate(DateTime dateTime)
        {
            // แปลงเป็นวันที่ในรูปแบบ ค.ศ. en-US
            dateTime = dateTime.AddYears(543);
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("en-US"));
        }
        public static string ConvertYYMMDD(DateTime dateTime)
        {
            // แปลงเป็นวันที่ในรูปแบบ ค.ศ. en-US
            return dateTime.ToString("yyMMdd", new CultureInfo("en-US"));
        }
        public static DateTime ConvertChristianToBuddhistDateDate(string dateTime)
        {
            var date = DateTime.Parse(dateTime, new CultureInfo("th-TH"));
            date = date.AddYears(543);
            return date;
        }
        public static string FormatThaiDate(DateTime date)
        {
            // แปลง string เป็น DateTime
            //DateTime date = DateTime.Parse(dateInput, null, DateTimeStyles.RoundtripKind);

            // ตั้งค่าข้อมูลวัฒนธรรมให้เป็น "th-TH" (ไทย)
            CultureInfo thaiCulture = new CultureInfo("th-TH");
            thaiCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();

            // จัดรูปแบบวันที่ตามที่ต้องการ
            string formattedDate = date.ToString("d MMM yyyy", thaiCulture); // "20 ต.ค. 2567"

            return formattedDate;
        }
    }
}
