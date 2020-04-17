using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Models;
using Navred.Core.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Navred.Crawling.Crawlers.Regions
{
    public class RousseBusStation : ICrawler
    {
        private const string Departures = "0";
        private const string Arrivals = "1";
        private const string IdsUrl = "https://www.avtogararuse.org/razpisanie.cgi";
        private const string DetailsUrl = "https://www.avtogararuse.org/result.cgi?id={0}&rev={1}";

        private readonly IRouteParser routeParser;
        private readonly ICultureProvider cultureProvider;
        private readonly ILegRepository repo;
        private readonly ILogger<Template> logger;

        public RousseBusStation(
            IRouteParser routeParser,
            ICultureProvider cultureProvider,
            ILegRepository repo,
            ILogger<Template> logger)
        {
            this.routeParser = routeParser;
            this.cultureProvider = cultureProvider;
            this.repo = repo;
            this.logger = logger;
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var legs = await this.GetLegsAsync(IdsUrl);

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
            var ids = doc.DocumentNode.SelectNodes("//h4[@class='box-title']/a")
                .Select(a => a.GetAttributeValue("href", null))
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(u => Regex.Match(u, @"id=(\d+)").Groups[1].Value)
                .ToList();
            var legs = new List<Leg>();

            foreach (var id in ids)
            {
                try
                {
                    var departures = await ProcessIdAsync(id, Departures);
                    var arrivals = await ProcessIdAsync(id, Arrivals);

                    legs.AddRange(departures);

                    legs.AddRange(arrivals);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, id);
                }
            }

            return legs;
        }

        private async Task<IEnumerable<Leg>> ProcessIdAsync(string id, string rev)
        {
            var web = new HtmlWeb();
            var url = string.Format(DetailsUrl, id, rev);
            var doc = await web.LoadFromWebAsync(url);
            var infoBoxParagraphs = doc.DocumentNode.SelectNodes("//div[@class='box col-sm-6']/p");
            var carrier = Regex.Match(infoBoxParagraphs[3].InnerText, @"-\s+(.*)").Groups[1].Value;
            var info = this.GetInfo(url, infoBoxParagraphs[4].InnerText);
            var dow = this.GetDow(infoBoxParagraphs[5].InnerText);
            var names = doc.DocumentNode.SelectNodes("//div[@class='panel style1']//a")
               .Select(a => a.InnerText).ToList();
            var (times, prices) = this.GetStopData(doc.DocumentNode, rev);
            var stops = Stop.CreateMany(
                names, times, prices, timesToMarkAsEstimable: new[] { "00:00" });
            var route = new Route(this.cultureProvider.Name, dow, carrier, Mode.Bus, stops, info);
            var legs = await this.routeParser.ParseRouteAsync(route);

            return legs;
        }

        private DaysOfWeek GetDow(string dowText)
        {
            var days = Regex.Match(dowText, @"-\s+(.*)").Groups[1].Value.Split(
                ';', StringSplitOptions.RemoveEmptyEntries);
            var dow = this.cultureProvider.ToDaysOfWeek(days);

            return dow;
        }

        private string GetInfo(string url, string phoneText)
        {
            var phone = Regex.Match(phoneText, @"-\s+(.*)").Groups[1].Value;

            if (string.IsNullOrWhiteSpace(phone))
            {
                return url;
            }

            var result = $"{url} ({phone})";

            return result;
        }

        private (IEnumerable<string>, IEnumerable<string>) GetStopData(
            HtmlNode doc, string rev)
        {
            var data = doc.SelectNodes("//div[@class='panel-content']").ToList();
            var times = new List<string>();
            var prices = new List<string>();
            var isDeparture = rev.Equals(Departures);

            foreach (var datum in data)
            {
                var departure = Regex.Match(
                  datum.InnerText, @"тръгване\s*[-:]\s*(\d{1,2}:\d{1,2})").Groups[1].Value;
                departure = string.IsNullOrWhiteSpace(departure) ?
                    Regex.Match(
                    datum.InnerText, @"пристигане\s*[-:]\s*(\d{1,2}:\d{1,2})").Groups[1].Value :
                    departure;
                var price = Regex.Match(
                    datum.InnerText,
                    @"(\d+[\.,]?\d*)\s*(?:(?:лева)|(?:лв\.?))").Groups[1].Value.Replace(',', '.');
                price = string.IsNullOrWhiteSpace(price) ? 
                    null : isDeparture ? price : null;

                times.Add(departure);

                prices.Add(price);
            }

            return (times, prices);
        }
    }
}
