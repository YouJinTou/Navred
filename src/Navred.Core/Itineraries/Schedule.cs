﻿using Navred.Core.Places;
using Navred.Core.Tools;
using System;
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

        public IEnumerable<Leg> GetWithChildren()
        {
            var legSpread = this.legs.GroupBy(l => l.UtcDeparture.Date).Select(g => g.Key).Count();
            var current = this.legs.ToArray();
            var all = new HashSet<Leg>(new LegEqualityComparer());

            for (int t = 0; t < legSpread; t++)
            {
                for (int i = t; i < current.Length; i += legSpread)
                {
                    for (int j = i; j < current.Length; j += legSpread)
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
                            arrivalEstimated: current[j].ArrivalEstimated,
                            priceEstimated: current[j].PriceEstimated);

                        all.Add(leg);
                    }
                }
            }

            this.SetPrices(all.ToList());

            return all;
        }

        private void SetPrices(IList<Leg> legs)
        {
            var basePricesByDestination = new Dictionary<Place, decimal?>();

            foreach (var leg in this.legs)
            {
                if (!basePricesByDestination.ContainsKey(leg.To))
                {
                    basePricesByDestination.Add(leg.To, leg.Price);
                }
            }

            var source = this.legs.First().From;

            foreach (var leg in legs)
            {
                if (leg.From.Equals(source))
                {
                    continue;
                }

                var price = leg.Price - basePricesByDestination[leg.From];

                if (price > 0m)
                {
                    leg.Price = price;
                    leg.PriceEstimated = true;
                }
            }
        }
    }
}
