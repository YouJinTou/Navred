using Microsoft.Extensions.DependencyInjection;
using Navred.Core;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Models;
using System;

namespace Navred.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = new ServiceCollection().AddCore().BuildServiceProvider();
            var finder = provider.GetService<IItineraryFinder>();
            var from = new DateTimeTz(new DateTime(2020, 3, 25, 1, 0, 0), Constants.BulgariaTimeZone);
            var to = new DateTimeTz(new DateTime(2020, 3, 25, 23, 59, 0), Constants.BulgariaTimeZone);
            var window = new TimeWindow(from, to);
            var result = finder.FindItinerariesAsync("София", "Ловеч", window).Result;
        }
    }
}
