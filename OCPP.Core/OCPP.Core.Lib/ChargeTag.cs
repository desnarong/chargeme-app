using System;

namespace OCPP.Core.Lib
{
    public partial class ChargeTag
    {
        public string TagId { get; set; }
        public string TagName { get; set; }
        public string ParentTagId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool? Blocked { get; set; }
        public bool? Authorize { get; set; }
        public string ChargePointId { get; set; }
        public int? ConnectorId { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(TagName))
                return TagName;
            else
                return TagId;
        }
    }
}
