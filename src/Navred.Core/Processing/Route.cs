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
            string info)
        {
            this.Country = Validator.ReturnOrThrowIfNullOrWhiteSpace(country, "Country is empty.");
            this.Mode = mode;
            this.DaysOfWeek = dow;
            this.Carrier = carrier;
            this.Stops = Validator.ReturnOrThrowIfNullOrEmpty(stops, "Stops empty.").ToList();
            this.Info = info;
        }

        public string Country { get; set; }

        public Mode Mode { get; set; }

        public DaysOfWeek DaysOfWeek { get; set; }

        public string Carrier { get; set; }

        public string Info { get; set; }

        public IList<Stop> Stops { get; set; }

        public Route Copy(IEnumerable<Stop> stops = null)
        {
            return new Route(
                this.Country,
                this.DaysOfWeek,
                this.Carrier,
                this.Mode,
                stops ?? this.Stops,
                this.Info);
        }
    }
}
