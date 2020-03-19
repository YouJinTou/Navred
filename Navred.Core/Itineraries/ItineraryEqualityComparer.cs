using System.Collections.Generic;

namespace Navred.Core.Itineraries
{
    public class ItineraryEqualityComparer : IEqualityComparer<Itinerary>
    {
        public bool Equals(Itinerary x, Itinerary y)
        {
            return
                x.From == y.From &&
                x.UtcDeparture == y.UtcDeparture &&
                x.To == y.To &&
                x.UtcArrival == y.UtcArrival;
        }

        public int GetHashCode(Itinerary i)
        {
            int prime = 83;
            int result = 1;

            unchecked
            {
                result = result * prime + i.UtcArrival.GetHashCode();
                result = result * prime + i.UtcDeparture.GetHashCode();
                result = result * prime + i.From.GetHashCode();
                result = result * prime + i.To.GetHashCode();
            }

            return result;
        }
    }
}
