using System;
using System.Collections.Generic;

namespace manager.Entities;

public partial class TblPaymentMethod
{
    public Guid FId { get; set; }

    public Guid? FStationId { get; set; }

    public string FName { get; set; } = null!;

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }
}
