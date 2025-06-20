using csms.DataContext;
using csms.Entities;
using System.ComponentModel.Design;

namespace csms.Models
{
    public class StationInfoModel
    {
        public static List<StationInfoData> GetStationInfoes()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from station in context.TblStations
                            join company in context.TblCompanies on station.FCompanyId equals company.FId into companytable
                            from companyinfo in companytable.DefaultIfEmpty()
                            where station.FStatus == 'Y'
                            select new StationInfoData
                            {
                                Id = station.FId,
                                CompanyId = companyinfo.FId,
                                CompanyName = companyinfo.FName,
                                Code = station.FCode,
                                Name = station.FName,
                                Address = station.FAddress,
                                City = station.FCity,
                                Country = station.FProvince,
                                Phone = station.FPhone,
                                Email = station.FEmail,
                                //StreetName = station.StreetName,
                                PostCode = station.FPostcode,
                                Office = station.FOffice,
                                Mobile = station.FMobile,
                                Status = station.FStatus,
                                StatusView = $"<a></>",
                                OnPeak = Convert.ToDouble(station.FOnpeak),
                                OffPeak = Convert.ToDouble(station.FOffpeak),
                                Image = station.FImage,
                                Logo = station.FLogo,
                                Rfid = station.FRfid,
                                Lat = $"{station.FLatitude}",
                                Long = $"{station.FLongitude}",
                                Chargertype = $"{station.FChagerType}",
                                ActionDefault = $"<i class='fa-solid fa-circle-check fa-2x color-online'></i>",
                                ElectricityRate = 
                                    station.FChagerType == 1 ? $"<a href='JavaScript:ElectricityrateModal(\"{station.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='rate'><i class='fa-solid fa-cash-register'></i></a>"
                                    : $"<a href='JavaScript:HourrateModal(\"{station.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='rate'><i class='fa-solid fa-cash-register'></i></a>",
                                ActionEdit = $"<a href='JavaScript:EditStationModal(\"{station.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='edit'><i class='fa fa-edit'></i></a>",
                                ActionDelete = $"<a href='JavaScript:DeleteStationModalClick(\"{station.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='delete'><i class='fa fa-trash'></i></a>"
                            })
                            .ToList();

