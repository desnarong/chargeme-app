using manager.DataContext;
using manager.Entities;

namespace manager.Models
{
    public class CompanyInfoModel
    {
        public static List<CompanyInfoData> GetCompanyInfoes()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from company in context.TblCompanies
                            select new CompanyInfoData
                            {
                                Id = company.FId,
                                Code = company.FCode,
                                Name = company.FName,
                                Address = company.FAddress,
                                City = company.FCity,
                                Country = company.FProvince,
                                Phone = company.FPhone,
                                Email = company.FEmail,
                                PostCode = company.FPostcode,
                                Office = company.FOffice,
                                Mobile = company.FMobile,
                                Status = company.FStatus,
                                Logo = company.FLogo,
                                StatusView = $"<a></>",
                                ActionEdit = $"<a href='JavaScript:EditCompanyModal(\"{company.FId}\")' class='btn btn-soft-secondary btn-sm' title='edit'><i class='ri-edit-box-line la-1-50x'></i></a>",
                                ActionDelete = $"<a href='JavaScript:DeleteCompanyModalClick(\"{company.FId}\")' class='btn btn-soft-secondary btn-sm' title='delete'><i class='ri-delete-bin-line la-1-50x'></i></a>"
                            })
                            .ToList();

                return data;
            }
        }
        public static List<TblCompany> GetCompanyInfo()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblCompanies.ToList();
                return data;
            }
        }
        public static TblCompany GetCompanyInfo(Guid? id)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblCompanies.Where(x => x.FId == id).FirstOrDefault();
                return data;
            }
        }
        public static TblCompany Create(TblCompany model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Add(model);
                context.SaveChanges();
                return model;
            }
        }
        public static TblCompany Update(TblCompany model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Update(model);
                context.SaveChanges();
                return model;
            }
        }
        public static string Delete(TblCompany model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Remove(model);
                context.SaveChanges();
                return "success";
            }
        }
    }
    public class CompanyInfoData
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PostCode { get; set; }
        public string Office { get; set; }
        public string Mobile { get; set; }
        public byte[] Logo { get; set; }
        public string ActionEdit { get; set; }
        public string ActionDelete { get; set; }
        public string ActionDefault { get; set; }
        public char Status { get; set; }
        public string StatusView { get; set; }
    }
}
