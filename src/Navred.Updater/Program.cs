using Microsoft.Extensions.DependencyInjection;
using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using Navred.Crawling.Crawlers;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var templateType = typeof(Template);
            var crawlerTypes = templateType.Assembly.GetTypes()
                .Where(t => typeof(ICrawler).IsAssignableFrom(t) && t.Name != templateType.Name)
                .ToList();
            var crawlersByKey = new Dictionary<string, ICrawler>();

            foreach (var type in crawlerTypes)
            {
                var dependencies = type.GetConstructors().First().GetParameters()
                    .Select(p => provider.GetService(p.ParameterType))
                    .ToArray();
                var instance = (ICrawler)Activator.CreateInstance(type, dependencies);

                crawlersByKey.Add(type.Name, instance);
            }

            return crawlersByKey;
        }
    }
}
