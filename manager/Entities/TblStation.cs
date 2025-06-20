using System;
using System.Collections.Generic;

namespace manager.Entities;

/// <summary>
/// สถานีชาร์จ
/// </summary>
public partial class TblStation
{
    public Guid FId { get; set; }

    public string FCode { get; set; } = null!;

    public string FName { get; set; } = null!;

    public string? FAddress { get; set; }

    public string? FCity { get; set; }

    public string? FProvince { get; set; }

    public string? FPostcode { get; set; }

    public string? FPhone { get; set; }

    public string? FFax { get; set; }

    public string? FOffice { get; set; }

    public string? FMobile { get; set; }

    public string? FWebsite { get; set; }

    public string? FEmail { get; set; }

    public string? FVatnumber { get; set; }

    public char? FIsvat { get; set; }

    public string? FInvoicenoprefix { get; set; }

    public string? FQuotenoprefix { get; set; }

    public string? FTermsandcondition { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public decimal? FMinimumAmount { get; set; }

    public Guid? FUnitId { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }

    public decimal? FOnpeak { get; set; }

    public decimal? FOffpeak { get; set; }

    public string? FRfid { get; set; }

    public byte[]? FImage { get; set; }

    public Guid? FCompanyId { get; set; }

    public long? FLatitude { get; set; }

    public long? FLongitude { get; set; }

    public int? FChagerType { get; set; }

    public byte[]? FLogo { get; set; }
}
