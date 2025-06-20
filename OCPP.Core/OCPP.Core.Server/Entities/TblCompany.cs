using System;
using System.Collections.Generic;

namespace OCPP.Core.Server.Entities;

/// <summary>
/// สำนักงาน
/// </summary>
public partial class TblCompany
{
    public Guid FId { get; set; }

    public string FCode { get; set; }

    public string FName { get; set; }

    public string FAddress { get; set; }

    public string FCity { get; set; }

    public string FProvince { get; set; }

    public string FPostcode { get; set; }

    public string FPhone { get; set; }

    public string FFax { get; set; }

    public string FOffice { get; set; }

    public string FMobile { get; set; }

    public string FWebsite { get; set; }

    public string FEmail { get; set; }

    public string FVatnumber { get; set; }

    public char? FIsvat { get; set; }

    public string FInvoicenoprefix { get; set; }

    public string FQuotenoprefix { get; set; }

    public string FLogo { get; set; }

    public string FTermsandcondition { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }
}
