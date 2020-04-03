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
                result *= prime + i.UtcArrival.GetHashCode();
                result *= prime + i.UtcDeparture.GetHashCode();
                result *= prime + i.From.GetHashCode();
                result *= prime + i.To.GetHashCode();
                result *= prime + i.Carrier.GetHashCode();
                result *= prime + i.Price?.GetHashCode() ?? prime;
            }

            return result;
        }
    }
}
