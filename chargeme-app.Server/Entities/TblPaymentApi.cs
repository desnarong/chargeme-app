using System;
using System.Collections.Generic;

namespace chargeme_app.Server.Entities;

public partial class TblPaymentApi
{
    public Guid FId { get; set; }

    public string FUrl { get; set; } = null!;

    public string FMerchantId { get; set; } = null!;

    public DateTime FCreated { get; set; }

    public Guid? FCreateby { get; set; }

    public DateTime? FUpdated { get; set; }

    public Guid? FUpdateby { get; set; }

    public string FToken { get; set; } = null!;
}
