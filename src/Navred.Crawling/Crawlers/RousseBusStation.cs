using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Estimation;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using Navred.Crawling.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Navred.Crawling.Crawlers
{
    public class RousseBusStation : ICrawler
    {
        private const string Departures = "0";
        private const string Arrivals = "1";
        private const string IdsUrl = "https://www.avtogararuse.org/razpisanie.cgi";
        private const string DetailsUrl = "https://www.avtogararuse.org/result.cgi?id={0}&rev={1}";

        private readonly IPlacesManager placesManager;
        private readonly ICultureProvider cultureProvider;
        private readonly ITimeEstimator estimator;
        private readonly ILegRepository repo;
        private readonly ILogger<Template> logger;

        public RousseBusStation(
            IPlacesManager placesManager,
            ICultureProvider cultureProvider,
            ITimeEstimator estimator,
            ILegRepository repo,
            ILogger<Template> logger)
        {
            this.placesManager = placesManager;
            this.cultureProvider = cultureProvider;
            this.estimator = estimator;
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
            var stopInfos = this.GetStops(doc.DocumentNode, rev);
            var schedule = new Schedule();

            for (int st = 0; st < stopInfos.Count - 1; st++)
            {
                try
                {
                    var departureTimes = dow.GetValidUtcTimesAhead(
                    stopInfos[st].Time, Defaults.DaysAhead);
                    var arrivalTimes = dow.GetValidUtcTimesAhead(
                        stopInfos[st + 1].Time, Defaults.DaysAhead);

                    foreach (var (departure, arrival) in departureTimes.Zip(arrivalTimes))
                    {
                        var fixedArrival = (departure >= arrival) ?
                            await this.estimator.EstimateArrivalTimeAsync(
                                stopInfos[st].Stop, stopInfos[st + 1].Stop, departure, Mode.Bus) :
                            arrival;
                        var leg = new Leg(
                            from: stopInfos[st].Stop,
                            to: stopInfos[st + 1].Stop,
                            utcDeparture: departure,
                            utcArrival: fixedArrival,
                            carrier: carrier,
                            mode: Mode.Bus,
                            info: info,
                            price: stopInfos[st + 1].Price);

                        schedule.AddLeg(leg);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, stopInfos[st].ToString());
                }
            }

            return schedule.Permute();
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

        private IList<StopInfo> GetStops(HtmlNode doc, string rev)
        {
            var stopInfos = new List<StopInfo>();
            var stops = doc.SelectNodes("//div[@class='panel style1']//a")
                .Select(a => a.InnerText).ToList();
            var data = doc.SelectNodes("//div[@class='panel-content']").ToList();

            if (!stops.Count.Equals(data.Count))
            {
                throw new InvalidOperationException("Stop data count mismatch.");
            }

            var places = this.placesManager.DeducePlacesFromStops(
                this.cultureProvider.Name, stops, false).Select(kvp => kvp.Value).ToList();

            foreach (var (place, datum) in places.Zip(data))
            {
                if (place == null)
                {
                    continue;
                }

                var departure = Regex.Match(
                    datum.InnerText, @"тръгване\s*[-:]\s*(\d{1,2}:\d{1,2})").Groups[1].Value;
                departure = string.IsNullOrWhiteSpace(departure) ?
                    Regex.Match(
                    datum.InnerText, @"пристигане\s*[-:]\s*(\d{1,2}:\d{1,2})").Groups[1].Value :
                    departure;
                var price = Regex.Match(
                    datum.InnerText,
                    @"(\d+[\.,]?\d*)\s*(?:(?:лева)|(?:лв\.?))").Groups[1].Value.Replace(',', '.');
                var isDeparture = rev.Equals(Departures);

                stopInfos.Add(new StopInfo
                {
                    Price = string.IsNullOrWhiteSpace(price) ? 
                        null : 
                        isDeparture ? decimal.Parse(price) : (decimal?)null,
                    Stop = place,
                    Time = TimeSpan.Parse(departure)
                });
            }

            return stopInfos;
        }
    }
}
