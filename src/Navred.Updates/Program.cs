using Microsoft.Extensions.DependencyInjection;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Estimation;
using Navred.Core.Extensions;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using Navred.Crawling.Crawlers;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Navred.Updater
{
    class Program
    {
        public static void Main(string[] args)
        {
            var crawlersByKey = CreateCrawlersByKey();

            Console.WriteLine("Choose what to update: ");


        }

        private static IDictionary<string, ICrawler> CreateCrawlersByKey()
        {
            var provider = new ServiceCollection().AddCore().BuildServiceProvider();
            var placesManager = provider.GetService<IPlacesManager>();
            var estimator = provider.GetService<ITimeEstimator>();
            var repo = provider.GetService<ILegRepository>();
            var httpClientFactory = provider.GetService<IHttpClientFactory>();
            var cultureProvider = provider.GetService<ICultureProvider>();
            var crawlerByKey = new Dictionary<string, ICrawler>
            {
                { "Велико Търново Юг", new VelikoTarnovoSouthBusStation(placesManager, estimator, repo, cultureProvider) },
                { "София", new SofiaCentralBusStation(repo, httpClientFactory, placesManager, estimator, cultureProvider) },
                { "Бойдеви", new Boydevi(repo, placesManager) },
            };

            return crawlerByKey;
        }
    }
}
