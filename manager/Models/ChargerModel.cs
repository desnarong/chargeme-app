using manager.DataContext;
using manager.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Collections.Specialized.BitVector32;
namespace manager.Models
{
    public class ChargerModel
    {
        static TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        public static List<TblCharger> GetChargers()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblChargers.ToList();
                return data;
            }
        }
        public static List<TblConnectorStatus> GetConnectorStatus()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblConnectorStatuses.ToList();
                return data;
            }
        }
        public static List<TblConnectorStatus> GetConnectorStatuses(string ChargerCode, string ConnectorCode)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from item in context.TblConnectorStatuses
                            join chargepoint in context.TblChargers on item.FChargerId equals chargepoint.FId
                            where chargepoint.FCode == ChargerCode && item.FCode == ConnectorCode
                            select item).ToList();
                return data;
            }
        }
        public static List<TblConnectorStatus> GetConnectorStatuses(Guid ChargerId)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from item in context.TblConnectorStatuses
                            join chargepoint in context.TblChargers on item.FChargerId equals chargepoint.FId
                            where chargepoint.FId == ChargerId
                            select item).ToList();
                return data;
            }
        }
        public static TblConnectorStatus GetConnectorStatus(Guid ChargerId, Guid ConnectorId)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblConnectorStatuses.Where(x => x.FChargerId == ChargerId && x.FId == ConnectorId).FirstOrDefault();
                return data;
            }
        }
        public static TblConnectorStatus GetConnectorStatus(Guid ChargerId)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblConnectorStatuses.Where(x => x.FChargerId == ChargerId).FirstOrDefault();
                return data;
            }
        }
        public static List<ConnectorStatusData> GetConnectorStatusDatas()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from item in context.TblConnectorStatuses
                            join chargepoint in context.TblChargers on item.FChargerId equals chargepoint.FId
                            select new ConnectorStatusData
                            {
                                ChargerId = item.FChargerId,
                                FId = item.FId,
                                ShortName = chargepoint.FShortName,
                                ChargerName = chargepoint.FName,
                                ConnectorId = item.FConnectorId ?? 0,
                                ConnectorName = item.FName,
                                Code = item.FCode,
                                IsOnline = (item.FCurrentStatus == "" ? "<i class='ri-cloud-off-fill la-2x color-offline'></i>" : "<i class='ri-cloud-fill la-2x color-online'></i>"),
                                LastStatus = item.FCurrentStatus,
                                LastStatusTime = TimeZoneInfo.ConvertTimeFromUtc(item.FCurrentStatusTime.Value, timeZone),
                                LastMeter = Convert.ToDouble(item.FCurrentMeter),
                                LastMeterTime = TimeZoneInfo.ConvertTimeFromUtc(item.FCurrentMeterTime.Value, timeZone),
                                StateOfCharge = Convert.ToDouble(item.FStateOfCharge),
                                CurrentChargeKw = Convert.ToDouble(item.FCurrentChargeKw),
                                ActionView = "",
                                ActionEdit = $"<button class='btn btn-soft-secondary btn-sm' type='button' onclick='EditConectorModal(\"{item.FChargerId}\",\"{item.FId}\")' aria-expanded='false'><i class='ri-edit-box-line la-1-50x'></i></button>",
                            }).ToList();
                return data;
            }
        }
        public static List<ConnectorStatusViewData> GetConnectorStatusViews()
        {
            var data = new List<ConnectorStatusViewData>();
            using (var context = new NpgsqlDbContext())
            {
                try
                {
                    var query = from cs in context.ConnectorStatusViews
                                join stationinfo in context.TblStations on cs.FStationId equals stationinfo.FId into stationtemp
                                from station in stationtemp.DefaultIfEmpty()
                                select new ConnectorStatusViewData()
                                {
                                    CompanyId = station.FCompanyId,
                                    StationId = cs.FStationId,
                                    StationName = station.FName,
                                    ChargerId = cs.FChargerId,
                                    ChargerCode = cs.FShortName,
                                    ChargerName = cs.FChargerName,
                                    ShortName = cs.FShortName,
                                    Id = cs.FId,
                                    ConnectorCode = cs.FCode,
                                    ConnectorId = cs.FConnectorId,
                                    ConnectorName = cs.FName,
                                    IsOnline = "<i class='ri-cloud-off-fill la-2x color-offline'></i>", //(cs.FCurrentStatus == "" ? "<i class='ri-cloud-off-fill la-2x color-offline'></i>" : "<i class='ri-cloud-fill la-2x color-online'></i>"),
                                    //LastStatus = cs.FCurrentStatus,
                                    LastStatus = "",
                                    LastStatusTime = TimeZoneInfo.ConvertTimeFromUtc(cs.FCurrentStatusTime.Value, timeZone),
                                    LastMeter = Convert.ToDouble(cs.FCurrentMeter),
                                    LastMeterTime = TimeZoneInfo.ConvertTimeFromUtc(cs.FCurrentMeterTime.Value, timeZone),
                                    StateOfCharge = Convert.ToDouble(cs.FStateOfCharge),
                                    CurrentChargeKw = Convert.ToDouble(cs.FCurrentChargeKw),
                                    //Comment = chargepoint.Comment,
                                    //Image = chargepoint.Image,
                                    TransactionNo = cs.FTransactionNo,
                                    ActionView = $"<a href='/Dashboards/ConnecterDetail?Charger={cs.FChargerId}&Connecter={cs.FId}' class='btn btn-soft-secondary btn-sm' title='View'><i class='ri-eye-line la-1-50x'></i></a>",
                                    //ActionEdit = $"<a href='JavaScript:EditConectorModal(\"{item.ChargerId}\",\"{item.ConnectorId}\")' class='btn btn-soft-secondary btn-sm' title='edit'><i class='ri-edit-box-line'></i></a>",
                                };
                    data = query.ToList();
                }
                catch (Exception ee)
                {

                }

            }
            return data;
        }
        public static List<ConnectorStatusViewData> GetConnectorStatusViews(Guid? companyid)
        {
            var data = new List<ConnectorStatusViewData>();
            using (var context = new NpgsqlDbContext())
            {
                try
                {
                    var query = from cs in context.ConnectorStatusViews
                                join station in context.TblStations on cs.FStationId equals station.FId
                                where station.FCompanyId == companyid
                                select new ConnectorStatusViewData()
                                {
                                    CompanyId = station.FCompanyId,
                                    StationId = cs.FStationId,
                                    ChargerId = cs.FChargerId,
                                    ChargerCode = cs.FShortName,
                                    ChargerName = cs.FChargerName,
                                    ShortName = cs.FShortName,
                                    Id = cs.FId,
                                    ConnectorCode = cs.FCode,
                                    ConnectorId = cs.FConnectorId,
                                    ConnectorName = cs.FName,
                                    IsOnline = (cs.FCurrentStatus == "" ? "<i class='ri-cloud-off-fill la-2x color-offline'></i>" : "<i class='ri-cloud-fill la-2x color-online'></i>"),
                                    LastStatus = cs.FCurrentStatus,
                                    LastStatusTime = TimeZoneInfo.ConvertTimeFromUtc(cs.FCurrentStatusTime.Value, timeZone),
                                    LastMeter = Convert.ToDouble(cs.FCurrentMeter),
                                    LastMeterTime = TimeZoneInfo.ConvertTimeFromUtc(cs.FCurrentMeterTime.Value, timeZone),
                                    StateOfCharge = Convert.ToDouble(cs.FStateOfCharge),
                                    CurrentChargeKw = Convert.ToDouble(cs.FCurrentChargeKw),
                                    //Comment = chargepoint.Comment,
                                    //Image = chargepoint.Image,
                                    TransactionNo = cs.FTransactionNo,
                                    ActionView = $"<a href='/Home/Detail?Charger={cs.FChargerId}&ConectorId={cs.FId}' class='btn btn-soft-secondary btn-sm' title='View'><i class='ri-eye-line la-1-50x'></i></a>",
                                    //ActionEdit = $"<a href='JavaScript:EditConectorModal(\"{item.ChargerId}\",\"{item.ConnectorId}\")' class='btn btn-soft-secondary btn-sm' title='edit'><i class='ri-edit-box-line'></i></a>",
                                };
                    data = query.ToList();
                }
                catch (Exception ee)
                {

                }

            }
            return data;
        }
        public static List<ConnectorStatusData>? GetConnectorStatusDatas(Guid ChargerId)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from item in context.TblConnectorStatuses
                            join chargepoint in context.TblChargers on item.FChargerId equals chargepoint.FId
                            where item.FChargerId == ChargerId
                            select new ConnectorStatusData
                            {
                                ChargerId = item.FChargerId,
                                ChargerName = chargepoint.FName,
                                ConnectorId = (int)item.FConnectorId,
                                ConnectorName = item.FName,
                                IsOnline = (item.FCurrentStatus == "" ? "color-offline" : "color-online"),
                                LastStatus = item.FCurrentStatus,
                                LastStatusTime = TimeZoneInfo.ConvertTimeFromUtc(item.FCurrentStatusTime.Value, timeZone),
                                LastMeter = Convert.ToDouble(item.FCurrentMeter),
                                LastMeterTime = TimeZoneInfo.ConvertTimeFromUtc(item.FCurrentMeterTime.Value, timeZone),
                                StateOfCharge = Convert.ToDouble(item.FStateOfCharge),
                                CurrentChargeKw = Convert.ToDouble(item.FCurrentChargeKw),
                                //Image = chargepoint.Image,
                                ActionView = $"<a href='/Home/Detail?Charger={item.FChargerId}&ConectorId={item.FId}' class='btn btn-soft-secondary btn-sm' title='View'><i class='ri-eye-line la-1-50x'></i></a>",
                                ActionEdit = $"<a href='JavaScript:EditConectorModal(\"{item.FChargerId}\",\"{item.FId}\")' class='btn btn-soft-secondary btn-sm' title='edit'><i class='ri-edit-box-line la-1-50x'></i></a>",
                            }).ToList();
                return data;
            }
        }
        public static List<ConnectorStatusViewData> GetConnectorStatusViewDatas(Guid ChargerId)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from item in context.ConnectorStatusViews
                            join chargepoint in context.TblChargers on item.FChargerId equals chargepoint.FId
                            where item.FChargerId == ChargerId
                            select new ConnectorStatusViewData
                            {
                                ChargerId = item.FChargerId,
                                ChargerName = chargepoint.FName,
                                ChargerCode = chargepoint.FShortName,
                                ShortName = chargepoint.FShortName,
                                Id = item.FId,
                                ConnectorCode = item.FCode,
                                ConnectorName = item.FName,
                                ConnectorId = item.FConnectorId,
                                IsOnline = (item.FCurrentStatus == "" ? "<i class='ri-cloud-off-fill la-2x color-offline'></i>" : "<i class='ri-cloud-fill la-2x color-online'></i>"),
                                LastStatus = item.FCurrentStatus,
                                LastStatusTime = TimeZoneInfo.ConvertTimeFromUtc(item.FCurrentStatusTime.Value, timeZone),
                                LastMeter = Convert.ToDouble(item.FCurrentMeter),
                                LastMeterTime = TimeZoneInfo.ConvertTimeFromUtc(item.FCurrentMeterTime.Value, timeZone),
                                StateOfCharge = Convert.ToDouble(item.FStateOfCharge),
                                CurrentChargeKw = Convert.ToDouble(item.FCurrentChargeKw),
                                Comment = chargepoint.FComment,
                                Image = chargepoint.FImage,
                                TransactionNo = item.FTransactionNo,
                                TransactionId = item.FTransactionId,
                                ActionView = $"<a href='/Home/Detail?Charger={item.FChargerId}&ConectorId={item.FId}' class='btn btn-soft-secondary btn-sm' title='View'><i class='ri-eye-line la-1-50x'></i></a>",
                                ActionEdit = $"<a href='JavaScript:EditConectorModal(\"{item.FChargerId}\",\"{item.FId}\")' class='btn btn-soft-secondary btn-sm' title='edit'><i class='ri-edit-box-line la-1-50x'></i></a>",
                            }).ToList();
                return data;
            }
        }
        public static ConnectorStatusData? GetConnectorStatusDatas(Guid ChargerId, int ConnectorId)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from item in context.TblConnectorStatuses
                            join chargepoint in context.TblChargers on item.FChargerId equals chargepoint.FId
                            where item.FChargerId == ChargerId && item.FConnectorId == ConnectorId
                            select new ConnectorStatusData
                            {
                                ChargerId = item.FChargerId,
                                ChargerName = chargepoint.FName,
                                ConnectorId = (int)item.FConnectorId,
                                ConnectorName = item.FName,
                                IsOnline = (item.FStatus == 'Y' ? "color-online" : "color-offline"),
                                LastStatus = item.FCurrentStatus,
                                LastStatusTime = TimeZoneInfo.ConvertTimeFromUtc(item.FCurrentStatusTime.Value, timeZone),
                                LastMeter = Convert.ToDouble(item.FCurrentMeter),
                                LastMeterTime = TimeZoneInfo.ConvertTimeFromUtc(item.FCurrentMeterTime.Value, timeZone),
                                StateOfCharge = Convert.ToDouble(item.FStateOfCharge),
                                CurrentChargeKw = Convert.ToDouble(item.FCurrentChargeKw),
                                Image = chargepoint.FImage,
                                ActionView = $"<a href='/Home/Detail?Charger={item.FChargerId}&ConectorId={item.FId}' class='btn btn-soft-secondary btn-sm' title='View'><i class='ri-eye-line la-1-50x'></i></a>",
                                ActionEdit = $"<a href='JavaScript:EditConectorModal(\"{item.FChargerId}\",\"{item.FId}\")' class='btn btn-soft-secondary btn-sm' title='edit'><i class='ri-edit-box-line la-1-50x'></i></a>",
                            }).FirstOrDefault();
                return data;
            }
        }
        public static List<ChargerData> GetChargerDatas()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from item in context.TblChargers
                            join tmp_station in context.TblStations on item.FStationId equals tmp_station.FId into stationtable
                            from station in stationtable.DefaultIfEmpty()
                            join tmp_company in context.TblCompanies on station.FCompanyId equals tmp_company.FId into companytable
                            from company in companytable.DefaultIfEmpty()
                            select new ChargerData
                            {
                                ChargerId = item.FId,
                                Code = item.FCode,
                                ShortName = item.FShortName,
                                Name = item.FName,
                                Comment = item.FComment,
                                Username = item.FUsername,
                                Password = item.FPassword,
                                ClientCertThumb = item.FClientCertThumb,
                                StationId = item.FStationId,
                                StationName = station.FName,
                                CompanyId = company.FId,
                                CompanyName = company.FName,
                                Image = item.FImage,
                                ActionEdit = $"<a href='JavaScript:EditChargerModal(\"{item.FId}\")' class='btn btn-soft-secondary btn-sm' title='edit'><i class='ri-edit-box-line la-1-50x'></i></a>",
                                ActionDelete = $"<a href='JavaScript:DeleteChargerModalClick(\"{item.FId}\")' class='btn btn-soft-secondary btn-sm' title='delete'><i class='ri-delete-bin-line la-1-50x'></i></a>"
                            })
                             .ToList();
                return data;
            }
        }
        public static TblCharger GetCharger(Guid chargepointid)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblChargers.Where(x => x.FId.Equals(chargepointid)).FirstOrDefault();
                return data;
            }
        }
        public static List<TblMessageLog> GetMessageLogs()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblMessageLogs.ToList();
                return data;
            }
        }
        public static TblMessageLog GetMessageLogs(Guid? chargepointid)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblMessageLogs.Where(x => x.FChargerId.Equals(chargepointid) && (x.FMessage == "Heartbeat" || x.FMessage == "BootNotification"))
                    .OrderByDescending(x => x.FDate)
                    .FirstOrDefault();
                return data;
            }
        }
        public static List<TblMessageLog> GetMessageLogs(Guid? chargepointid, Guid ConnectorId)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblMessageLogs.Where(x => x.FChargerId.Equals(chargepointid) && x.FConnectorId.Equals(ConnectorId)).ToList();
                return data;
            }
        }
        public static List<MessageLogData> GetMessageLogDatasWithRows(Guid chargepointid, Guid ConnectorId, int rows)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from item in context.TblMessageLogs.Where(x => x.FChargerId == chargepointid).OrderByDescending(x => x.FDate).Take(rows)
                            join con in context.TblConnectorStatuses on item.FConnectorId equals con.FId
                            where con.FId == ConnectorId && item.FMessage != "Heartbeat"
                            select new MessageLogData
                            {
                                LogId = item.FId,
                                LogDate = item.FDate,
                                LogTime = TimeZoneInfo.ConvertTimeFromUtc(item.FDate.Value, timeZone).ToString("dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("en-US")),
                                ChargerId = item.FChargerId,
                                ConnectorId = con.FConnectorId,
                                Message = item.FMessage,
                                Result = item.FResult,
                                ErrorCode = item.FErrorCode,
                                LogType = item.FLogType,
                                LogState = item.FLogState
                            })
                             .ToList();
                return data;
            }
        }
        public static List<MessageLogData> GetMessageLogDatas(Guid chargepointid, Guid ConnectorId)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from item in context.TblMessageLogs
                            where item.FChargerId.Equals(chargepointid) && item.FConnectorId.Equals(ConnectorId)
                            join con in context.TblConnectorStatuses on item.FConnectorId equals con.FId
                            select new MessageLogData
                            {
                                LogId = item.FId,
                                LogDate = item.FDate,
                                LogTime = TimeZoneInfo.ConvertTimeFromUtc(item.FDate.Value, timeZone).ToString("dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("en-US")),
                                ChargerId = item.FChargerId,
                                ConnectorId = con.FConnectorId,
                                Message = item.FMessage,
                                Result = item.FResult,
                                ErrorCode = item.FErrorCode,
                                LogType = item.FLogType,
                                LogState = item.FLogState
                            })
                             .ToList();
                return data;
            }
        }

        public static TblCharger CreateCharger(TblCharger model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Add(model);
                context.SaveChanges();
                return model;
            }
        }
        public static TblCharger UpdateCharger(TblCharger model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Update(model);
                context.SaveChanges();
                return model;
            }
        }
        public static TblConnectorStatus UpdateConnectorStatus(TblConnectorStatus model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Update(model);
                context.SaveChanges();
                return model;
            }
        }
        public static string DeleteCharger(TblCharger model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Remove(model);
                context.SaveChanges();
                return "success";
            }
        }
        public static string DeleteMessgaeLog(List<TblMessageLog> model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.RemoveRange(model);
                context.SaveChanges();
                return "success";
            }
        }
    }
    public partial class ChargerData
    {
        public Guid? ChargerId { get; set; } = null!;
        public string? ShortName { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Comment { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ClientCertThumb { get; set; }
        public Guid? StationId { get; set; }
        public string? StationName { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public byte[]? Image { get; set; }
        public string ActionEdit { get; set; }
        public string ActionDelete { get; set; }
    }
    public partial class ConnectorStatusData
    {
        public Guid? ChargerId { get; set; } = null!;
        public string? ShortName { get; set; }
        public string? ChargerName { get; set; }
        public Guid? FId { get; set; } = null!;
        public int ConnectorId { get; set; }
        public string? ConnectorName { get; set; }
        public string? Code { get; set; }
        public string? Status { get; set; }
        public string? LastStatus { get; set; }
        public DateTime? LastStatusTime { get; set; }
        public double? LastMeter { get; set; }
        public DateTime? LastMeterTime { get; set; }
        public double? StateOfCharge { get; set; }
        public double? CurrentChargeKw { get; set; }
        public string? IsOnline { get; set; }
        public bool? IsServerOnlone { get; set; }
        public string? IsHeartBeat { get; set; }
        public string? HeartBeatlastDate { get; set; }
        public string? Reason { get; set; }
        public string? Comment { get; set; }
        public byte[]? Image { get; set; }
        public string ActionView { get; set; }
        public string ActionStartStop { get; set; }
        public string ActionEdit { get; set; }
    }

    public partial class ConnectorStatusViewDataModel
    {
        public ConnectorStatusView connector { get; set; }
        public TblCharger charge { get; set; }
    }
    public partial class ConnectorStatusViewData
    {
        public Guid? CompanyId { get; set; } = null!;
        public Guid? StationId { get; set; } = null!;
        public string? StationName { get; set; } = null!;
        public Guid? ChargerId { get; set; } = null!;
        public string? ShortName { get; set; }
        public string? ChargerCode { get; set; } = null!;
        public string? ChargerName { get; set; }
        public Guid? Id { get; set; }
        public int? ConnectorId { get; set; }
        public string? ConnectorCode { get; set; }
        public string? ConnectorName { get; set; }
        public string? Status { get; set; }
        public string? LastStatus { get; set; }
        public DateTime? LastStatusTime { get; set; }
        public double? LastMeter { get; set; }
        public DateTime? LastMeterTime { get; set; }
        public string? StartTagId { get; set; }
        public DateTime? StartTime { get; set; }
        public double? MeterStart { get; set; }
        public string? StartResult { get; set; }
        public string? StopTagId { get; set; }
        public DateTime? StopTime { get; set; }
        public double? MeterStop { get; set; }
        public string? StopReason { get; set; }
        public double? CurrentChargeKw { get; set; }
        public double? StateOfCharge { get; set; }
        public Guid? TransactionId { get; set; } = null!;
        public long? TransactionNo { get; set; }
        public byte[]? Image { get; set; }
        public string? IsOnline { get; set; }
        public bool? IsServerOnlone { get; set; }
        public string? IsHeartBeat { get; set; }
        public string? HeartBeatlastDate { get; set; }
        public string? Reason { get; set; }
        public string? Comment { get; set; }
        public string? WebSocketStatus { get; set; }
        public string ActionView { get; set; }
        public string ActionStartStop { get; set; }
        public string ActionEdit { get; set; }
    }
    public partial class MessageLogData
    {
        public Guid LogId { get; set; }

        public string LogTime { get; set; }
        public DateTime? LogDate { get; set; }

        public Guid? ChargerId { get; set; } = null!;
        public string? ChargerName { get; set; } = null!;

        public int? ConnectorId { get; set; }
        public string? ConnectorName { get; set; }

        public string Message { get; set; } = null!;

        public string? Result { get; set; }

        public string? ErrorCode { get; set; }

        public string? LogType { get; set; }

        public string? LogState { get; set; }
    }
    public partial class ConnectorStatusDataModel
    {
        public List<ConnectorStatusData> connectorStatusDatas { get; set; }
        public List<ConnectorStatusViewData> connectorStatusViewDatas { get; set; }
    }
}
