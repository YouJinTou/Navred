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
using System.Threading.Tasks;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers
{
    public class PlovdivHebrosBus : ICrawler
    {
        private const string BaseUrl = "http://hebrosbus.com/bg/";
        private const string Url = "http://hebrosbus.com/bg/search/razpisaniya/.5/{0}/{1}/";
        private const string FromHisaryaUrl = "http://hebrosbus.com/bg/pages/route-details/.6/280/3%2c79999995231628/470/56784/77270/";
        private const string PlovdivId = "56784";

        private readonly IRouteParser routeParser;
        private readonly ILegRepository repo;
        private readonly ICultureProvider cultureProvider;
        private readonly ILogger<PlovdivHebrosBus> logger;
        private readonly string scrapeIdsUrl;
        private readonly ICollection<string> bannedPlaces;

        public PlovdivHebrosBus(
            IRouteParser routeParser,
            ILegRepository repo,
            ICultureProvider cultureProvider,
            ILogger<PlovdivHebrosBus> logger)
        {
            this.routeParser = routeParser;
            this.repo = repo;
            this.cultureProvider = cultureProvider;
            this.logger = logger;
            this.scrapeIdsUrl = string.Format(Url, "-1", "450");
            this.bannedPlaces = new HashSet<string> { "Точиларци" };
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(scrapeIdsUrl);
                var ids = doc.DocumentNode.SelectNodes("//select[@id='ddlArrive']/option")
                    .Select(n => n.GetAttributeValue("value", null))
                    .Where(v => !string.IsNullOrWhiteSpace(v) && !v.Equals(PlovdivId))
                    .ToList();

                await this.ProcessAsync(FromHisaryaUrl);

                await this.UpdateLegsAsync(ids, true);

                await this.UpdateLegsAsync(ids, false);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Update failed.");
            }
        }

        private async Task UpdateLegsAsync(IEnumerable<string> ids, bool isDeparture)
        {
            await ids.RunBatchesAsync(30, async (id) =>
            {
                try
                {
                    var web = new HtmlWeb();
                    var fromId = isDeparture ? PlovdivId : id;
                    var toId = isDeparture ? id : PlovdivId;
                    var mainDoc = await web.LoadFromWebAsync(string.Format(Url, fromId, toId));
                    var detailLinks = mainDoc.DocumentNode.SelectNodes("//a[@class='table_link']")
                        ?.Select(a => BaseUrl + a.GetAttributeValue("href", null)?.Replace("../", ""))
                        ?.Where(a => !string.IsNullOrWhiteSpace(a))
                        ?.ToList() ?? new List<string>();
                    var all = new List<Leg>();

                    await detailLinks.RunBatchesAsync(10, async (l) =>
                    {
                        try
                        {
                            var legs = await this.ProcessAsync(l);

                            all.AddRange(legs);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, $"{l} failed.");
                        }
                    }, delayBetweenBatches: 1000, delayBetweenBatchItems: 100);

                    await this.repo.UpdateLegsAsync(all);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"{id} failed.");
                }
            }, 30);
        }

        private async Task<IEnumerable<Leg>> ProcessAsync(string link)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(link);
            var carrier = doc.DocumentNode
                .SelectNodes("//span[@class='route_details_row']")?[2]?.InnerText;
            var daysOfWeekStrings = doc.DocumentNode
                .SelectNodes("//span[@class='route_details_row bold_text']").Skip(4)
                .Select(n => n.InnerText)
                .ToList();
            var dow = this.cultureProvider.ToDaysOfWeek(daysOfWeekStrings);
            var addresses = doc.DocumentNode.SelectNodes(
                "//td[@class='cityColumn' and position() mod 2 = 0]").Select(s => s.InnerText);
            var names = doc.DocumentNode.SelectNodes(
                "//td[@class='cityColumn' and position() mod 2 = 1]").Select(s => s.InnerText);
            var firstStopTime = doc.DocumentNode
                .SelectNodes("//table[@class='route_table']//td[position() mod 4 = 0]")
                .First().InnerText;
            var times = doc.DocumentNode
                .SelectNodes("//table[@class='route_table']//td[position() mod 3 = 0]")
                .Select(t => t.InnerText)
                .Skip(1)
                .ToList();
            var allStopTimes = firstStopTime.AsList().Concat(times);
            var prices = doc.DocumentNode
                .SelectNodes("//table[@class='route_table']//td[position() mod 5 = 0]")
                .Select(t => t.InnerText).ToList();
            var stops = Stop.CreateMany(names, times, prices, addresses);
            var route = new Route(BCP.CountryName, dow, carrier, Mode.Bus, stops, link);
            var legs = await this.routeParser.ParseRouteAsync(route);

            return legs;
        }
    }
}
