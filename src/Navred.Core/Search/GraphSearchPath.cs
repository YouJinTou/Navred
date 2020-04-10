using Navred.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    public class GraphSearchPath
    {
        public GraphSearchPath()
        {
            this.Path = new List<Edge>();
        }

        public GraphSearchPath(IEnumerable<Edge> edges)
        {
            this.Path = new List<Edge>();

            this.AddMany(edges);
        }

        public ICollection<Edge> Path { get; private set; }

        public Weight Weight { get; private set; }

        public Vertex Source => this.Path.First().Source;

        public Vertex Destination => this.Path.Last().Destination;

        public Edge Tail => this.Path.Last();

        public IEnumerable<Vertex> Vertices => 
            new List<Vertex>(this.Path.Select(p => p.Source)) { this.Tail.Destination };

        public bool Touches(Edge edge)
            => this.Path.Any(e => e.Source.Equals(edge.Destination));

        public bool Contains(Edge edge)
            => this.Path.Any(
                e => e.Source.Equals(edge.Source) && e.Destination.Equals(edge.Destination));

        public void Add(Edge edge)
        {
            this.Weight = this.Weight ?? new Weight();
            this.Weight += edge.Weight;

            this.AddWaitTime(edge);

            this.Path.Add(edge);
        }

        public void AddMany(IEnumerable<Edge> edges)
        {
            foreach (var edge in edges)
            {
                this.Add(edge);
            }
        }

        public void Remove(Edge edge)
        {
            this.Weight -= edge.Weight;

            this.Path.Remove(edge);

            this.RemoveWeightTime(edge);
        }

        public GraphSearchPath Copy()
        {
            var cost = this.Weight == null ? null : new Weight
            {
                Duration = this.Weight.Duration,
                Price = this.Weight.Price
            };
            var path = new GraphSearchPath
            {
                Weight = cost,
                Path = this.Path.Select(e => e.Copy()).ToList()
            };

            return path;
        }

        public GraphSearchPath Merge()
        {
            var path = this.MergeRecursive(new GraphSearchPath(), true);

            return path;
        }

        private void AddWaitTime(Edge edge)
        {
            if (this.Path.IsEmpty())
            {
                return;
            }

            var utcDeparture = edge?.Leg?.UtcDeparture ?? new DateTime();
            var utcArrival = this.Tail?.Leg?.UtcArrival ?? new DateTime();

            this.Weight += new Weight
            {
                Duration = utcDeparture - utcArrival,
                Price = null
            };
        }

        private void RemoveWeightTime(Edge edge)
        {
            if (this.Path.IsEmpty())
            {
                return;
            }

            this.Weight -= new Weight
            {
                Duration = edge.Leg.UtcDeparture - this.Path.Last().Leg.UtcArrival,
                Price = null
            };
        }

        private GraphSearchPath MergeRecursive(
            GraphSearchPath currentMerged, bool somethingChanged)
        {
            if (!somethingChanged)
            {
                return currentMerged;
            }

            var index = 0;
            var iterable = currentMerged.Path.IsEmpty() ? this : currentMerged;
            var mergedPath = new GraphSearchPath();
            var mergedOccurred = false;

            while (index < iterable.Path.Count)
            {
                var edges = iterable.Path.Skip(index).ToArray();
                var isLast = index == iterable.Path.Count - 1;
                Edge merged;

                if (isLast)
                {
                    if (mergedPath.Path.IsEmpty())
                    {
                        mergedPath.Add(edges[0]);

                        break;
                    }

                    if (mergedPath.Tail.TryMergeWith(edges[0], out merged))
                    {
                        mergedOccurred = true;

                        mergedPath.Remove(mergedPath.Tail);

                        mergedPath.Add(merged);
                    }
                    else
                    {
                        mergedPath.Add(edges[0]);
                    }

                    break;
                }

                if (edges[0].TryMergeWith(edges[1], out merged))
                {
                    mergedOccurred = true;

                    mergedPath.Add(merged);

                    index += 2;
                }
                else
                {
                    mergedPath.Add(edges[0]);

                    index++;
                }
            }

            return this.MergeRecursive(mergedPath, mergedOccurred);
        }

        public override string ToString()
        {
            var destination = this.Destination.ToString();
            var legs = string.Join(" - ", this.Path.Select(p => p.Source));
            var result = $"{legs} - {destination} | {this.Weight}";

            return result;
        }
    }
}
