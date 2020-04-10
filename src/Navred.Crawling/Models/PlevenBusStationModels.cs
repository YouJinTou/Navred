using Newtonsoft.Json;

namespace Navred.Crawling.Models.PlevenBusStation
{

    public class Itinerary
    {
        public Options Options { get; set; }
        public Value Value { get; set; }
    }

    public class Options
    {
        public string Classes { get; set; }
    }

    public class Value
    {
        [JsonProperty("1")]
        public string Legs { get; set; }
        [JsonProperty("2")]
        public string FromToWithTime { get; set; }
        [JsonProperty("3")]
        public string OnDays { get; set; }
        [JsonProperty("4")]
        public string Carrier { get; set; }
        [JsonProperty("сектор")]
        public string Sector { get; set; }
        [JsonProperty("___id___")]
        public string Id { get; set; }
    }
}
