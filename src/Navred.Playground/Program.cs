using Microsoft.Extensions.DependencyInjection;
using Navred.Core;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
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
            var from = new DateTimeTz(DateTime.Now, Constants.BulgariaTimeZone);
            var to = new DateTimeTz(DateTime.Now.AddDays(1), Constants.BulgariaTimeZone);
            var window = new TimeWindow(from, to);
            var result = finder.FindItinerariesAsync("Велико Търново", "Омуртаг", window).Result;
        }
    }
}
