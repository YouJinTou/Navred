using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Estimation;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using Navred.Core.Tools;
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
        private const string PlovdivId = "56784";
        private readonly string RhodopiUrl = string.Format(Url, "-1", "450");

        private readonly ILegRepository repo;
        private readonly IPlacesManager placesManager;
        private readonly ITimeEstimator estimator;
        private readonly ICultureProvider cultureProvider;
        private readonly ILogger<PlovdivHebrosBus> logger;

        public PlovdivHebrosBus(
            ILegRepository repo,
            IPlacesManager placesManager,
            ITimeEstimator estimator,
            ICultureProvider cultureProvider,
            ILogger<PlovdivHebrosBus> logger)
        {
            this.repo = repo;
            this.placesManager = placesManager;
            this.estimator = estimator;
            this.cultureProvider = cultureProvider;
            this.logger = logger;
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                await this.ProcessAsync("http://hebrosbus.com/bg/pages/route-details/.6/100000517/2/450/702/56784/");
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(RhodopiUrl);
                var ids = doc.DocumentNode.SelectNodes("//select[@id='ddlArrive']/option")
                    .Select(n => n.GetAttributeValue("value", null))
                    .Where(v => !string.IsNullOrWhiteSpace(v) && !v.Equals(PlovdivId))
                    .ToList();
                
                await this.UpdateLegsAsync(ids);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Update failed.");
            }
        }

        private async Task UpdateLegsAsync(IEnumerable<string> ids)
        {
            await ids.RunBatchesAsync(30, async (d) =>
            {
                try
                {
                    var web = new HtmlWeb();
                    var mainDoc = await web.LoadFromWebAsync(string.Format(Url, PlovdivId, d));
                    var detailLinks = mainDoc.DocumentNode.SelectNodes("//a[@class='table_link']")
                        ?.Select(a => BaseUrl + a.GetAttributeValue("href", null)?.Replace("../", ""))
                        ?.Where(a => !string.IsNullOrWhiteSpace(a))
                        ?.ToList() ?? new List<string>();

                    await detailLinks.RunBatchesAsync(10, async (l) =>
                    {
                        try
                        {
                            await this.ProcessAsync(l);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, $"{l} failed.");
                        }
                    });
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"{d} failed.");
                }
            }, 30);
        }

        private async Task ProcessAsync(string link)
        {
            var web = new HtmlWeb();
            var detailsDoc = await web.LoadFromWebAsync(link);
            var fromPlace = this.placesManager.GetPlace(BCP.CountryName, BCP.City.Plovdiv);
            var carrier = detailsDoc.DocumentNode
                .SelectNodes("//span[@class='route_details_row']")[2].InnerText;
            var daysOfWeekStrings = detailsDoc.DocumentNode
                .SelectNodes("//span[@class='route_details_row bold_text']").Skip(4)
                .Select(n => n.InnerText)
                .ToList();
            var daysOfWeek = this.cultureProvider.ToDaysOfWeek(daysOfWeekStrings);
            var stopRows = detailsDoc.DocumentNode.SelectNodes(
                "//table[@class='route_table']/tr").Skip(1).ToList();
            var placesByName = this.placesManager.DeducePlacesFromStops(
                BCP.CountryName,
                stopRows.Select(r => r.SelectSingleNode("td").InnerText).Distinct().ToList());
            var sourceDeparture = TimeSpan.Parse(stopRows[0].SelectNodes("td")[3].InnerText);
            var lastPlace = fromPlace;
            var lastSpecific = BCP.City.Plovdiv;
            var lastDeparture = DateTime.UtcNow.Date + sourceDeparture;
            var schedule = new Schedule();
            var legSpread = Defaults.DaysAhead;

            foreach (var stopRow in stopRows.Skip(1))
            {
                var tds = stopRow.SelectNodes("td");
                var arrivalString = tds[2].InnerText;
                var departureString = tds[3].InnerText;

                if (Validator.AllNullOrWhiteSpace(arrivalString, departureString))
                {
                    continue;
                }

                var place = placesByName[tds[0].InnerText];
                var specificPlace = tds[1].InnerText;
                var arrivalTime = string.IsNullOrWhiteSpace(arrivalString) ?
                    (await this.estimator.EstimateArrivalTimeAsync(
                        lastPlace, place, lastDeparture, Mode.Bus)).TimeOfDay :
                    TimeSpan.Parse(arrivalString);
                var arrivalTimes =
                    daysOfWeek.GetValidUtcTimesAhead(arrivalTime, Defaults.DaysAhead).ToList();
                var departureTimes = daysOfWeek.GetValidUtcTimesAhead(
                    lastDeparture.TimeOfDay, Defaults.DaysAhead).ToList();
                var price = this.cultureProvider.ParsePrice(tds[4].InnerText);
                legSpread = departureTimes.Count;

                for (int t = 0; t < arrivalTimes.Count; t++)
                {
                    var leg = new Leg(
                        from: lastPlace,
                        to: place,
                        utcDeparture: departureTimes[t],
                        utcArrival: arrivalTimes[t],
                        carrier: carrier,
                        mode: Mode.Bus,
                        info: link,
                        price: price,
                        fromSpecific: lastSpecific,
                        toSpecific: specificPlace,
                        arrivalEstimated: string.IsNullOrWhiteSpace(tds[2].InnerText));

                    schedule.AddLeg(leg);
                }

                var departureTime = string.IsNullOrWhiteSpace(departureString) ?
                    arrivalTime : TimeSpan.Parse(departureString);
                lastPlace = place;
                lastSpecific = specificPlace;
                lastDeparture = DateTime.UtcNow.Date + departureTime;
            }

            var all = schedule.GetWithChildren(legSpread);

            await this.repo.UpdateLegsAsync(all);
        }
    }
}
