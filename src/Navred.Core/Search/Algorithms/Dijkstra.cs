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

            var reversed = g.Reverse();
            var backwardPass = this.DoPass(reversed);
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
                var shortest = diffs.GetMin(kvp => kvp.Value);
                var path = new GraphSearchPath();
                var fromSource = this.RetrievePath(
                    g.Source, shortest.Key.Source, forwardPass, g, shortest.Key);
                var fromShortestDestinationToDestination = this.RetrievePath(
                    shortest.Key.Destination, g.Destination, forwardPass, g, shortest.Key);

                path.AddMany(fromSource);

                path.Add(shortest.Key);

                path.AddMany(fromShortestDestinationToDestination);

                diffs.Remove(shortest.Key);

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
            Vertex from, Vertex to, Result result, Graph g, Edge e)
        {
            var edges = new Stack<Edge>();
            var current = to;

            while (!current.Equals(from))
            {
                var prev = result.Previous[current];

                if (prev == null)
                {
                    return new List<Edge>();
                }

                var weight = result.Distances[current] - result.Distances[prev];
                var edge = g.FindEdge(prev, current, weight, e);

                if (edge == null)
                {
                    return new List<Edge>();
                }

                edges.Push(edge);

                current = prev;
            }

            return edges;
        }
    }
}
