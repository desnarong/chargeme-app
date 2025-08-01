﻿using csms.DataContext;
using csms.Entities;
using System.ComponentModel.Design;

namespace csms.Models
{
    public class UserInfoModel
    {
        public static List<UserInfo> GetUserDatas()
        {
            using (var context = new NpgsqlDbContext())
            {
                var groups = new List<Guid>() {
                    Guid.Parse("f63caefd-2a43-49da-a6cd-6aa40ce90dd4") ,
                    Guid.Parse("365e6dd9-bfa1-4151-8c7f-42d9139ab73b"),
                    Guid.Empty
                };
                var data = (from user in context.TblUsers.Where(x => x.FStatus == 'Y' && !groups.Contains(x.FUserGroupId))
                            select new UserInfo
                            {
                                Id = user.FId,
                                Address = user.FAddress,
                                City = user.FCity,
                                Email = user.FEmail,
                                GroupName = user.FName,
                                Language = user.FLanguage,
                                Lastlogin = user.FLastlogin,
                                Lastname = user.FLastname,
                                Postcode = user.FPostcode,
                                Province = user.FProvince,
                                CompanyId = user.FCompanyId,
                                UserGroupId = user.FUserGroupId,
                                Username = user.FUsername,
                                Name = user.FName,
                                Mobile = user.FMobile,
                                Status = user.FStatus,
                                StatusView = $"<a></>",
                                Image = user.FImage,
                                ActionReset = $"<a href='JavaScript:ResetPasswordClick(\"{user.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='reset'><i class='fa fa-key'></i></a>",
                                ActionEdit = $"<a href='JavaScript:EditUserModal(\"{user.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='edit'><i class='fa fa-edit'></i></a>",
                                ActionDelete = $"<a href='JavaScript:DeleteUserModalClick(\"{user.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='delete'><i class='fa fa-trash'></i></a>"
                            })
                            .ToList();

                return data;
            }
        }
        public static List<TblUser> GetStationInfo()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblUsers.ToList();
                return data;
            }
        }
        public static TblUser GetStationInfo(Guid id)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblUsers.Where(x => x.FId == id).FirstOrDefault();
                return data;
            }
        }
        public static TblUser Create(TblUser model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Add(model);
                context.SaveChanges();
                return model;
            }
        }
        public static TblUser Update(TblUser model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Update(model);
                context.SaveChanges();
                return model;
            }
        }
        public static string Delete(TblUser model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Remove(model);
                context.SaveChanges();
                return "success";
            }
        }

    }
    public partial class UserInfo
    {
        public Guid Id { get; set; }

        public Guid? CompanyId { get; set; }

        public string? CompanyName { get; set; }

        public Guid UserGroupId { get; set; }

        public string? GroupName { get; set; }

        public string? Name { get; set; }

        public string? Lastname { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? Province { get; set; }

        public string? Postcode { get; set; }

        public string? Mobile { get; set; }

        public string? Email { get; set; }

        public string Language { get; set; } = null!;

        public string? Image { get; set; }

        public Guid? Createby { get; set; }

        public Guid? Updateby { get; set; }

        public char? Status { get; set; }

        public string? Token { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Updated { get; set; }

        public DateTime? Lastlogin { get; set; }

        public string ActionReset { get; set; }

        public string ActionEdit { get; set; }

        public string ActionDelete { get; set; }

        public string StatusView { get; set; }
    }
}
