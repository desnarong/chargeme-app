using System;
using System.Collections.Generic;

namespace csms.Entities;

public partial class TblUser
{
    public Guid FId { get; set; }

    public Guid? FCompanyId { get; set; }

    public Guid FUserGroupId { get; set; }

    public string? FName { get; set; }

    public string? FLastname { get; set; }

    public string? FPassword { get; set; }

    public string? FAddress { get; set; }

    public string? FCity { get; set; }

    public string? FProvince { get; set; }

    public string? FPostcode { get; set; }

    public string? FMobile { get; set; }

    public string? FEmail { get; set; }

    public string FLanguage { get; set; } = null!;

    public string? FImage { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char? FStatus { get; set; }

    public string? FToken { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }

    public DateTime? FLastlogin { get; set; }

    public string? FUsername { get; set; }

    public decimal? FBalanceKwh { get; set; }
}
