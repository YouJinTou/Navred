using Navred.Core.Tools;
using System.Collections.Generic;

namespace Navred.Core.Itineraries
{
    public class Schedule
    {
        private bool ignoreInvalidRoutes;
        private IList<Leg> legs;

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
            var all = new HashSet<Leg>(new LegEqualityComparer());

            for (int t = 0; t < legTimeSpread; t++)
            {
                for (int i = t; i < this.legs.Count; i += legTimeSpread)
                {
                    for (int j = i; j < this.legs.Count; j += legTimeSpread)
                    {
                        all.Add(new Leg(
                            from: this.legs[i].From,
                            to: this.legs[j].To,
                            utcDeparture: this.legs[i].UtcDeparture,
                            utcArrival: this.legs[j].UtcArrival,
                            carrier: this.legs[j].Carrier,
                            mode: this.legs[j].Mode,
                            info: this.legs[j].Info,
                            price: this.legs[j].Price,
                            fromSpecific: this.legs[i].FromSpecific,
                            toSpecific: this.legs[j].ToSpecific,
                            arrivalEstimated: this.legs[j].ArrivalEstimated));
                    }
                }
            }

            return all;
        }
    }
}
