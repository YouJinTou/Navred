using System.Collections.Generic;

namespace Navred.Core.Search
{
    public class Vertex
    {
        public object Data { get; set; }

        public string Name { get; set; }

        public IEnumerable<Edge> Edges { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
