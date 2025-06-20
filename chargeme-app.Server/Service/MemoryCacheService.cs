using chargeme_app.Server.DataContext;
using chargeme_app.Server.Entities;
using chargeme_app.Server.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace chargeme_app.Server.Service
{
    public class MemoryCacheService
    {
        private readonly NpgsqlDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(10); // ตั้งเวลาแคชเป็น 10 นาที
        public MemoryCacheService(NpgsqlDbContext context, IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _context = context;
        }
        public async Task<object> GetDataAsync(Guid fid)
        {
            await RefreshCacheIfDatabaseUpdated(fid);

            // ตรวจสอบข้อมูลในแคชก่อน
            if (!_cache.TryGetValue(fid, out object cachedData))
            {
                // ถ้าไม่มีข้อมูลในแคช ดึงข้อมูลจากฐานข้อมูล
                cachedData = await GetDataFromDatabaseAsync(fid);

                // เพิ่มข้อมูลลงแคช
                _cache.Set(fid, cachedData, _cacheDuration);
            }

            return cachedData;
        }
        public void ClearData(Guid fid)
        {
            // ตรวจสอบข้อมูลในแคชก่อน
            if (_cache.TryGetValue(fid, out _))
            {
                // ลบข้อมูลแคช
                _cache.Remove(fid);
            }
        }
        private async Task<object> GetDataFromDatabaseAsync(Guid fid)
        {
            // ดึงข้อมูลจากฐานข้อมูลแบบ Async
            var trans = await _context.TblTransactions.FirstOrDefaultAsync(x => x.FId == fid);
            if (trans == null) return new { error = "Transaction not found" };

            var status = await _context.TblConnectorStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FId == trans.FConnectorId);
            if (status == null) return new { error = "Connector status not found" };

            var station = await _context.TblStations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FId == status.FStationId);
            if (station == null) return new { error = "Station not found" };

            // ตรวจสอบเวลาเพื่อคำนวณอัตราค่าใช้จ่าย
            DateTime currentTime = DateTime.Now;
            decimal rate = HelpApp.IsOnPeak(currentTime) ? (station.FOnpeak ?? 0) : (station.FOffpeak ?? 0);

            // คำนวณค่าใช้จ่าย
            trans.FCost = HelpApp.CalculatePrice(status.FCurrentMeter, rate);

            // บันทึกการเปลี่ยนแปลง
            _context.TblTransactions.Update(trans);
            await _context.SaveChangesAsync();

            return new { data = status, transdata = trans };
        }

        public async Task RefreshCacheIfDatabaseUpdated(Guid fid)
        {
            // ตรวจสอบว่า database มีข้อมูลอัปเดตหรือไม่ เช่นตรวจสอบด้วย timestamp ล่าสุด
            bool isDatabaseUpdated = CheckDatabaseUpdate(fid);

            if (isDatabaseUpdated)
            {
                var newData = await GetDataFromDatabaseAsync(fid);
                _cache.Set(fid, newData, _cacheDuration);
            }
        }
        private bool CheckDatabaseUpdate(Guid fid)
        {
            if (_cache.TryGetValue(fid, out object cachedData))
            {
                var data = ((dynamic)cachedData).data;
                var _data = ((TblConnectorStatus)data);
                var status = _context.TblConnectorStatuses.Any(x => x.FId == _data.FId && x.FCurrentStatusTime > _data.FCurrentStatusTime);
                return status;
            }
            return true;
        }
        #region payment
        public async Task<TblPayment> GetPaymentAsync(Guid fid)
        {
            // ตรวจสอบข้อมูลในแคชก่อน
            if (!_cache.TryGetValue($"pay_{fid}", out TblPayment cachedData))
            {
                // ถ้าไม่มีข้อมูลในแคช ดึงข้อมูลจากฐานข้อมูล
                cachedData = await GetPaymentFromDatabaseAsync(fid);

                // เพิ่มข้อมูลลงแคช
                _cache.Set($"pay_{fid}", cachedData, _cacheDuration);
            }

            return cachedData;
        }
        public void ClearPayment(Guid fid)
        {
            // ตรวจสอบข้อมูลในแคชก่อน
            if (_cache.TryGetValue($"pay_{fid}", out _))
            {
                // ลบข้อมูลแคช
                _cache.Remove($"pay_{fid}");
            }
        }
        private async Task<TblPayment> GetPaymentFromDatabaseAsync(Guid fid)
        {
            // ดึงข้อมูลจากฐานข้อมูล (เชื่อมต่อฐานข้อมูลที่นี่)
            await Task.Delay(100); // สมมติว่ามีการดึงข้อมูล (ทดแทนด้วยการ query จริง)
            var payment = _context.TblPayments.First(x => x.FId == fid);
            return payment;
        }
        public async Task RefreshCachePaymentIfDatabaseUpdated(Guid fid)
        {
            // ตรวจสอบว่า database มีข้อมูลอัปเดตหรือไม่ เช่นตรวจสอบด้วย timestamp ล่าสุด
            bool isDatabaseUpdated = CheckPaymentDatabaseUpdate(fid);

            if (isDatabaseUpdated)
            {
                var newData = await GetPaymentFromDatabaseAsync(fid);
                _cache.Set($"pay_{fid}", newData, _cacheDuration);
            }
        }
        private bool CheckPaymentDatabaseUpdate(Guid fid)
        {
            if (_cache.TryGetValue($"pay_{fid}", out TblPayment cachedData))
            {
                var status = _context.TblPayments.Any(x => x.FId == cachedData.FId && x.FPaymentStatus != cachedData.FPaymentStatus);
                return status;
            }
            return true;
        }
        #endregion
    }
}
