using Navred.Core.Extensions;
using Navred.Core.Places;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Itineraries
{
    public class Schedule
    {
        private readonly IDictionary<Place, IList<Leg>> legsByPlace;
        private readonly IDictionary<Place, DateTime> currentArrivals;

        public Schedule()
        {
            this.legsByPlace = new Dictionary<Place, IList<Leg>>();
            this.currentArrivals = new Dictionary<Place, DateTime>();
        }

        private int LegSpread
        {
            get
            {
                var groupCounts = this.legsByPlace.Select(kvp => kvp.Value.Count).Distinct();

                if (groupCounts.Count() > 1)
                {
                    throw new InvalidOperationException("Invalid leg spread.");
                }

                if (groupCounts.IsEmpty())
                {
                    return 0;
                }

                return groupCounts.First();
            }
        }

        public void AddLeg(Leg leg)
        {
            leg.ThrowIfNull("Leg empty.");

            if (leg.IsZeroLength())
            {
                throw new InvalidOperationException("Leg is zero length.");
            }

            foreach (var kvp in this.legsByPlace)
            {
                if (kvp.Key.Equals(leg.From))
                {
                    continue;
                }

                var comparableIndex = this.legsByPlace.ContainsKey(leg.From) ?
                    this.legsByPlace[leg.From].Count : 0;
                var targetComparable = kvp.Value[comparableIndex];

                if (targetComparable.UtcArrival > leg.UtcDeparture)
                {
                    throw new InvalidOperationException("Leg timeline mismatch.");
                }
            }

            if (this.legsByPlace.ContainsKey(leg.From))
            {
                this.legsByPlace[leg.From].Add(leg);
                this.currentArrivals[leg.From] = leg.UtcArrival;
            }
            else
            {
                this.legsByPlace.Add(leg.From, new List<Leg> { leg });

                this.currentArrivals.Add(leg.From, leg.UtcArrival);
            }
        }

        public void AddLegs(IEnumerable<Leg> legs)
        {
            foreach (var l in legs)
            {
                this.AddLeg(l);
            }
        }

        public IEnumerable<Leg> Permute()
        {
            var legs = this.legsByPlace.SelectMany(l => l.Value).ToArray();
            var all = new HashSet<Leg>(new LegEqualityComparer());

            for (int t = 0; t < this.LegSpread; t++)
            {
                for (int i = t; i < legs.Length; i += this.LegSpread)
                {
                    for (int j = i; j < legs.Length; j += this.LegSpread)
                    {
                        var leg = new Leg(
                            from: legs[i].From,
                            to: legs[j].To,
                            utcDeparture: legs[i].UtcDeparture,
                            utcArrival: legs[j].UtcArrival,
                            carrier: legs[j].Carrier,
                            mode: legs[j].Mode,
                            info: legs[j].Info,
                            price: legs[j].Price,
                            fromSpecific: legs[i].FromSpecific,
                            toSpecific: legs[j].ToSpecific,
                            departureEstimated: legs[i].DepartureEstimated,
                            arrivalEstimated: legs[j].ArrivalEstimated,
                            priceEstimated: legs[j].PriceEstimated);

                        all.Add(leg);
                    }
                }
            }

            this.SetPrices(all);

            return all;
        }

        private void SetPrices(ICollection<Leg> legs)
        {
            var priceByPlace = this.legsByPlace.ToDictionary(
                kvp => kvp.Value.First().To, kvp => kvp.Value.First().Price);

            foreach (var leg in legs)
            {
                if (!priceByPlace.ContainsKey(leg.From) || !priceByPlace.ContainsKey(leg.To))
                {
                    continue;
                }

                var sourcePrice = priceByPlace[leg.From];
                var destinationPrice = priceByPlace[leg.To];
                leg.Price = (sourcePrice == null || destinationPrice == null) ? 
                    null : destinationPrice - sourcePrice;
            }
        }
    }
}
