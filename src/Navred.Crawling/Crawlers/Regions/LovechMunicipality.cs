using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core;
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
using System.Threading.Tasks;
using System.Web;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers
{
    public class LovechMunicipality : ICrawler
    {
        private const string Url = "https://www.lovech.bg/bg/transport/razpisanie-na-mezhduselishtnite-avtobusni-linii-v-obshtina-lovech";

        private readonly IRouteParser routeParser;
        private readonly ILegRepository repo;
        private readonly ILogger<LovechMunicipality> logger;
        private readonly IDictionary<string, string> regions;

        public LovechMunicipality(
            IRouteParser routeParser, ILegRepository repo, ILogger<LovechMunicipality> logger)
        {
            this.routeParser = routeParser;
            this.repo = repo;
            this.logger = logger;
            this.regions = new Dictionary<string, string>
            {
                { "Лисец", BCP.Region.LOV }
            };
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var legs = await this.GetLegsAsync(Url);

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
            var rows = doc.DocumentNode
                .SelectSingleNode("//table").SelectNodes("//tr").Skip(4).ToList();
            var all = new List<Leg>();

            foreach (var row in rows)
            {
                var data = row.SelectNodes(".//td").ToList();
                var names = data[3].InnerText.Replace("Автогара", BCP.Region.LOV).Split('-')
                    .ToDictionary(kvp => kvp, kvp => this.regions.GetOrDefault(kvp.Trim()));
                var weekDayTimes = this.GetStopTimes(data[4], data[5], names.Count);
                var weekendTimes = this.GetStopTimes(data[6], data[7], names.Count);
                var route = default(Route);

                try
                {
                    foreach (var times in weekDayTimes)
                    {
                        var stops = Stop.CreateMany(names.Keys, times, regions: names.Values);
                        route = new Route(
                            BulgarianCultureProvider.CountryName,
                            Constants.MondayToFriday,
                            "Община Ловеч",
                            Mode.Bus,
                            stops,
                            Url);
                        var legs = await this.routeParser.ParseRouteAsync(route);

                        all.AddRange(legs);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Row failed.");
                }

                try
                {
                    foreach (var times in weekendTimes)
                    {
                        var stops = Stop.CreateMany(names.Keys, times, regions: names.Values);
                        route = route.Copy(Constants.Weekend, stops);
                        var legs = await this.routeParser.ParseRouteAsync(route);

                        all.AddRange(legs);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Row failed.");
                }
            }

            return all;
        }

        private IList<IEnumerable<string>> GetStopTimes(HtmlNode first, HtmlNode second, int count)
        {
            var times = first
                .SelectNodes(".//p")
                .Where(n => !string.IsNullOrWhiteSpace(HttpUtility.HtmlDecode(n.InnerText)))
                .Zip(second
                    .SelectNodes(".//p")
                    .Where(n => !string.IsNullOrWhiteSpace(HttpUtility.HtmlDecode(n.InnerText))))
                .Select(fs => new[]
                {
                    HttpUtility.HtmlDecode(fs.First.InnerText),
                    HttpUtility.HtmlDecode(fs.Second.InnerText)
                }.InsertBetween(null, count))
                .ToList();

            return times;
        }
    }
}
