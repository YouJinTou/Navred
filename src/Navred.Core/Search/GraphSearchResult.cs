using Navred.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    public class GraphSearchResult
    {
        public ICollection<GraphSearchPath> Paths { get; private set; }

        public void Add(GraphSearchPath path)
        {
            if (this.Paths.IsNullOrEmpty())
            {
                this.Paths = new HashSet<GraphSearchPath>(new GraphSearchPathEqualityComparer());
            }

            this.Paths.Add(path.Copy());
        }

        public void Sort()
        {
            if (this.Paths.IsNullOrEmpty())
            {
                return;
            }

            this.Paths = this.Paths.OrderBy(p => p.Weight).ToList();
        }
    }
}
