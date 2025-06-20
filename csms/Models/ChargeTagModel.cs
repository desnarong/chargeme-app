using csms.DataContext;
using csms.Entities;

namespace csms.Models
{
    public class ChargeTagModel
    {
        public static List<TblChargingTag> GetChargeTags()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblChargingTags.ToList();
                return data;
            }
        }
        public static List<ChargeTagData> GetChargeTagDatas()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = (from chargetag in context.TblChargingTags
                            select new ChargeTagData
                            {
                                TagId = chargetag.FId,
                                TagCode = chargetag.FCode,
                                TagName = chargetag.FName,
                                ParentTagId = chargetag.FAgencyId,
                                ExpiryDate = chargetag.FExpiryDate.Value.ToString("dd/MM/yyyy", new System.Globalization.CultureInfo("en-US")),
                                Blocked = (chargetag.FBlocked ?? 'N') == 'Y' ? true : false,
                                BlockedStatus = ((chargetag.FBlocked ?? 'N') == 'N' ? "<i class='fa-solid fa-ban fa-2x color-broken'></i>" : "<i class='fa-solid fa-circle-check fa-2x color-online'></i>"),
                                Authorize = chargetag.FAuthorize == 'N' ? false : true,
                                //ChargerId = chargetag.FChagerId,
                                PlateNo = chargetag.FPlateNo,
                                //ConnectorId = chargetag.FConnectorId,
                                CustomerName = chargetag.FName,
                                //AgencyName = chargetag.,
                                ActionEdit = $"<a href='JavaScript:EditChargeTagModal(\"{chargetag.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='edit'><i class='fa fa-edit'></i></a>",
                                ActionDelete = $"<a href='JavaScript:DeleteChargeTagModalClick(\"{chargetag.FId}\")' class='btn btn-clean btn-icon btn-icon-md' title='delete'><i class='fa fa-trash'></i></a>"
                            })
                             .ToList();
                return data;
            }
        }
        public static TblChargingTag GetChargeTag(Guid tagid)
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblChargingTags.Where(x => x.FId.Equals(tagid)).FirstOrDefault();
                return data;
            }
        }
        public static TblChargingTag CreateChargeTag(TblChargingTag model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Add(model);
                context.SaveChanges();
                return model;
            }
        }
        public static TblChargingTag UpdateChargeTag(TblChargingTag model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Update(model);
                context.SaveChanges();
                return model;
            }
        }
        public static string DeleteChargeTag(TblChargingTag model)
        {
            using (var context = new NpgsqlDbContext())
            {
                context.Remove(model);
                context.SaveChanges();
                return "success";
            }
        }
    }
    public class ChargeTagData
    {
        public Guid? TagId { get; set; } = null!;
        public string TagCode { get; set; } = null!;
        public string? TagName { get; set; }
        public string? ParentTagId { get; set; }
        public string? ExpiryDate { get; set; }
        public bool? Blocked { get; set; }
        public string? BlockedStatus { get; set; }
        public bool? Authorize { get; set; }
        public string? ChargerId { get; set; }
        public int? ConnectorId { get; set; }
        public string? CustomerName { get; set; }
        public string? PlateNo { get; set; }
        public string? AgencyName { get; set; }
        public string ActionEdit { get; set; }
        public string ActionDelete { get; set; }
    }
    public class ChargeTagModelData
    {
        public List<ConnectorStatusData> connectorStatusDatas { get; set; }
    }
}
