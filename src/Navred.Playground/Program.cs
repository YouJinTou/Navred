using Microsoft.Extensions.DependencyInjection;
using Navred.Core;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Models;
using Navred.Core.Places;
using System;

namespace Navred.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
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
