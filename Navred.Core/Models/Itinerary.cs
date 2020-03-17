using Navred.Core.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Models
{
    public class Itinerary : IEnumerable<Stop>
    {
        private readonly IEnumerable<Stop> stops;

        public Itinerary(
            IEnumerable<Stop> stops, 
            string carrier,
            decimal? price = null, 
            DaysOfWeek onDays = Constants.AllWeek)
        {
            if (stops == null || stops.Count() <= 1)
            {
                throw new ArgumentException("Itinerary is empty.");
            }

            this.stops = stops;
            this.Carrier = carrier;
            this.From = stops.First().Name;
            this.To = stops.Last().Name;
            this.Price = price;
            this.OnDays = onDays;
            this.Stops = stops.ToList();
            this.ChildrenAndSelf = this.GetChildrenAndSelf();
            this.Duration = this.GetDuration();
        }

        public IEnumerable<Itinerary> ChildrenAndSelf { get; }

        public string Carrier { get; }

        public string From { get; }

        public string To { get; }

        public IEnumerable<Stop> Stops { get; }

        public decimal? Price { get; }

        public DaysOfWeek OnDays { get; }

        public TimeSpan Duration { get; }

        public override string ToString()
        {
            return $"{this.From} - {this.To} {this.Price}";
        }

        public IEnumerator<Stop> GetEnumerator()
        {
            return this.stops.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.stops.GetEnumerator();
        }

        private IEnumerable<Itinerary> GetChildrenAndSelf()
        {
            var subItineraries = new List<Itinerary>();
            var stops = this.stops.ToList();

            subItineraries.Add(this);

            for (int s = 1; s < stops.Count - 1; s++)
            {
                var itinerary = new Itinerary(stops.Skip(s), this.Carrier, onDays: this.OnDays);

                subItineraries.Add(itinerary);
            }

            return subItineraries;
        }

        private TimeSpan GetDuration()
        {
            var span = new TimeSpan();
            var last = default(Stop);

            foreach (var stop in this.stops)
            {
                if (last == null)
                {
                    last = stop;

                    continue;
                }

                var diff = stop.ArrivalTimeToTimeSpan() - last.ArrivalTimeToTimeSpan();
                span += diff;
                last = stop;
            }

            return span;
        }
    }
}
