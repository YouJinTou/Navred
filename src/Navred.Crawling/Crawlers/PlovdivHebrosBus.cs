using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Crawling.Crawlers
{
    public class PlovdivHebrosBus : ICrawler
    {
        private const string BaseUrl = "http://hebrosbus.com/bg/";
        private const string Url = "http://hebrosbus.com/bg/search/razpisaniya/.5/{0}/{1}/";
        private const string PlovdivId = "56784";
        private readonly string RhodopiUrl = string.Format(Url, "-1", "450");

        private readonly ILegRepository repo;
        private readonly IPlacesManager placesManager;
        private readonly ILogger<PlovdivHebrosBus> logger;

        public PlovdivHebrosBus(
            ILegRepository repo,
            IPlacesManager placesManager,
            ILogger<PlovdivHebrosBus> logger)
        {
            this.repo = repo;
            this.placesManager = placesManager;
            this.logger = logger;
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(RhodopiUrl);
                var ids = doc.DocumentNode.SelectNodes("//select[@id='ddlArrive']/option")
                    .Select(n => n.GetAttributeValue("value", null))
                    .Where(v => !string.IsNullOrWhiteSpace(v) && !v.Equals(PlovdivId))
                    .ToList();
                var legs = await this.GetLegsAsync(ids);

                await repo.UpdateLegsAsync(legs);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Update failed.");
            }
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync(IEnumerable<string> ids)
        {
            var from = this.placesManager.GetPlace(
                BulgarianCultureProvider.CountryName, "Пловдив");
            var legs = new List<Leg>();
            var web = new HtmlWeb();

            await ids.RunBatchesAsync(30, async (d) =>
            {
                try
                {
                    var mainDoc = await web.LoadFromWebAsync(string.Format(Url, PlovdivId, d));
                    var detailLinks = mainDoc.DocumentNode.SelectNodes("//a[@class='table_link']")
                        ?.Select(a => BaseUrl + a.GetAttributeValue("href", null)?.Replace("../", ""))
                        ?.Where(a => !string.IsNullOrWhiteSpace(a))
                        ?.ToList() ?? new List<string>();

                    await detailLinks.RunBatchesAsync(10, async (l) =>
                    {
                        var detailsDoc = await web.LoadFromWebAsync(l);
                        var stopRows = detailsDoc.DocumentNode.SelectNodes(
                            "//table[@class='route_table']/tr").Skip(1).ToList();
                        var schedule = new Schedule();

                        foreach (var stopRow in stopRows.Skip(1))
                        {
                            var tds = stopRow.SelectNodes("td");
                            var place = tds[0].InnerText;
                            var to = this.placesManager.GetPlace(
                                BulgarianCultureProvider.CountryName,
                                place);
                            var specificPlace = tds[1].InnerText;
                            var departsAt = tds[2].InnerText;
                            var arrivesAt = tds[3].InnerText;
                            var price = tds[4].InnerText;
                            //var leg = new Leg(
                            //    from: from,
                            //    to: to,
                            //    )
                        }

                    });
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"{d} failed.");
                }
            }, 30);

            return legs;
        }
    }
}
