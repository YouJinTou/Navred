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
        private const string FromHisaryaUrl = "http://hebrosbus.com/bg/pages/route-details/.6/280/3%2c79999995231628/470/56784/77270/";
        private const string PlovdivId = "56784";

        private readonly ILegRepository repo;
        private readonly IPlacesManager placesManager;
        private readonly ITimeEstimator estimator;
        private readonly ICultureProvider cultureProvider;
        private readonly ILogger<PlovdivHebrosBus> logger;
        private readonly Place plovdiv;
        private readonly Place hisarya;
        private readonly string scrapeIdsUrl;
        private readonly ICollection<string> bannedPlaces;

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
            this.plovdiv = this.placesManager.GetPlace(
                BCP.CountryName, BCP.City.Plovdiv, BCP.Region.PDV);
            this.hisarya = this.placesManager.GetPlace(BCP.CountryName, BCP.City.Hisarya);
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

                await this.ProcessAsync(this.hisarya, FromHisaryaUrl);

                await this.UpdateLegsAsync(ids);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Update failed.");
            }
        }

        private async Task UpdateLegsAsync(IEnumerable<string> ids)
        {
            await ids.RunBatchesAsync(30, async (id) =>
            {
                try
                {
                    var web = new HtmlWeb();
                    var mainDoc = await web.LoadFromWebAsync(string.Format(Url, PlovdivId, id));
                    var detailLinks = mainDoc.DocumentNode.SelectNodes("//a[@class='table_link']")
                        ?.Select(a => BaseUrl + a.GetAttributeValue("href", null)?.Replace("../", ""))
                        ?.Where(a => !string.IsNullOrWhiteSpace(a))
                        ?.ToList() ?? new List<string>();
                    var all = new List<Leg>();

                    await detailLinks.RunBatchesAsync(10, async (l) =>
                    {
                        try
                        {
                            var legs = await this.ProcessAsync(this.plovdiv, l);

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

        private async Task<IEnumerable<Leg>> ProcessAsync(Place from, string link)
        {
            var web = new HtmlWeb();
            var detailsDoc = await web.LoadFromWebAsync(link);
            var carrier = detailsDoc.DocumentNode
                .SelectNodes("//span[@class='route_details_row']")?[2]?.InnerText;
            var daysOfWeekStrings = detailsDoc.DocumentNode
                .SelectNodes("//span[@class='route_details_row bold_text']").Skip(4)
                .Select(n => n.InnerText)
                .ToList();
            var daysOfWeek = this.cultureProvider.ToDaysOfWeek(daysOfWeekStrings);
            var stopRows = detailsDoc.DocumentNode.SelectNodes(
                "//table[@class='route_table']/tr").Skip(1).ToList();
            var placesByName = this.GetPlacesByName(stopRows);
            var sourceDeparture = TimeSpan.Parse(stopRows[0].SelectNodes("td")[3].InnerText);
            var lastPlace = from;
            var lastSpecific = detailsDoc.DocumentNode.SelectSingleNode(
                "//span[@class='route_details_row']").InnerText;
            var firstAvailableDate = daysOfWeek.GetFirstAvailableUtcDate();
            var lastDeparture = firstAvailableDate + sourceDeparture;
            var schedule = new Schedule();

            for (int sr = 1; sr < stopRows.Count; sr++)
            {
                var row = this.ParseRow(stopRows[sr]);
                var place = placesByName[row.PlaceName];

                if (Validator.AllNullOrWhiteSpace(row.Arrival, row.Departure) || place == null)
                {
                    continue;
                }

                var arrivalTime = await this.GetArrivalAsync(row, lastPlace, place, lastDeparture);
                var arrivalTimes =
                    daysOfWeek.GetValidUtcTimesAhead(arrivalTime, Defaults.DaysAhead).ToList();
                var departureTimes = daysOfWeek.GetValidUtcTimesAhead(
                    lastDeparture.TimeOfDay, Defaults.DaysAhead).ToList();
                decimal? price = this.GetPrice(row, sr, stopRows);

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
                        toSpecific: row.SpecificName,
                        arrivalEstimated: string.IsNullOrWhiteSpace(row.Arrival));

                    schedule.AddLeg(leg);
                }

                lastPlace = place;
                lastSpecific = row.SpecificName;
                var departureTime = this.GetDeparture(arrivalTime, row.Departure, sr, stopRows);
                lastDeparture = firstAvailableDate + departureTime;
            }

            var all = schedule.GetWithChildren();

            return all;
        }

        private IDictionary<string, Place> GetPlacesByName(IEnumerable<HtmlNode> stopRows)
        {
            var stopNames = stopRows
                .Select(r => r.SelectSingleNode("td").InnerText)
                .Where(p => !bannedPlaces.Contains(p))
                .Distinct()
                .ToList();

            if (stopNames.Count > 2)
            {
                return this.placesManager.DeducePlacesFromStops(BCP.CountryName, stopNames, false);
            }

            var to = this.placesManager.GetPlace(BCP.CountryName, stopNames[1]);
            var placesByName = new Dictionary<string, Place>
            {
                { stopNames[1], to }
            };

            return placesByName;
        }

        private async Task<TimeSpan> GetArrivalAsync(
            RowData row, Place lastPlace, Place place, DateTime lastDeparture)
        {
            var arrivalTime = string.IsNullOrWhiteSpace(row.Arrival) ?
                (await this.estimator.EstimateArrivalTimeAsync(
                    lastPlace, place, lastDeparture, Mode.Bus)).TimeOfDay :
                TimeSpan.Parse(row.Arrival);
            arrivalTime = (arrivalTime <= lastDeparture.TimeOfDay) ?
                lastDeparture.TimeOfDay.AddMinutes(1) : arrivalTime;

            return arrivalTime;
        }

        private TimeSpan GetDeparture(
            TimeSpan arrival, string departureString, int current, IList<HtmlNode> stopRows)
        {
            var isLastStop = current.Equals(stopRows.Count - 1);

            if (isLastStop || string.IsNullOrWhiteSpace(departureString))
            {
                return arrival.AddMinutes(1);
            }

            var departureTime = TimeSpan.Parse(departureString);
            var nextStop = default(RowData);

            for (int s = current + 1; s < stopRows.Count; s++)
            {
                nextStop = this.ParseRow(stopRows[s]);

                if (!string.IsNullOrEmpty(nextStop.Arrival))
                {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(nextStop.Arrival))
            {
                return departureTime;
            }

            var nextStopArrival = TimeSpan.Parse(nextStop.Arrival);
            var hasError = (nextStopArrival <= departureTime);

            if (hasError)
            {
                return arrival.AddMinutes(1);
            }

            return departureTime;
        }

        private decimal? GetPrice(RowData row, int sr, List<HtmlNode> stopRows)
        {
            if (sr.Equals(stopRows.Count - 1))
            {
                return this.cultureProvider.ParsePrice(row.Price);
            }

            var rowToExtractPriceFrom = row;

            for (int i = sr + 1; i < stopRows.Count; i++)
            {
                var nextRow = this.ParseRow(stopRows[sr + 1]);

                if (nextRow.PlaceName.Equals(row.PlaceName))
                {
                    rowToExtractPriceFrom = nextRow;
                }
            }

            return this.cultureProvider.ParsePrice(rowToExtractPriceFrom.Price);
        }

        private RowData ParseRow(HtmlNode row)
        {
            var tds = row.SelectNodes("td");

            return new RowData
            {
                Arrival = tds[2].InnerText,
                Departure = tds[3].InnerText,
                PlaceName = tds[0].InnerText,
                SpecificName = tds[1].InnerText,
                Price = tds[4].InnerText
            };
        }

        private class RowData
        {
            public string Arrival { get; set; }

            public string Departure { get; set; }

            public string PlaceName { get; set; }

            public string SpecificName { get; set; }

            public string Price { get; set; }
        }
    }
}
