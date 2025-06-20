using System;
using System.Collections.Generic;

namespace csms.Entities;

public partial class TblConnectorStatus
{
    public Guid FId { get; set; }

    public Guid FStationId { get; set; }

    public string? FCode { get; set; }

    public string? FName { get; set; }

    public string? FCurrentStatus { get; set; }

    public decimal FCurrentMeter { get; set; }

    public decimal FStateOfCharge { get; set; }

    public decimal FCurrentChargeKw { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public Guid? FChargerId { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }

    public DateTime? FCurrentStatusTime { get; set; }

    public DateTime? FCurrentMeterTime { get; set; }

    public int? FConnectorId { get; set; }

    public Guid? FTransactionId { get; set; }
}
