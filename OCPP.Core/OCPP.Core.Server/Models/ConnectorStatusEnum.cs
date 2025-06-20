namespace OCPP.Core.Server.Models
{
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
