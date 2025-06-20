using System;
using System.Collections.Generic;

namespace manager.Entities;

public partial class TblEventLog
{
    public Guid FId { get; set; }

    public Guid FUserId { get; set; }

    public Guid? FEventTypeId { get; set; }

    public string? FDescription { get; set; }

    public DateTime? FCreated { get; set; }
}
