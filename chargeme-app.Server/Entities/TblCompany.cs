using System;
using System.Collections.Generic;

namespace chargeme_app.Server.Entities;

/// <summary>
/// สำนักงาน
/// </summary>
public partial class TblCompany
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

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }

    public byte[]? FLogo { get; set; }
}
