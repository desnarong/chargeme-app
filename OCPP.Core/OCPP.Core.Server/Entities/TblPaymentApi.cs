using System;
using System.Collections.Generic;

namespace OCPP.Core.Server.Entities;

public partial class TblPaymentApi
{
    public Guid FId { get; set; }

    public string FUrl { get; set; }

    public string FMerchantId { get; set; }

    public DateTime FCreated { get; set; }

    public Guid? FCreateby { get; set; }

    public DateTime? FUpdated { get; set; }

    public Guid? FUpdateby { get; set; }

    public string FToken { get; set; }
}
