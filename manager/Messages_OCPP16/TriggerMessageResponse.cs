﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace manager.Messages_OCPP16
{
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.3.1.0 (Newtonsoft.Json v9.0.0.0)")]
    public partial class TriggerMessageResponse
    {
        [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
        public TriggerMessageResponseStatus Status { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.3.1.0 (Newtonsoft.Json v9.0.0.0)")]
    public enum TriggerMessageResponseStatus
    {
        [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
        Accepted = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"Rejected")]
        Rejected = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"NotImplemented")]
        NotImplemented = 2
    }
}
