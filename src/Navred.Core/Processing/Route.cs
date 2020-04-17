using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Models;
using Navred.Core.Tools;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Processing
{
    public class Route
    {
        public Route(
            string country,
            DaysOfWeek dow,
            string carrier,
            Mode mode,
            IEnumerable<Stop> stops,
            string info,
            IEnumerable<Stop> banned = null)
        {
            this.Country = Validator.ReturnOrThrowIfNullOrWhiteSpace(country, "Country is empty.");
            this.Mode = mode;
            this.DaysOfWeek = dow;
            this.Carrier = carrier;
            this.Stops = Validator.ReturnOrThrowIfNullOrEmpty(stops, "Stops empty.").ToList();
            this.Banned = new HashSet<Stop>(
                banned ?? new List<Stop>(), new StopNameEqualityComparer());
            this.Info = info;
        }

        public string Country { get; set; }

        public Mode Mode { get; set; }

        public DaysOfWeek DaysOfWeek { get; set; }

        public string Carrier { get; set; }

        public string Info { get; set; }

        public IList<Stop> Stops { get; set; }

        public ICollection<Stop> Banned { get; set; }

        public bool IsValid => this.Stops.IsAscending(s => s.Time.Time);

        public ICollection<Stop> Estimables => this.Stops
            .Where(s => s.Time.Equals(LegTime.Estimable))
            .Select(s => s.Copy())
            .ToList();

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

            for (int s = 0; s < this.Stops.Count; s++)
            {
                if (estimables.Contains(this.Stops[s], new StopNameEqualityComparer()))
                {
                    var copy = this.Copy();

                    copy.Stops.RemoveAt(s);

                    if (copy.IsValid)
                    {
                        this.Stops.RemoveAt(s);

                        break;
                    }
                }
            }

            return this;
        }

        public Route SetStops(IEnumerable<Stop> stops)
        {
            this.Stops = stops.ToList();

            return this;
        }

        public Route Copy()
        {
            return new Route(
                this.Country,
                this.DaysOfWeek,
                this.Carrier,
                this.Mode,
                this.Stops.Select(s => s.Copy()),
                this.Info,
                this.Banned);
        }
    }
}
