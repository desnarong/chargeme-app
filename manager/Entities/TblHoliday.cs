using System;
using System.Collections.Generic;

namespace manager.Entities;

public partial class TblHoliday
{
    public Guid FId { get; set; }

    public Guid? FStationId { get; set; }

    public string FName { get; set; } = null!;

    public string? FDescription { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FDay { get; set; }

    public DateTime? FUpdated { get; set; }

    public DateTime? FCreated { get; set; }
}
