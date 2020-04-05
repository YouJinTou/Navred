using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Estimation;
using Navred.Core.Extensions;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using Navred.Crawling.Crawlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Navred.Updates
{
    public class Program
    {
        private const string All = "All";

        public static void Main()
        {
            Console.WriteLine("Choose crawlers to run: ");

            var crawlersByKey = CreateCrawlersByKey();
            var optionsByIndex = new Dictionary<int, string>
            {
                { 1, $"1. {All}" }
            };
            var index = 2;

            foreach (var kvp in crawlersByKey)
            {
                optionsByIndex.Add(index, $"{index}. {kvp.Key}");

                index++;
            }

            foreach (var kvp in optionsByIndex)
            {
                Console.WriteLine(kvp.Value);
            }

            var parsed = int.TryParse(Console.ReadLine(), out int option);
            var validOption = parsed && option >= 1 && option <= crawlersByKey.Count + 1;

            if (!validOption)
            {
                throw new InvalidOperationException("Invalid option.");
            }

            var optionString = optionsByIndex[option];
            var crawlerString = optionString.Split('.')[1].Trim();

            if (crawlerString.Equals(All))
            {
                foreach (var kvp in crawlersByKey)
                {
                    Console.WriteLine($"Running {kvp.Key}");

                    kvp.Value.UpdateLegsAsync().Wait();
                }
            }
            else
            {
                var crawler = crawlersByKey[crawlerString];

                crawler.UpdateLegsAsync().Wait();
            }
        }

        private static IDictionary<string, ICrawler> CreateCrawlersByKey()
        {
            var provider = new ServiceCollection().AddCore().BuildServiceProvider();
            var placesManager = provider.GetService<IPlacesManager>();
            var estimator = provider.GetService<ITimeEstimator>();
            var repo = provider.GetService<ILegRepository>();
            var httpClientFactory = provider.GetService<IHttpClientFactory>();
            var cultureProvider = provider.GetService<ICultureProvider>();
            var crawlersByKey = new Dictionary<string, ICrawler>
            {
                { "Бойдеви", new Boydevi(repo, placesManager) },
                { "София", new SofiaCentralBusStation(repo, httpClientFactory, placesManager, estimator, cultureProvider) },
                { "Велико Търново Юг", new VelikoTarnovoSouthBusStation(placesManager, estimator, repo, cultureProvider) },
                { "Пловдив ХебросБус", new PlovdivHebrosBus(repo, placesManager, estimator, cultureProvider, provider.GetService<ILogger<PlovdivHebrosBus>>()) }
            };

            return crawlersByKey;
        }
    }
}
