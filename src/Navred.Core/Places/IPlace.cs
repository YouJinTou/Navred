using Newtonsoft.Json;

namespace Navred.Core.Places
{
    public interface IPlace
    {
        [JsonIgnore]
        string Country { get; set; }

        string Name { get; set; }

        string RegionCode { get; set; }

        double? Longitude { get; set; }

        double? Latitude { get; set; }

        double DistanceToInKm(IPlace other);
    }
}
