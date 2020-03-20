using Navred.Core.Tools;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Itineraries
{
    public class Schedule
    {
        private ICollection<Itinerary> itineraries;

        public Schedule()
        {
            this.itineraries = new HashSet<Itinerary>(new ItineraryEqualityComparer());
        }

        public void AddItinerary(Itinerary itinerary)
        {
            Validator.ThrowIfNull(itinerary);

            this.itineraries.Add(itinerary);
        }

        public void AddItineraries(IEnumerable<Itinerary> itineraries)
        {
            foreach (var i in itineraries)
            {
                this.AddItinerary(i);
            }
        }

        public IEnumerable<Itinerary> GetWithChildren()
        {
            var all = new HashSet<Itinerary>(new ItineraryEqualityComparer());

            foreach (var i in this.itineraries)
            {
                foreach (var child in this.GetWithChildren(i))
                {
                    if (!child.IsZeroLeg)
                    {
                        all.Add(child);
                    }
                }
            }

            return all;
        }

        private IEnumerable<Itinerary> GetWithChildren(Itinerary i)
        {
            var all = new List<Itinerary>();

            for (int s = 0; s < i.Legs.Count(); s++)
            {
                var legs = i.Legs.Skip(s).ToList();

                for (int x = legs.Count(); x > 0; x--)
                {
                    var itinerary = new Itinerary();

                    itinerary.AddLegs(legs.Take(x));

                    all.Add(itinerary);
                }
            }

            return all;
        }
    }
}
