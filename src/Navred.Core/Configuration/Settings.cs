using Navred.Core.Tools;

namespace Navred.Core.Configuration
{
    public class Settings
    {
        public string ItinerariesTable => Env.ToStage("Itineraries");
    }
}
