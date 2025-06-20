using csms.DataContext;
using csms.Entities;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;

namespace csms.Models
{
    public class TransactionModel
    {
        static TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        public static List<TblTransaction> GetTransactions()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblTransactions.ToList();
                return data;
            }
        }
        public static List<TransactionData> GetTransactionDatas()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from trans in context.TblTransactions
                            //join tmp_tag in context.TblChargingTags on trans.FCardId equals tmp_tag.FId into tagtable
                            //from tag in tagtable.DefaultIfEmpty()
                            join charger in context.TblChargers on trans.FChargerId equals charger.FId
                            join connector in context.TblConnectorStatuses on trans.FConnectorId equals connector.FId
                            where trans.FStartTime != null// trans.FMeterEnd > 0
                            select new TransactionData
                            {
                                ChargerId = trans.FChargerId,
                                ChargerName = charger.FName,
                                ChargerCode = charger.FCode,
                                ConnectorId = trans.FConnectorId,
                                ConnectorNo = connector.FConnectorId,
                                ConnectorName = connector.FName,
                                //StartTagId = trans.StartTagId,
                                //PlateNo = tag.FPlateNo,
                                StartTime = trans.FStartTime,
                                StartDateTime = (trans.FStartTime != null) ? trans.FStartTime.Value.ToString("dd/MM/yyyy HH:mm", new System.Globalization.CultureInfo("en-US")):"",
                                StopTime = trans.FEndTime,
                                TransactionId = trans.FId,
                                StopReason = trans.FEndResult,
                                StartResult = trans.FStartResult,
                                MeterStart = (trans.FMeterStart != null) ? Convert.ToDouble(trans.FMeterStart) : 0,
                                MeterStop = (trans.FMeterEnd != null) ? Convert.ToDouble(trans.FMeterEnd) : 0,
                                Cost = (trans.FCost != null) ? Convert.ToDouble(trans.FCost) : 0,
                                ChargeSum = (trans.FMeterEnd != null ? (trans.FMeterEnd - trans.FMeterStart).Value.ToString("#,0.00#") : ""),
                            })
                             .ToList();
                return data;
            }
            return new List<TransactionData>();
        }
        public static List<TransactionData> GetTransactionDatas(Guid? station)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from trans in context.TblTransactions
                            join tmp_tag in context.TblChargingTags on trans.FCardId equals tmp_tag.FId into tagtable
                            from tag in tagtable.DefaultIfEmpty()
                            join charger in context.TblChargers on trans.FChargerId equals charger.FId
                            join connector in context.TblConnectorStatuses on trans.FConnectorId equals connector.FId
                            where trans.FStationId == station//trans.FMeterEnd > 0 && 
                            select new TransactionData
                            {
                                ChargerId = trans.FChargerId,
                                ChargerName = charger.FName,
                                ChargerCode = charger.FCode,
                                ConnectorId = trans.FConnectorId,
                                ConnectorNo = connector.FConnectorId,
                                ConnectorName = connector.FName,
                                StartTagId = "",
                                PlateNo = tag.FPlateNo,
                                StartTime = (trans.FStartTime != null) ? trans.FStartTime.Value : DateTime.MinValue,
                                StartDateTime = (trans.FStartTime != null) ? TimeZoneInfo.ConvertTimeFromUtc(trans.FStartTime.Value, timeZone).ToString("dd/MM/yyyy HH:mm", new System.Globalization.CultureInfo("en-US")) : "",
                                StopTime = trans.FEndTime,
                                TransactionId = trans.FId,
                                TransactionNo = trans.FTransactionNo,
                                StopReason = trans.FEndResult,
                                StartResult = trans.FStartResult,
                                MeterStart = (trans.FMeterStart != null) ? Convert.ToDouble(trans.FMeterStart) : 0,
                                MeterStop = (trans.FMeterEnd != null) ? Convert.ToDouble(trans.FMeterEnd) : 0,
                                Cost = (trans.FCost != null) ? Convert.ToDouble(trans.FCost) : 0,
                                ChargeSum = (trans.FMeterEnd != null ? (trans.FMeterEnd - trans.FMeterStart).Value.ToString("#,0.00#") : ""),
                            })
                             .ToList();
                return data;
            }
            return new List<TransactionData>();
        }
    }
    public class TransactionData
    {
        public Guid? TransactionId { get; set; }
        public long? TransactionNo { get; set; }
        public string? Uid { get; set; }

        public Guid? ChargerId { get; set; } = null!;

        public string? ChargerCode { get; set; }

        public string? ChargerName { get; set; }

        public Guid? ConnectorId { get; set; }

        public int? ConnectorNo { get; set; }

        public string? ConnectorName { get; set; }

        public string? StartTagId { get; set; }

        public DateTime? StartTime { get; set; }

        public double MeterStart { get; set; }

        public string? StartResult { get; set; }

        public string? StopTagId { get; set; }

        public DateTime? StopTime { get; set; }

        public double? MeterStop { get; set; }

        public string? StopReason { get; set; }

        public bool? Status { get; set; }
        public string? PlateNo { get; set; }
        public string? StartDateTime { get; set; }
        public string? ChargeSum { get; set; }
        public string? UsedTime { get; set; }
        public double? Cost { get; set; }
    }
}
