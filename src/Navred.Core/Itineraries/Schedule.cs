using Navred.Core.Tools;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Itineraries
{
    public class Schedule
    {
        private bool ignoreInvalidRoutes;
        private ICollection<Leg> legs;

        public Schedule(bool ignoreInvalidRoutes = true)
        {
            this.ignoreInvalidRoutes = ignoreInvalidRoutes;
            this.legs = new List<Leg>();
        }

        public void AddLeg(Leg leg)
        {
            Validator.ThrowIfNull(leg);

            if (this.ignoreInvalidRoutes && leg.IsZeroLength())
            {
                return;
            }

            this.legs.Add(leg);
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
            var current = this.legs.ToArray();
            var all = new HashSet<Leg>(current, new LegEqualityComparer());

            for (int t = 0; t < legTimeSpread; t++)
            {
                for (int i = t; i < current.Length; i += legTimeSpread)
                {
                    for (int j = i; j < current.Length; j += legTimeSpread)
                    {
                        var leg = new Leg(
                            from: current[i].From,
                            to: current[j].To,
                            utcDeparture: current[i].UtcDeparture,
                            utcArrival: current[j].UtcArrival,
                            carrier: current[j].Carrier,
                            mode: current[j].Mode,
                            info: current[j].Info,
                            price: current[j].Price,
                            fromSpecific: current[i].FromSpecific,
                            toSpecific: current[j].ToSpecific,
                            arrivalEstimated: current[j].ArrivalEstimated);

                        all.Add(leg);
                    }
                }
            }

            return all;
        }
    }
}
