using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Processing
{
    public class Route
    {
        private IDictionary<Stop, IDictionary<Stop, decimal?>> pricesPerStop;

        public Route(
            string country,
            DaysOfWeek dow,
            string carrier,
            Mode mode,
            IEnumerable<Stop> stops,
            string info,
            IEnumerable<Stop> banned = null)
        {
            this.Country = country.ReturnOrThrowIfNullOrWhiteSpace("Country is empty.");
            this.Mode = mode;
            this.DaysOfWeek = dow;
            this.Carrier = carrier;
            this.Stops = stops.ReturnOrThrowIfNullOrEmpty("Stops empty.").ToList();
            this.Banned = new HashSet<Stop>(
                banned ?? new List<Stop>(), new StopNameEqualityComparer());
            this.Info = info;
        }

        public string Country { get; private set; }

        public Mode Mode { get; private set; }

        public DaysOfWeek DaysOfWeek { get; private set; }

        public string Carrier { get; private set; }

        public string Info { get; private set; }

        public IList<Stop> Stops { get; private set; }

        public ICollection<Stop> Banned { get; private set; }

        public ICollection<Stop> NotFound { get; private set; }

        public bool IsValid
        {
            get
            {
                var currentDate = DateTime.UtcNow.Date;
                var times = this.Stops.Select(s => s.Time.Time);
                var lastTime = times.First();
                var dateTimes = new List<DateTime>();

                foreach (var time in times)
                {
                    if (lastTime > time)
                    {
                        currentDate = currentDate.AddDays(1);
                        lastTime = time;
                    }

                    dateTimes.Add(currentDate + time);
                }

                var isValid = dateTimes.IsAscending(d => d);

                return isValid;
            }
        }

        public ICollection<Stop> Estimables => this.Stops
            .Where(s => s.Time.Equals(LegTime.Estimable) || s.Time.Estimated)
            .Select(s => s.Copy())
            .ToList();

        public decimal? GetPrice(Stop from, Stop to, out bool priceEstimated)
        {
            priceEstimated = false;

            if (!to.Price.Value.HasValue)
            {
                return null;
            }

            var source = this.Stops.First();

            if (from.Equals(source))
            {
                return to.Price.Value;
            }

            if (!from.Price.Value.HasValue)
            {
                return null;
            }

            var price = (to.Price - from.Price);
            var isFree = price.Equals(0m);

            if (isFree)
            {
                return null;
            }

            priceEstimated = true;

            return price;
        }

        public Route TagEmptyStops()
        {
            this.Stops = this.Stops.Select(s =>
            {
                s.Name = string.IsNullOrWhiteSpace(s.Name) ? "EMPTY" : s.Name;

                return s;
            }).ToList();

            return this;
        }

        public Route RemoveBanned()
        {
            this.Stops = this.Stops.Except(this.Banned, new StopNameEqualityComparer()).ToList();

            return this;
        }

        public Route RemoveDuplicates()
        {
            var reversed = this.Stops.Reverse();
            reversed = new HashSet<Stop>(
                reversed, new StopTimeEqualityComparer()).ToList();
            var duplicatesWithEstimableTimes = reversed
                .GroupBy(s => s.CompositeName)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .Where(s => s.Time.Equals(LegTime.Estimable))
                .ToList();
            reversed = reversed.Except(
                duplicatesWithEstimableTimes, new StopTimeEqualityComparer()).ToList();
            reversed = new HashSet<Stop>(reversed, new StopNameEqualityComparer()).ToList();
            this.Stops = reversed.Reverse().ToList();

            return this;
        }

        public Route RemovePlaceless()
        {
            this.NotFound = this.Stops.Where(s => s.Place.IsNull()).ToList();
            this.Stops = this.Stops.Where(s => !s.Place.IsNull()).ToList();

            return this;
        }

        public Route RemoveOffendingEstimables()
        {
            if (this.IsValid)
            {
                return this;
            }

            var estimables = this.Estimables;
            var copy = this.Copy();
            var toRemove = new List<int>();

            for (int s = 0; s < this.Stops.Count; s++)
            {
                if (estimables.Contains(this.Stops[s], new StopNameEqualityComparer()))
                {
                    copy.Stops.RemoveAt(s);

                    toRemove.Add(s);

                    if (copy.IsValid)
                    {
                        break;
                    }
                }
            }

            if (copy.IsValid)
            {
                foreach (var idx in toRemove)
                {
                    this.Stops.RemoveAt(idx);
                }
            }

            return this;
        }

        public Route SetStops(IEnumerable<Stop> stops)
        {
            this.Stops = stops.ToList();

            return this;
        }

        public Route Copy(DaysOfWeek? dow = null, IEnumerable<Stop> stops = null)
        {
            return new Route(
                this.Country,
                dow ?? this.DaysOfWeek,
                this.Carrier,
                this.Mode,
                stops ?? this.Stops.Select(s => s.Copy()),
                this.Info,
                this.Banned);
        }

        public override string ToString()
        {
            return $"{string.Join(" | ", this.Stops)} ({this.Carrier})";
        }
    }
}
