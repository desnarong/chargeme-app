using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCPP.Core.Lib
{
    public class PreTransactions
    {
        public Int32 Id { get; set; }
        public string ChargePointId { get; set; }
        public Int32 ConnectorId { get; set; }
        public Int32? TransactionId { get; set; }
        public double Meter { get; set; }
        public string? TagId { get; set; }
        public Int32 Status { get; set; }
        public Int64 PaymentId { get; set; }
    }
}