                return data;
            }
        }
        public static List<StationInfoData> GetStationInfoes(Guid? compayid)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from station in context.TblStations
                            join company in context.TblCompanies on station.FCompanyId equals company.FId into companytable
                            from companyinfo in companytable.DefaultIfEmpty()
                            where station.FStatus == 'Y' && companyinfo.FId == compayid
                            select new StationInfoData
                            {
                                Id = station.FId,
                                CompanyId = companyinfo.FId,
                                CompanyName = companyinfo.FName,
                                Code = station.FCode,
                                Name = station.FName,
                                Address = station.FAddress,
                                City = station.FCity,
                                Country = station.FProvince,
                                Phone = station.FPhone,
                                Email = station.FEmail,
                                //StreetName = station.StreetName,
                                PostCode = station.FPostcode,
                                Office = station.FOffice,
                                Mobile = station.FMobile,
                                Status = station.FStatus,
                                StatusView = $"<a></>",
                                OnPeak = Convert.ToDouble(station.FOnpeak),
                                OffPeak = Convert.ToDouble(station.FOffpeak),
                                Image = station.FImage,
                                Logo = station.FLogo,
                                Rfid = station.FRfid,
                                ActionDefault = $"<i class='fa-solid fa-circle-check fa-2x color-online'></i>",
                                ElectricityRate = $"<a href='JavaScript:ElectricityrateModal(\"{station.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='rate'><i class='fa-solid fa-cash-register'></i></a>",
                                ActionEdit = $"<a href='JavaScript:EditStationModal(\"{station.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='edit'><i class='fa fa-edit'></i></a>",
                                ActionDelete = $"<a href='JavaScript:DeleteStationModalClick(\"{station.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='delete'><i class='fa fa-trash'></i></a>"
                            })
                            .ToList();

                return data;
            }
        }
        public static List<TblStation> GetStationInfo()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblStations.ToList();
                return data;
            }
        }
        public static TblStation GetStationInfo(Guid id)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblStations.Where(x => x.FId == id).FirstOrDefault();
                return data;
            }
        }
        public static TblStation Create(TblStation model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Add(model);
                context.SaveChanges();
                return model;
            }
        }
        public static TblStation Update(TblStation model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Update(model);
                context.SaveChanges();
                return model;
            }
        }
        public static string Delete(TblStation model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Remove(model);
                context.SaveChanges();
                return "success";
            }
        }

        /*Holiday*/
        public static List<HolidayData> GetHolidayDatas(Guid stationid)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from holiday in context.TblHolidays
                            where holiday.FStationId == stationid
                            select new HolidayData
                            {
                                StationId = holiday.FStationId ?? Guid.Empty,
                                Id = holiday.FId,
                                Day = holiday.FDay.Value.ToString("dd/MM/yyyy", new System.Globalization.CultureInfo("en-US")),
                                Name = holiday.FName,
                                Description = holiday.FDescription,
                                ActionEdit = $"<a href='JavaScript:EditHolidayModal(\"{holiday.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='edit'><i class='fa fa-edit'></i></a>",
                                ActionDelete = $"<a href='JavaScript:DeleteHolidayClick(\"{holiday.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='delete'><i class='fa fa-trash'></i></a>"
                            })
                            .ToList();

                return data;
            }
        }
        public static List<TblHoliday> GetHolidays(Guid stationid)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblHolidays.Where(x => x.FStationId == stationid).ToList();
                return data;
            }
        }
        public static TblHoliday GetHoliday(Guid id)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblHolidays.Where(x => x.FId == id).FirstOrDefault();
                return data;
            }
        }
        public static TblHoliday Create(TblHoliday model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Add(model);
                context.SaveChanges();
                return model;
            }
        }
        public static TblHoliday Update(TblHoliday model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Update(model);
                context.SaveChanges();
                return model;
            }
        }
        public static string Delete(TblHoliday model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Remove(model);
                context.SaveChanges();
                return "success";
            }
        }

        public static List<ChargerPriceShowData> GetChargerPriceShows(Guid stationid)
        {
            using (var context = new NpgsqlDbContext())
            {
                var prices = from p in context.TblChargerPriceShows
                             join s in context.TblStations on p.FStationId equals s.FId
                             join u in context.TblChargerUnits on s.FUnitId equals u.FId
                             where p.FStationId == stationid
                             orderby p.FValue
                             select new ChargerPriceShowData
                             {
                                 Value = $"{p.FValue}",
                                 Text = $"{p.FText}",
                                 Id = p.FId,
                                 UnitId = u.FId,
                                 Unit = u.FName,
                                 ActionValue = $"<input id='{p.FId}' value='{p.FValue}' data-text='{p.FText}' class='form-control'>"
                             };
                return prices.ToList();
            }
        }
        public static TblChargerPriceShow GetChargerPrice(Guid? id)
        {
            using (var context = new NpgsqlDbContext())
            {
                return context.TblChargerPriceShows.FirstOrDefault(x => x.FId == id);
            }
        }
        public static TblChargerPriceShow Create(TblChargerPriceShow model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Add(model);
                context.SaveChanges();
                return model;
            }
        }
        public static TblChargerPriceShow Update(TblChargerPriceShow model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Update(model);
                context.SaveChanges();
                return model;
            }
        }
        public static string Delete(TblChargerPriceShow model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Remove(model);
                context.SaveChanges();
                return "success";
            }
        }
    }
    public partial class ChargerPriceShowData
    {
        public Guid? Id { get; set; } = null!;
        public string? Text { get; set; }
        public string? Value { get; set; }
        public Guid? UnitId { get; set; }
        public string? Unit { get; set; }
        public string? ActionValue { get; set; }
    }
    public class StationInfoData
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string StreetName { get; set; }
        public string PostCode { get; set; }
        public string Office { get; set; }
        public string Mobile { get; set; }
        public string ElectricityRate { get; set; }
        public string ActionHoliday { get; set; }
        public string ActionEdit { get; set; }
        public string ActionDelete { get; set; }
        public string ActionDefault { get; set; }
        public bool? IsDefault { get; set; }
        public char Status { get; set; }
        public string StatusView { get; set; }
        public double? OnPeak { get; set; }
        public double? OffPeak { get; set; }
        public byte[]? Image { get; set; }
        public byte[]? Logo { get; set; }
        public string? Rfid { get; set; }
        public string? Chargertype { get; set; }
        public string? Lat { get; set; }
        public string? Long { get; set; }
    }
    public class ElectricityRateData
    {
        public long RateId { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public double? OnPeak { get; set; }
        public double? OffPeak { get; set; }
    }
    public class HolidayData
    {
        public Guid StationId { get; set; }
        public Guid Id { get; set; }
        public string Day { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ActionEdit { get; set; }
        public string ActionDelete { get; set; }
    }
}
