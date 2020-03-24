using Navred.Core.Tools;
using System.Collections.Generic;

namespace Navred.Core.Itineraries
{
    public class Schedule
    {
        private IList<Leg> legs;

        public Schedule()
        {
            this.legs = new List<Leg>();
        }

        public void AddLeg(Leg Leg)
        {
            Validator.ThrowIfNull(Leg);

            this.legs.Add(Leg);
        }

        public void AddLegs(IEnumerable<Leg> legs)
        {
            foreach (var l in legs)
            {
                this.AddLeg(l);
            }
        }

        public IEnumerable<Leg> GetWithChildren(int legTimeSpread)
        {
            var all = new HashSet<Leg>(new LegEqualityComparer());

            for (int t = 0; t < legTimeSpread; t++)
            {
                for (int i = t; i < this.legs.Count; i += legTimeSpread)
                {
                    for (int j = i; j < this.legs.Count; j += legTimeSpread)
                    {
                        all.Add(new Leg(
                            this.legs[i].From,
                            this.legs[j].To,
                            this.legs[i].UtcDeparture,
                            this.legs[j].UtcArrival,
                            this.legs[i].Carrier,
                            this.legs[i].Mode,
                            this.legs[i].Info,
                            this.GetLegsPrice(i, j, legTimeSpread),
                            this.legs[i].FromSpecific,
                            this.legs[j].ToSpecific));
                    }
                }
            }

            return all;
        }

        private decimal? GetLegsPrice(int start, int end, int legTimeSpread)
        {
            var sum = default(decimal?);

            for (int i = start; i <= end; i += legTimeSpread)
            {
                sum += this.legs[i].Price;
            }

            return sum;
        }
    }
}
