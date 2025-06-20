using System;

namespace OCPP.Core.Lib
{
    public partial class Reservation
    {
        public int ReservationID { get; set; }
        public string Uid { get; set; }
        public string ChargePointId { get; set; }
        public int ConnectorId { get; set; }
        public string TagId { get; set; }
        public DateTime ReservationTime { get; set; }
        public DateTime ReservationExpiryTime { get; set; }
        public Boolean Status { get; set; }
        public string StatusReason { get; set; }

        public virtual ChargePoint ChargePoint { get; set; }
    }
}
