using manager.Entities;
using manager.Messages_OCPP16;

namespace manager.Models
{
    public class OCPPViewModel
    {
        public List<TblCharger> ChargePoints { get; set; }
        public List<TblConnectorStatus> ConnectorStatuses { get; set; }
        public List<ResetRequestType> ResetRequestTypes { get; set; }
        public List<ChangeAvailabilityRequestType> ChangeAvailabilityRequestTypes { get; set; }
        public List<ChargingProfilePurpose> ChargingProfilePurposes { get; set; }
        public List<ChargingRateUnit> ChargingRateUnits { get; set; }
        public List<UpdateType> UpdateTypes { get; set; }
    }
}
