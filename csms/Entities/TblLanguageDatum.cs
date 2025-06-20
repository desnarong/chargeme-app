using System;
using System.Collections.Generic;

namespace csms.Entities;

/// <summary>
/// แปลภาษา
/// </summary>
public partial class TblLanguageDatum
{
    public Guid FId { get; set; }

    public Guid FLanguageId { get; set; }

    public string FName { get; set; } = null!;

    public string FText { get; set; } = null!;

    public string FValue { get; set; } = null!;

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }
}
