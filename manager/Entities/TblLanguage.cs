using System;
using System.Collections.Generic;

namespace manager.Entities;

/// <summary>
/// ภาษา
/// </summary>
public partial class TblLanguage
{
    public Guid FId { get; set; }

    public string FName { get; set; } = null!;

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public string? FCode { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }
}
