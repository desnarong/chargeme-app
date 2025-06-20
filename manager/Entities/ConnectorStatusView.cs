using System;
using System.Collections.Generic;

namespace manager.Entities;

public partial class ConnectorStatusView
{
    public Guid? FId { get; set; }

    public Guid? FChargerId { get; set; }

    public string? FShortName { get; set; }

    public string? FChargerName { get; set; }

    public int? FConnectorId { get; set; }

    public string? FCode { get; set; }

    public string? FName { get; set; }

    public string? FCurrentStatus { get; set; }

    public DateTime? FCurrentStatusTime { get; set; }

    public decimal? FCurrentMeter { get; set; }

    public DateTime? FCurrentMeterTime { get; set; }

    public Guid? FCardId { get; set; }

    public DateTime? FStartTime { get; set; }

    public decimal? FMeterStart { get; set; }

    public string? FStartResult { get; set; }

    public DateTime? FEndTime { get; set; }

    public decimal? FMeterEnd { get; set; }

    public string? FEndResult { get; set; }

    public decimal? FCurrentChargeKw { get; set; }

    public decimal? FStateOfCharge { get; set; }

    public long? FTransactionNo { get; set; }

    public Guid? FStationId { get; set; }
}
