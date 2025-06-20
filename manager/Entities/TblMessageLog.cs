using System;
using System.Collections.Generic;

namespace manager.Entities;

public partial class TblMessageLog
{
    public Guid FId { get; set; }

    public Guid? FStationId { get; set; }

    public Guid? FChargerId { get; set; }

    public Guid? FConnectorId { get; set; }

    public string? FMessage { get; set; }

    public string? FResult { get; set; }

    public string? FErrorCode { get; set; }

    public string? FLogType { get; set; }

    public string? FLogState { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }

    public DateTime? FDate { get; set; }
}
