using Navred.Core.Itineraries;
using Navred.Core.Search;
using System.Collections.Generic;

namespace Navred.Api.Models
{
    public class ItineraryViewModel
    {
        public IEnumerable<Leg> Legs { get; set; }

        public Weight Weight { get; set; }
    }
}
