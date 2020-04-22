using Navred.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search.Algorithms
{
    public class Dijkstra
    {
        private class Result
        {
            public GraphSearchPath BestPath { get; set; }

            public IDictionary<Vertex, Vertex> Previous { get; set; }

            public IDictionary<Vertex, Weight> Distances { get; set; }
        }

        public GraphSearchPath FindShortestPath(Graph g)
        {
            var result = this.DoPass(g);

            return result.BestPath;
        }

        public GraphSearchResult FindKShortestPaths(Graph g, int k)
        {
            var graphResult = new GraphSearchResult();
            var forwardPass = this.DoPass(g);

            graphResult.Add(forwardPass.BestPath);

            var r = g.Reverse();
            var backwardPass = this.DoPass(r);
            var seenVertices = new HashSet<Vertex>();
            var diffs = new Dictionary<Edge, Weight>();

            foreach (var v in forwardPass.BestPath.Vertices)
            {
                var nonBestEdges = v.Edges
                    .Where(e =>
                        !forwardPass.BestPath.Contains(e) &&
                        !seenVertices.Contains(e.Destination))
                    .ToList();

                foreach (var nonBestEdge in nonBestEdges)
                {
                    var diff =
                        backwardPass.Distances[nonBestEdge.Destination] -
                        backwardPass.Distances[nonBestEdge.Source] +
                        nonBestEdge.Weight;

                    diffs.Add(nonBestEdge, diff);
                }

                seenVertices.Add(v);
            }

            while (k >= 0 && !diffs.IsEmpty())
            {
                var best = diffs.GetMin(kvp => kvp.Value);
                var path = new GraphSearchPath();
                var fromSource = this.RetrievePath(
                    g.Source, best.Key.Source, forwardPass, g, best.Key, false);
                var fromReversedSource = this.RetrievePath(
                    r.Source, best.Key.Destination, backwardPass, r, best.Key.Reverse(), true);

                path.AddMany(fromSource);

                path.Add(best.Key);

                path.AddMany(fromReversedSource.Select(e => e.Reverse()));

                diffs.Remove(best.Key);

                if (!(path.Source.Equals(g.Source) && path.Destination.Equals(g.Destination)))
                {
                    continue;
                }

                graphResult.Add(path);

                k--;
            }

            var finalResult = graphResult.Finalize();

            return finalResult;
        }

        private Result DoPass(Graph g)
        {
            g.ThrowIfNull("Graph is empty.");

            var unvisited = new HashSet<Vertex>(g.Vertices);
            var distances = g.Vertices.ToDictionary(kvp => kvp, kvp => Weight.Max);
            var previous = g.Vertices.ToDictionary(kvp => kvp, kvp => default(Vertex));
            var paths = g.Vertices.ToDictionary(kvp => kvp, kvp => new List<Edge>());
            distances[g.Source] = Weight.Zero();

            while (!unvisited.IsEmpty())
            {
                var current = distances.Where(
                    d => unvisited.Contains(d.Key)).GetMin(d => d.Value).Key;

                if (distances[current].Equals(Weight.Max))
                {
                    unvisited.Remove(current);

                    continue;
                }

                foreach (var e in current.Edges)
                {
                    var currentDistance = distances[current] + e.Weight;

                    if (currentDistance < distances[e.Destination])
                    {
                        distances[e.Destination] = currentDistance;
                        previous[e.Destination] = current;
                        paths[e.Destination] = new List<Edge>(paths[current]) { e };
                    }
                }

                unvisited.Remove(current);
            }

            var result = new Result
            {
                Distances = distances,
                Previous = previous,
                BestPath = new GraphSearchPath(paths[g.Destination])
            };

            return result;
        }

        private IEnumerable<Edge> RetrievePath(
            Vertex from, Vertex to, Result result, Graph g, Edge e, bool fromReversed)
        {
            var edges = new Stack<Edge>();
            var current = to;
            var lastNeighbor = e;

            while (!current.Equals(from))
            {
                var prev = result.Previous[current];

                if (prev.IsNull())
                {
                    return new List<Edge>();
                }

                var recoverableEdges = g.Edges.Where(ed =>
                    ed.Source.Equals(from) &&
                    ed.Destination.Equals(to) &&
                    (fromReversed ? 
                        ed.Leg.UtcDeparture >= lastNeighbor.Leg.UtcArrival: 
                        ed.Leg.UtcArrival <= lastNeighbor.Leg.UtcDeparture))
                    .ToList();

                if (recoverableEdges.IsEmpty())
                {
                    return new List<Edge>();
                }

                var edge = recoverableEdges.GetMin(ed => ed.Leg.UtcDeparture);
                lastNeighbor = edge;

                edges.Push(edge);

                current = prev;
            }

            return edges;
        }
    }
}
