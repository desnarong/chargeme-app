using System;
using System.Collections.Generic;

namespace chargeme_app.Server.Entities;

public partial class TblChargerUnit
{
    public Guid FId { get; set; }

    public Guid FStationId { get; set; }

    public string FCode { get; set; } = null!;

    public string FName { get; set; } = null!;

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }
}
