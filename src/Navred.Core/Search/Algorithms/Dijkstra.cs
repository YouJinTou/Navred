﻿using Navred.Core.Extensions;
using Navred.Core.Tools;
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
            var diffs = new Dictionary<Edge, Weight>();

            foreach (var v in forwardPass.BestPath.Vertices)
            {
                foreach (var nonBestEdge in v.Edges.Where(e => !forwardPass.BestPath.Contains(e)))
                {
                    var diff =
                        backwardPass.Distances[nonBestEdge.Destination] -
                        backwardPass.Distances[nonBestEdge.Source] +
                        nonBestEdge.Weight;

                    diffs.Add(nonBestEdge, diff);
                }
            }

            if (diffs.IsEmpty())
            {
                return graphResult;
            }

            for (int i = 0; i < k - 1; i++)
            {
                var nextShortestEdge = diffs.GetMin(kvp => kvp.Value);
                var path = new GraphSearchPath();
                var pathFromSource = this.RetrievePath(
                    g.Source, nextShortestEdge.Key.Source, forwardPass, g);
                var pathFromShortestDestination = this.RetrievePath(
                    reversed.Source, nextShortestEdge.Key.Destination, backwardPass, reversed);
                pathFromShortestDestination = pathFromShortestDestination
                    .Select(e => e.Reverse()).ToList();

                path.AddMany(pathFromSource);

                path.Add(nextShortestEdge.Key);

                path.AddMany(pathFromShortestDestination);

                graphResult.Add(path);

                diffs.Remove(nextShortestEdge.Key);
            }

            return graphResult;
        }

        private Result DoPass(Graph g)
        {
            Validator.ThrowIfNull(g, "Graph is empty.");

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

        private IEnumerable<Edge> RetrievePath(Vertex from, Vertex to, Result result, Graph g)
        {
            var edges = new List<Edge>();
            var current = to;

            while (!current.Equals(from))
            {
                var prev = result.Previous[current];
                var edgeToFind = new Edge
                {
                    Source = prev,
                    Destination = current,
                    Weight = result.Distances[current] - result.Distances[prev]
                };
                var edge = g.Edges.Single(e => e.Equals(edgeToFind));

                edges.Add(edge);

                current = prev;
            }

            return edges;
        }
    }
}