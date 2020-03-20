using Navred.Core.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Itineraries
{
    public class Itinerary : IEnumerable<Leg>
    {
        private readonly IList<Leg> legs;

        public Itinerary()
        {
            this.legs = new List<Leg>();
            this.Carriers = new HashSet<string>();
        }

        public ICollection<string> Carriers { get; }

        public string From { get; private set; }

        public string To { get; private set; }

        public IEnumerable<Leg> Legs => this.legs.ToList();

        public decimal? Price { get; private set; }

        public TimeSpan Duration { get; private set; }

        public DateTime UtcDeparture { get; private set; }

        public DateTime UtcArrival { get; private set; }

        public bool IsZeroLeg => this.From == this.To && this.UtcArrival == this.UtcDeparture;

        public void AddLeg(Leg leg)
        {
            Validator.ThrowIfNull(leg);

            foreach (var l in this.legs)
            {
                if (l.UtcArrival >= leg.UtcArrival)
                {
                    throw new InvalidOperationException("Invalid arrival time.");
                }
            }

            this.legs.Add(leg);

            this.From = legs.First().From;
            this.To = legs.Last().From;
            this.UtcDeparture = legs.First().UtcArrival;
            this.UtcArrival = legs.Last().UtcArrival;
            this.Duration = this.GetDuration();
            this.Price += leg.Price;

            this.Carriers.Add(leg.Carrier);
        }

        public void AddLegs(IEnumerable<Leg> legs)
        {
            foreach (var leg in legs)
            {
                this.AddLeg(leg);
            }
        }

        public override string ToString()
        {
            return $"{this.From} - {this.To} ({this.UtcDeparture} - {this.UtcArrival}) {this.Price}";
        }

        public IEnumerator<Leg> GetEnumerator()
        {
            return this.legs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.legs.GetEnumerator();
        }

        private TimeSpan GetDuration()
        {
            var span = new TimeSpan();
            var last = default(Leg);

            foreach (var leg in this.legs)
            {
                if (last == null)
                {
                    last = leg;

                    continue;
                }

                var diff = leg.UtcArrival - last.UtcArrival;
                span += diff;
                last = leg;
            }

            return span;
        }
    }
}
