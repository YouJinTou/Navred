using Newtonsoft.Json;

namespace Navred.Crawling.Models.PlevenBusStation
{
    public interface Itinerary
    {
        Value Ref { get; set; }
    }

    public interface Value
    {
        string Legs { get; set; }
        string FromToWithTime { get; set; }
        string OnDays { get; set; }
        string Carrier { get; set; }
        string Sector { get; set; }
        string Id { get; set; }
    }

    public class Departure : Itinerary
    {
        public DepartureOptions Options { get; set; }
        public DepartureValue Value { get; set; }
        Value Itinerary.Ref { get => this.Value; set => throw new System.NotImplementedException(); }
    }

    public class DepartureOptions
    {
        public string Classes { get; set; }
    }

    public class DepartureValue : Value
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


    public class Arrival : Itinerary
    {
        public ArrivalOptions Options { get; set; }
        public ArrivalValue Value { get; set; }
        Value Itinerary.Ref { get => this.Value; set => throw new System.NotImplementedException(); }
    }

    public class ArrivalOptions
    {
        public string classes { get; set; }
    }

    public class ArrivalValue : Value
    {
        [JsonProperty("1")]
        public string Legs { get; set; }
        [JsonProperty("2")]
        public string FromToWithTime { get; set; }
        [JsonProperty("3")]
        public string Sector { get; set; }
        [JsonProperty("4")]
        public string OnDays { get; set; }
        [JsonProperty("5")]
        public string Carrier { get; set; }
        [JsonProperty("___id___")]
        public string Id { get; set; }
    }

}
