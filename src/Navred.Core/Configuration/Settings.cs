using Navred.Core.Tools;

namespace Navred.Core.Configuration
{
    public class Settings
    {
        public string ItinerariesTable => Env.ToStage("Itineraries");

        public string GeocodingApiKey => Env.GetVar("GCP_GEOCODING_API_KEY", isRequired: true);

        public string BuildGeocodingUrl(string address)
        {
            return $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={this.GeocodingApiKey}";
        }
    }
}
