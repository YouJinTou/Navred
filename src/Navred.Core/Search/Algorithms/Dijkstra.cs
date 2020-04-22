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

            foreach (var v in forwardPass.BestPath.Vertices)
            {
                var nonBestEdges = v.Edges
                    .Where(e =>
                        !forwardPass.BestPath.Contains(e) &&
                        !seenVertices.Contains(e.Destination))
                    .ToList();

                foreach (var edge in nonBestEdges)
                {
                    var pathsFromSource = this.RetrievePaths(
                        g.Source, edge.Source, forwardPass, g, edge, false);
                    var pathsFromReversedSource = this.RetrievePaths(
                        r.Source, edge.Destination, backwardPass, r, edge.Reverse(), true);
                    pathsFromSource = pathsFromSource.IsEmpty() ?
                        new List<List<Edge>> { new List<Edge> { null } } :
                        pathsFromSource;
                    pathsFromReversedSource = pathsFromReversedSource.IsEmpty() ?
                        new List<List<Edge>> { new List<Edge> { null } } :
                        pathsFromReversedSource;

                    foreach (var pathFromSource in pathsFromSource)
                    {
                        var pathStart = new GraphSearchPath(pathFromSource);

                        pathStart.Add(edge);

                        foreach (var pathFromReversedSource in pathsFromReversedSource)
                        {
                            var path = new GraphSearchPath(pathStart.Path);

                            path.AddMany(pathFromReversedSource.Select(e => e?.Reverse()));

                            if (path.Source.Equals(g.Source) && path.Destination.Equals(g.Destination))
                            {
                                graphResult.Add(path);
                            }
                        }
                    }
                }

                seenVertices.Add(v);
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

        private IEnumerable<IEnumerable<Edge>> RetrievePaths(
           Vertex from, Vertex to, Result result, Graph g, Edge e, bool fromReversed)
        {
            var traversed = new HashSet<Edge>();
            var paths = new List<IEnumerable<Edge>>();

            while (true)
            {
                var edges = new Stack<Edge>();
                var current = to;
                var lastNeighbor = e;
                var shouldAddPath = false;

                while (!current.Equals(from))
                {
                    var prev = result.Previous[current];

                    if (prev.IsNull())
                    {
                        shouldAddPath = false;

                        break;
                    }

                    var recoverableEdges = g.Edges.Where(ed =>
                        ed.Source.Equals(prev) &&
                        ed.Destination.Equals(current) &&
                        !traversed.Contains(ed) &&
                        (fromReversed ?
                            ed.Leg.UtcDeparture >= lastNeighbor.Leg.UtcArrival :
                            ed.Leg.UtcArrival <= lastNeighbor.Leg.UtcDeparture))
                        .ToList();

                    if (recoverableEdges.IsEmpty())
                    {
                        shouldAddPath = false;

                        break;
                    }

                    shouldAddPath = true;
                    var edge = recoverableEdges.GetMin(ed => ed.Leg.UtcDeparture);
                    lastNeighbor = edge;
                    current = prev;

                    traversed.Add(edge);
                    edges.Push(edge);
                }

                if (shouldAddPath)
                {
                    paths.Add(edges);
                }
                else
                {
                    break;
                }
            }

            return paths;
        }
    }
}
