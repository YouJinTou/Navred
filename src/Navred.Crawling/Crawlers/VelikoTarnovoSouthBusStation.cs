using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using Navred.Core.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers
{
    public class VelikoTarnovoSouthBusStation : ICrawler
    {
        private const string DeparturesUrl = "http://avtogaratarnovo.eu/?mode=schedule";
        private const string ArrivalsUrl = "http://avtogaratarnovo.eu/?mode=schedule&type=arrive";

        private readonly IRouteParser routeParser;
        private readonly IPlacesManager placesManager;
        private readonly ILegRepository repo;
        private readonly ILogger<VelikoTarnovoSouthBusStation> logger;

        public VelikoTarnovoSouthBusStation(
            IRouteParser routeParser,
            IPlacesManager placesManager,
            ILegRepository repo,
            ICultureProvider cultureProvider,
            ILogger<VelikoTarnovoSouthBusStation> logger)
        {
            this.routeParser = routeParser;
            this.placesManager = placesManager;
            this.repo = repo;
            this.logger = logger;
            Console.OutputEncoding = cultureProvider.GetEncoding();
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var departures = await this.GetLegsAsync(DeparturesUrl);
                var arrivals = await this.GetLegsAsync(ArrivalsUrl);
                var legs = new List<Leg>(departures);

                legs.AddRange(arrivals);

                await this.repo.UpdateLegsAsync(legs);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Update failed.");
            }
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync(string url)
        {
            var web = new HtmlWeb();
            var isDeparture = url.Equals(DeparturesUrl);
            var doc = await web.LoadFromWebAsync(url);
            var all = new List<Leg>();
            var trs = doc.DocumentNode.SelectNodes("//div[@class='table-responsive']//tr")
                .TakeAllButLast(1)
                .ToList();

            foreach (var tr in trs)
            {
                try
                {
                    var tds = tr.SelectNodes("td").ToList();
                    var region = this.GetRegion(isDeparture ? tds[2].InnerText : tds[1].InnerText);
                    var stops = new List<string> { tds[1].InnerText, tds[2].InnerText };
                    var stopTimes = new List<LegTime> { null, new LegTime(tds[3].InnerText) };
                    var prices = new List<string> { null, tds[5].InnerText };
                    var carrier = tds[4].InnerText;
                    var route = new Route(
                        BCP.CountryName,
                        Constants.AllWeek,
                        carrier,
                        Mode.Bus,
                        stopTimes,
                        stops,
                        prices: prices,
                        info: isDeparture ? DeparturesUrl : ArrivalsUrl);
                    var legs = await routeParser.ParseRouteAsync(
                        route, StopTimeOptions.EstimateDeparture);

                    all.AddRange(legs);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, url);
                }
            }

            return all;
        }

        private string GetRegion(string to)
        {
            var formattedTo = this.placesManager.FormatPlace(to);
            var regionByPlace = new Dictionary<string, string>
            {
                { "търговище", BCP.Region.TGV },
                { "добрич", BCP.Region.DOB },
                { "попово", BCP.Region.TGV },
                { "разград", BCP.Region.RAZ },
            };

            return regionByPlace.ContainsKey(formattedTo) ? regionByPlace[formattedTo] : null;
        }
    }
}
