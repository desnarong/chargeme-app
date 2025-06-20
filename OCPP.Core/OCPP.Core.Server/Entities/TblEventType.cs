using System;
using System.Collections.Generic;

namespace OCPP.Core.Server.Entities;

public partial class TblEventType
{
    public Guid FId { get; set; }

    public string FName { get; set; }

    public string FDescription { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }
}
