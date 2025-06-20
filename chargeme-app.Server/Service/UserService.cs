using chargeme_app.Server.DataContext;
using chargeme_app.Server.Entities;
using chargeme_app.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace chargeme_app.Server.Service
{
    public class UserService
    {
        private readonly NpgsqlDbContext _context;

        public UserService(NpgsqlDbContext context)
        {
            _context = context;
        }

        // ฟังก์ชันตัวอย่างในการตรวจสอบว่ามี email ในฐานข้อมูลหรือไม่
        public bool CheckIfEmailExists(string email)
        {
            // คุณสามารถใช้การเชื่อมต่อฐานข้อมูลเพื่อตรวจสอบ email ได้ที่นี่
            return  _context.TblUsers.Any(u => u.FEmail == email);
        }
        // ตรวจสอบใน database ว่ามี fid นี้หรือไม่
        public async Task<TblUser> CheckExistingUser(Guid  fid)
        {
            var existingUser = await _context.TblUsers.FirstOrDefaultAsync(u => u.FId == fid);
            return existingUser;
        }
        public async Task<TblLanguage> Language(Guid fid)
        {
            var lang = await _context.TblLanguages.FirstOrDefaultAsync(u => u.FId == fid);
            return lang;
        }
        // ดำเนินการสร้าง user ใหม่
        public async Task<TblUser> NewUser(RegisterUserRequest model)
        {
            
            var newUser = new TblUser
            {
                FEmail = model.Email,
                FPassword = model.Password,
            };

            _context.TblUsers.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }
        // ดำเนินการสร้าง user ใหม่ แบบ update จาก fid
        public async Task<TblUser> UpdateUser(TblUser model)
        {
            _context.TblUsers.Update(model);
            await _context.SaveChangesAsync();

            return model;
        }
        public async Task<TblUser> GetUser(Guid id)
        {
            var model = await _context.TblUsers.FirstOrDefaultAsync(x => x.FId == id);
            return model;
        }
    }
}
