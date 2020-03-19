using System.Collections.Generic;

namespace Navred.Core.Itineraries
{
    public class ItineraryEqualityComparer : IEqualityComparer<Itinerary>
    {
        public bool Equals(Itinerary x, Itinerary y)
        {
            return
                x.From == y.From &&
                x.Departure == y.Departure &&
                x.To == y.To &&
                x.Arrival == y.Arrival;
        }

        public int GetHashCode(Itinerary obj)
        {
            int prime = 83;
            int result = 1;

            unchecked
            {
                result = result * prime + obj.Arrival.GetHashCode();
                result = result * prime + obj.Departure.GetHashCode();
                result = result * prime + obj.From.GetHashCode();
                result = result * prime + obj.To.GetHashCode();
            }

            return result;
        }
    }
}
