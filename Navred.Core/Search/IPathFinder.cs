using Navred.Core.Itineraries;
using System.Collections.Generic;

namespace Navred.Core.Search
{
    public interface IPathFinder
    {
        IEnumerable<Itinerary> FindItineraries(Graph graph);
    }
}
