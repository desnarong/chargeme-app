using System;
using System.Collections.Generic;

namespace manager.Entities;

public partial class TblChargerPriceShow
{
    public Guid FId { get; set; }

    public Guid FStationId { get; set; }

    public Guid FChargerUnitId { get; set; }

    public decimal FValue { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }

    public string? FText { get; set; }
}
