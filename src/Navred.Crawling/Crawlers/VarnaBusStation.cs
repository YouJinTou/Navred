using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Models;
using Navred.Core.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Navred.Crawling.Crawlers
{
    public class VarnaBusStation : ICrawler
    {
        private const string DeparturesUrl = "https://autogaravn.com/index.php?module=schedules&action=departures&page={0}";
        private const string ArrivalsUrl = "https://autogaravn.com/index.php?module=schedules&action=arrivals";

        private readonly IRouteParser routeParser;
        private readonly ICultureProvider cultureProvider;
        private readonly ILegRepository repo;
        private readonly ILogger<VarnaBusStation> logger;
        private readonly IDictionary<string, string> replacements;

        public VarnaBusStation(
            IRouteParser routeParser,
            ICultureProvider cultureProvider,
            ILegRepository repo,
            ILogger<VarnaBusStation> logger)
        {
            this.routeParser = routeParser;
            this.cultureProvider = cultureProvider;
            this.repo = repo;
            this.logger = logger;
            this.replacements = new Dictionary<string, string>
            {
                { "горен близнак", "Близнаци" },
                { "долен близнак", "Близнаци" },
            };
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var departures = await this.GetLegsAsync(DeparturesUrl);
                var arrivals = await this.GetLegsAsync(ArrivalsUrl);
                var legs = new List<Leg>(departures);

                legs.AddRange(arrivals);

                await repo.UpdateLegsAsync(legs);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Update failed.");
            }
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var pageCount = this.GetPageCount(doc);
            var legs = new List<Leg>();

            for (int p = 1; p <= pageCount; p++)
            {
                var nextUrl = string.Format(url, p);
                var nextDoc = await web.LoadFromWebAsync(nextUrl);
                var rows = nextDoc.DocumentNode.SelectNodes("//div[contains(@class, 'row trip')]");

                foreach (var row in rows)
                {
                    try
                    {
                        var dow = this.GetDow(row);
                        var carrier = row.SelectSingleNode(".//a[descendant::i]").InnerText;
                        var info = row.SelectSingleNode(".//span[@class='pinfo']")?.InnerText ?? url;
                        var times = row.SelectNodes(".//span[@class='time-depart']")
                            .Select(n => Regex.Matches(n.InnerText, "(\\d{2}:\\d{2})").Last().Groups[1].Value)
                            .ToList();
                        var names = row.SelectNodes(".//div[@class='point-name']")
                            .Select(n => this.replacements.GetOrReturn(n.InnerText.Trim().ToLower()))
                            .ToList();
                        var addresses = row.SelectNodes(".//address").Select(n => n.InnerText).ToList();
                        var prices = row.SelectNodes(".//div[@class='point-price-wrap']")
                            .Select(n => n.InnerText).ToList();
                        var stops = Stop.CreateMany(names, times, prices, addresses);
                        var route = new Route(
                            this.cultureProvider.Name, dow, carrier, Mode.Bus, stops, url);
                        var currentLegs = await this.routeParser.ParseRouteAsync(
                            route, 
                            RouteOptions.RemoveDuplicates | RouteOptions.AdjustInvalidArrivals);

                        legs.AddRange(currentLegs);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, row.InnerText.Trim());
                    }
                }
            }

            return legs;
        }

        private int GetPageCount(HtmlDocument doc)
        {
            var lastPageText = doc.DocumentNode
                .SelectNodes("//ul[contains(@class, 'c-content-pagination')]/li/a")
                .Last()
                .GetAttributeValue("href", null);

            if (string.IsNullOrWhiteSpace(lastPageText))
            {
                return 10;
            }

            var lastPage = int.Parse(Regex.Match(lastPageText, "page=(\\d+)").Groups[1].Value);

            return lastPage;
        }

        private DaysOfWeek GetDow(HtmlNode row)
        {
            var dowStrings = row.SelectNodes(".//span[contains(@class, 'label ok')]")
                .Select(n => n.InnerText).ToList();
            var dow = this.cultureProvider.ToDaysOfWeek(dowStrings);

            return dow;
        }
    }
}
