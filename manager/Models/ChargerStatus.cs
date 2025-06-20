namespace manager.Models
{
    public class ChargerStatus
    {
        public ChargerStatus()
        {
            OnlineConnectors = new Dictionary<int, OnlineConnectorStatus>();
        }

        /// <summary>
        /// ID of chargepoint
        /// </summary>
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Name of chargepoint
        /// </summary>
        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// OCPP protocol version
        /// </summary>
        [Newtonsoft.Json.JsonProperty("protocol")]
        public string Protocol { get; set; }

        public string Heartbeat { get; set; }

        /// <summary>
        /// WebSocket status
        /// </summary>
        [Newtonsoft.Json.JsonProperty("webSocketStatus")]
        public string WebSocketStatus { get; set; }
        /// <summary>
        /// Dictionary with online connectors
        /// </summary>
        public Dictionary<int, OnlineConnectorStatus> OnlineConnectors { get; set; }
    }/// <summary>
     /// Encapsulates details about online charge point connectors
     /// </summary>
    public class OnlineConnectorStatus
    {
        /// <summary>
        /// Status of charge connector
        /// </summary>
        public ConnectorStatusEnum Status { get; set; }

        /// <summary>
        /// Current charge rate in kW
        /// </summary>
        public double? ChargeRateKW { get; set; }

        /// <summary>
        /// Current meter value in kWh
        /// </summary>
        public double? MeterKWH { get; set; }

        /// <summary>
        /// StateOfCharges in percent
        /// </summary>
        public double? SoC { get; set; }
    }

    public enum ConnectorStatusEnum
    {
        [System.Runtime.Serialization.EnumMember(Value = @"")]
        Undefined = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"Available")]
        Available = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"Occupied")]
        Occupied = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"Unavailable")]
        Unavailable = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"Faulted")]
        Faulted = 4,

        [System.Runtime.Serialization.EnumMember(Value = @"Preparing")]
        Preparing = 5,

        [System.Runtime.Serialization.EnumMember(Value = @"Charging")]
        Charging = 6,

        [System.Runtime.Serialization.EnumMember(Value = @"Finishing")]
        Finishing = 7,

        [System.Runtime.Serialization.EnumMember(Value = @"Reserved")]
        Reserved = 8
    }
}
