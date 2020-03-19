using Navred.Core.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Itineraries
{
    public class Itinerary : IEnumerable<Stop>
    {
        private readonly IList<Stop> stops;

        public Itinerary(string carrier, decimal? price = null)
        {
            this.stops = new List<Stop>();
            this.Carrier = carrier;
            this.Price = price;
        }

        public string Carrier { get; }

        public string From { get; private set; }

        public string To { get; private set; }

        public IEnumerable<Stop> Stops => this.stops.ToList();

        public decimal? Price { get; }

        public DaysOfWeek? OnDays { get; }

        public TimeSpan Duration { get; private set; }

        public DateTime Departure { get; private set; }

        public DateTime Arrival { get; private set; }

        public bool IsZeroStops => this.From == this.To && this.Arrival == this.Departure;

        public void AddStop(Stop stop)
        {
            Validator.ThrowIfNull(stop);

            foreach (var s in this.stops)
            {
                if (s.ArrivalTime >= stop.ArrivalTime)
                {
                    throw new InvalidOperationException("Invalid arrival time.");
                }
            }

            this.stops.Add(stop);

            this.From = stops.First().Name;
            this.To = stops.Last().Name;
            this.Departure = stops.First().ArrivalTime;
            this.Arrival = stops.Last().ArrivalTime;
            this.Duration = this.GetDuration();
        }

        public void AddStops(IEnumerable<Stop> stops)
        {
            foreach (var stop in stops)
            {
                this.AddStop(stop);
            }
        }

        public override string ToString()
        {
            return $"{this.From} - {this.To} ({this.Departure} - {this.Arrival}) {this.Price}";
        }

        public IEnumerator<Stop> GetEnumerator()
        {
            return this.stops.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.stops.GetEnumerator();
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

                var diff = stop.ArrivalTime - last.ArrivalTime;
                span += diff;
                last = stop;
            }

            return span;
        }
    }
}
