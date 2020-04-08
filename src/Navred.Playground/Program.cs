using Microsoft.Extensions.DependencyInjection;
using Navred.Core;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Models;
using Navred.Core.Places;
using Navred.Core.Search;
using Navred.Core.Search.Algorithms;
using System;
using System.Collections.Generic;

namespace Navred.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = new Vertex { Name = "A" };
            var b = new Vertex { Name = "B" };
            var c = new Vertex { Name = "C" };
            var d = new Vertex { Name = "D" };
            var e = new Vertex { Name = "E" };
            var f = new Vertex { Name = "F" };
            var g = new Vertex { Name = "G" };
            var h = new Vertex { Name = "H" };
            var i = new Vertex { Name = "I" };
            var vertices = new List<Vertex> { a, /*b,*/ c/*, d, e, f, g, h, i*/ };
            var ih = new Edge
            {
                Source = i,
                Destination = h,
                Weight = new Weight { Duration = TimeSpan.FromHours(15)}
            };
            var ig = new Edge
            {
                Source = i,
                Destination = g,
                Weight = new Weight { Duration = TimeSpan.FromHours(12) }
            };
            var @if = new Edge
            {
                Source = i,
                Destination = f,
                Weight = new Weight { Duration = TimeSpan.FromHours(17) }
            };
            var gf = new Edge
            {
                Source = g,
                Destination = f,
                Weight = new Weight { Duration = TimeSpan.FromHours(2) }
            };
            var gd = new Edge
            {
                Source = g,
                Destination = d,
                Weight = new Weight { Duration = TimeSpan.FromHours(3) }
            };
            var he = new Edge
            {
                Source = h,
                Destination = e,
                Weight = new Weight { Duration = TimeSpan.FromHours(10) }
            };
            var df = new Edge
            {
                Source = d,
                Destination = f,
                Weight = new Weight { Duration = TimeSpan.FromHours(17) }
            };
            var de = new Edge
            {
                Source = d,
                Destination = e,
                Weight = new Weight { Duration = TimeSpan.FromHours(50) }
            };
            var da = new Edge
            {
                Source = d,
                Destination = a,
                Weight = new Weight { Duration = TimeSpan.FromHours(22) }
            };
            var fb = new Edge
            {
                Source = f,
                Destination = b,
                Weight = new Weight { Duration = TimeSpan.FromHours(25) }
            };
            var ec = new Edge
            {
                Source = e,
                Destination = c,
                Weight = new Weight { Duration = TimeSpan.FromHours(7) }
            };
            var ba = new Edge
            {
                Source = b,
                Destination = a,
                Weight = new Weight { Duration = TimeSpan.FromHours(10) }
            };
            var ca = new Edge
            {
                Source = c,
                Destination = a,
                Weight = new Weight { Duration = TimeSpan.FromHours(24) }
            };
            var edges = new List<Edge> 
            { 
                //ih, ig, @if, 
                //gf, gd,
                //he,
                //df, de, da,
                //fb,
                //ec,
                //ba,
                ca
            };
            var graph = new Graph(c, a, vertices, edges);
            var dijkstra = new Dijkstra();
            var paths = dijkstra.FindKShortestPaths(graph, 3);

            var provider = new ServiceCollection().AddCore().BuildServiceProvider();
            var finder = provider.GetService<IItineraryFinder>();
            var placesManager = provider.GetService<IPlacesManager>();
            var startTime = new DateTimeTz(DateTime.Now, Constants.BulgariaTimeZone);
            var endTime = new DateTimeTz(DateTime.Now.AddDays(1), Constants.BulgariaTimeZone);
            var window = new TimeWindow(startTime, endTime);
            var from = placesManager.GetPlace(
                BulgarianCultureProvider.CountryName, BulgarianCultureProvider.City.VelikoTarnovo);
            var to = placesManager.GetPlace(BulgarianCultureProvider.CountryName, "Омуртаг");
            var result = finder.FindItinerariesAsync(from, to, window).Result;
        }
    }
}
