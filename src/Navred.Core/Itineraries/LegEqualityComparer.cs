using System.Collections.Generic;

namespace Navred.Core.Itineraries
{
    public class LegEqualityComparer : IEqualityComparer<Leg>
    {
        public bool Equals(Leg x, Leg y)
        {
            return
                x.From == y.From &&
                x.UtcDeparture == y.UtcDeparture &&
                x.To == y.To &&
                x.UtcArrival == y.UtcArrival &&
                x.Carrier == y.Carrier &&
                x.Price == y.Price;
        }

        public int GetHashCode(Leg i)
        {
            int prime = 83;
            int result = 1;

            unchecked
            {
                result = result * prime + i.UtcArrival.GetHashCode();
                result = result * prime + i.UtcDeparture.GetHashCode();
                result = result * prime + i.From.GetHashCode();
                result = result * prime + i.To.GetHashCode();
                result = result * prime + i.Carrier.GetHashCode();
                result = result * prime + i.Price?.GetHashCode() ?? prime;
            }

            return result;
        }
    }
}
