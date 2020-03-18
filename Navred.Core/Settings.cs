using Navred.Core.Tools;

namespace Navred.Core
{
    public class Settings
    {
        public string ItinerariesTable => Env.ToStage("Itineraries");
    }
}
