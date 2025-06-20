using System;
using System.Collections.Generic;

namespace chargeme_app.Server.Entities;

public partial class TblChargingMachine
{
    public Guid FId { get; set; }

    public Guid FStationId { get; set; }

    public string FCode { get; set; } = null!;

    public string FName { get; set; } = null!;

    public char? FCurrentStatus { get; set; }

    public TimeOnly FCreated { get; set; }

    public Guid? FCreateby { get; set; }

    public TimeOnly? FUpdated { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public string? FBrand { get; set; }

    public string? FModel { get; set; }

    public string? FCompanyId { get; set; }

    public string? FSupport { get; set; }
}
