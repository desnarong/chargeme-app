﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace manager.Messages_OCPP16
{
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.3.1.0 (Newtonsoft.Json v9.0.0.0)")]
    public partial class GetDiagnosticsResponse
    {
        [Newtonsoft.Json.JsonProperty("fileName", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public int FileName { get; set; }
    }
}
