﻿using Navred.Core.Extensions;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using Navred.Core.Search;
using Navred.Core.Search.Algorithms;
using Navred.Core.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries
{
    public class ItineraryFinder : IItineraryFinder
    {
        private readonly ILegRepository repo;

        public ItineraryFinder(ILegRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IEnumerable<GraphSearchPath>> FindItinerariesAsync(
            Place from, Place to, TimeWindow window)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to, window);

            var legs = (await this.repo.GetLegsAsync(from, to, window)).ToList();

            if (legs.IsEmpty() || !legs.Any(l => l.To.Equals(to)))
            {
                return new List<GraphSearchPath>();
            }

            var vertices = legs
                .Select(l => new List<string> { l.From.GetId(), l.To.GetId() })
                .SelectMany(s => s)
                .Distinct()
                .Select(s => new Vertex { Name = s })
                .ToList();
            var edges = legs.Select(l => new Edge
            {
                Source = vertices.Single(v => v.Name == l.From.GetId()),
                Destination = vertices.Single(v => v.Name == l.To.GetId()),
                Weight = new Weight
                {
                    Duration = l.Duration + l.UtcDeparture.TimeOfDay,
                    Price = l.Price,
                },
                Leg = l
            }).ToList();
            var graph = new Graph(
                 vertices.Single(v => v.Name == from.GetId()),
                 vertices.Single(v => v.Name == to.GetId()),
                 vertices,
                 edges);
            var result = new Dijkstra().FindKShortestPaths(graph, 4);

            return result.Paths;
        }
    }
}
