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
                this.Paths = new List<GraphSearchPath>();
            }

            this.Paths.Add(path.Copy());
        }

        public GraphSearchResult Sort()
        {
            if (this.Paths.IsNullOrEmpty())
            {
                return this;
            }

            this.Paths = this.Paths
                .OrderBy(p => p.Weight)
                .ThenBy(p => p.Path.Count)
                .ToList();

            return this;
        }

        public GraphSearchResult Filter()
        {
            this.Paths = new HashSet<GraphSearchPath>(
                this.Paths, new GraphSearchPathEqualityComparer());

            return this;
        }

        public GraphSearchResult Merge()
        {
            var merged = this.Paths.Select(p => p.Merge()).ToList();
            this.Paths = merged;

            return this;
        }
    }
}
