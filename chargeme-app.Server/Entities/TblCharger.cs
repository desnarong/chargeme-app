using System;
using System.Collections.Generic;

namespace chargeme_app.Server.Entities;

public partial class TblCharger
{
    public Guid FId { get; set; }

    public Guid FStationId { get; set; }

    public string FCode { get; set; } = null!;

    public string FName { get; set; } = null!;

    public char? FCurrentStatus { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public string? FBrand { get; set; }

    public string? FModel { get; set; }

    public string? FSupport { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }

    public string? FShortName { get; set; }

    public string? FUsername { get; set; }

    public string? FPassword { get; set; }

    public string? FClientCertThumb { get; set; }

    public string? FComment { get; set; }

    public byte[]? FImage { get; set; }
}
