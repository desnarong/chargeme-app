using System;
using System.Collections.Generic;

namespace OCPP.Core.Server.Entities;

/// <summary>
/// แปลภาษา
/// </summary>
public partial class TblLanguageDatum
{
    public Guid FId { get; set; }

    public Guid FLanguageId { get; set; }

    public string FName { get; set; }

    public string FText { get; set; }

    public string FValue { get; set; }

    public Guid? FCreateby { get; set; }

    public Guid? FUpdateby { get; set; }

    public char FStatus { get; set; }

    public DateTime? FCreated { get; set; }

    public DateTime? FUpdated { get; set; }
}
