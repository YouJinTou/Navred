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

        public int GetHashCode(Itinerary i)
        {
            int prime = 83;
            int result = 1;

            unchecked
            {
                result = result * prime + i.Arrival.GetHashCode();
                result = result * prime + i.Departure.GetHashCode();
                result = result * prime + i.From.GetHashCode();
                result = result * prime + i.To.GetHashCode();
            }

            return result;
        }
    }
}
