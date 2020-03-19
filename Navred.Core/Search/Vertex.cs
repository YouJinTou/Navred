using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    public class Vertex
    {
        public object Data { get; set; }

        public string Name { get; set; }

        public IEnumerable<Edge> Edges { get; set; }

        public Edge GetCheapeastEdge()
        {
            var cheapestWeight = Weight.CreateMax();
            var cheapestEdge = this.Edges.First();

            foreach (var edge in this.Edges)
            {
                if (edge.Weight.CompareTo)
                {
                    cheapestWeight = edge.Weight;
                    cheapestEdge = edge;
                }
            }

            return cheapestEdge;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
