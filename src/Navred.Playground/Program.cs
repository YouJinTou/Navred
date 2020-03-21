using Microsoft.Extensions.DependencyInjection;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using System;

namespace Navred.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = new ServiceCollection().AddCore().BuildServiceProvider();
            var finder = provider.GetService<IItineraryFinder>();
            var from = new DateTime(2020, 3, 25, 1, 0, 0);
            var to = new DateTime(2020, 3, 25, 23, 59, 0);
            var window = new TimeWindow(from, to);
            var result = finder.FindItinerariesAsync("Пловдив", "Любимец", window).Result;
        }
    }
}
