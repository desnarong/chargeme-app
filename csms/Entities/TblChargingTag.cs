using System;
using System.Collections.Generic;

namespace csms.Entities;

public partial class TblChargingTag
{
    public Guid FId { get; set; }

    public Guid? FStationId { get; set; }

    public string FCode { get; set; } = null!;

    public string FName { get; set; } = null!;

    public char? FBlocked { get; set; }

    public char? FAuthorize { get; set; }

    public string? FPlateNo { get; set; }

    public string? FModel { get; set; }

    public string? FAgencyId { get; set; }

    public Guid? FUserId { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }

    public Guid? FChargerId { get; set; }

    public Guid? FConnectorId { get; set; }

    public DateTime? FExpiryDate { get; set; }
}
