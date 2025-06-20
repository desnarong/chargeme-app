namespace chargeme_app.Server.Models
{
    public class TransactionRequestModel
    {
        public required string Stationid { get; set; }
        public required string Chargerid { get; set; }
        public required string Headerid { get; set; }
        public required decimal Amount { get; set; }
        public required decimal Meter { get; set; }
    }
    public class TransactionResponseModel
    {
        public string status { get; set; }
        public PromptpayDataModel data { get; set; }
    }
    public class PromptpayDataModel
    {
        public string Fid { get; set; }
        public string OrderNo { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Total { get; set; }
        public string Orderdatetime { get; set; }
        public string Expiredate { get; set; }
        public string Image { get; set; }
        public string Detail { get; set; }
        public string StationName { get; set; }
        public string Status { get; set; }
    }
    public class TransactionStatusRequestModel
    {
        public required string Paymentid { get; set; }
    }
    public class TransactionCheckRequestModel
    {
        public required string Fid { get; set; }
    }
    public class TransactionListRequestModel
    {
        public required string Fid { get; set; }
    }
}
