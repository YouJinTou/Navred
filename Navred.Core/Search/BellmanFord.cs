using Navred.Core.Itineraries;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    internal class BellmanFord : IPathFinder
    {
        public IEnumerable<Itinerary> FindItineraries(Graph graph)
        {
            var weights = graph.Vertices.ToDictionary(kvp => kvp.Name, kvp => Weight.CreateMax());
            weights[graph.Source.Name] = new Weight();

            return null;
        }
    }
}
