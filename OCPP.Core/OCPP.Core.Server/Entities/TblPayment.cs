using System;
using System.Collections.Generic;

namespace OCPP.Core.Server.Entities;

public partial class TblPayment
{
    public Guid FId { get; set; }

    public Guid FTransactionId { get; set; }

    public decimal? FPaymentAmount { get; set; }

    public decimal? FVat { get; set; }

    public decimal? FNet { get; set; }

    public Guid FPaymentMethod { get; set; }

    public string FPaymentStatus { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }

    public string FPaymentCode { get; set; }

    public DateTime? FExpireDate { get; set; }

    public string FQrImage { get; set; }

    public DateTime? FOrderDatetime { get; set; }

    public string FOrderNo { get; set; }
}
