using System;

namespace OCPP.Core.Lib
{
    public partial class ConnectorStatus
    {
        public string ChargePointId { get; set; }
        public int ConnectorId { get; set; }
        public string ConnectorName { get; set; }
        public string LastStatus { get; set; }
        public DateTime? LastStatusTime { get; set; }
        public double? LastMeter { get; set; }
        public DateTime? LastMeterTime { get; set; }
        public virtual ChargePoint ChargePoint { get; set; }
        public double? StateOfCharge { get; set; }
        public double? CurrentChargeKW { get; set; }

        public override string ToString()
        {
            string chargePointName = ChargePoint?.Name ?? ChargePointId;
            if (!string.IsNullOrEmpty(ConnectorName))
            {
                return ConnectorName;
            }

            return $"{chargePointName}:{ConnectorId}";
        }
    }
}
