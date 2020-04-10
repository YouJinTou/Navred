using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    public class GraphSearchResult : ICopyable<GraphSearchResult>
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

        public GraphSearchResult Finalize()
        {
            return this.Merge().Filter().Sort();
        }

        public GraphSearchResult Sort()
        {
            var copy = this.Copy();

            if (this.Paths.IsNullOrEmpty())
            {
                return copy;
            }

            copy.Paths = copy.Paths
                .OrderBy(p => p.Weight)
                .ThenBy(p => p.Path.Count)
                .ToList();

            return copy;
        }

        public GraphSearchResult Filter()
        {
            var copy = this.Copy();
            copy.Paths = new HashSet<GraphSearchPath>(
                copy.Paths, new GraphSearchPathEqualityComparer());

            return copy;
        }

        public GraphSearchResult Merge()
        {
            var copy = this.Copy();
            var merged = copy.Paths.Select(p => p.Merge()).ToList();
            copy.Paths = merged;

            return copy;
        }

        public GraphSearchResult Copy()
        {
            var copy = new GraphSearchResult();

            foreach (var path in this.Paths)
            {
                copy.Add(path.Copy());
            }

            return copy;
        }
    }
}
