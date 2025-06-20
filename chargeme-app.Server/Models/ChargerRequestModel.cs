namespace chargeme_app.Server.Models
{
    public class ChargerRequestModel
    {
        public required string Chargeid { get; set; }
    }
    public class ChargerPriceShowRequestModel
    {
        public required string Stationid { get; set; }
    }
    public class ChargerPriceCalRequestModel
    {
        public required string Stationid { get; set; }
        public required decimal Amount { get; set; }
        public required string Hour { get; set; }
    }
}
