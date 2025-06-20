using System;
using System.Collections.Generic;

namespace manager.Entities;

public partial class TblTransaction
{
    public Guid FId { get; set; }

    public Guid FStationId { get; set; }

    public Guid FUserId { get; set; }

    public Guid FChargerId { get; set; }

    public Guid? FCardId { get; set; }

    public decimal? FMeterStart { get; set; }

    public string? FStartResult { get; set; }

    public decimal? FMeterEnd { get; set; }

    public string? FEndResult { get; set; }

    public char FStatus { get; set; }

    public decimal FCost { get; set; }

    public DateTime FCreated { get; set; }

    public DateTime? FUpdated { get; set; }

    public DateTime? FStartTime { get; set; }

    public DateTime? FEndTime { get; set; }

    public string FTransactionStatus { get; set; } = null!;

    public Guid? FConnectorId { get; set; }

    public long? FTransactionNo { get; set; }

    public decimal? FPreMeter { get; set; }
}
