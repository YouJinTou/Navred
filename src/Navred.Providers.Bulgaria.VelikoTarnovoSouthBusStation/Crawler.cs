using HtmlAgilityPack;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Estimation;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.VelikoTarnovoSouthBusStation
{
    public class Crawler : ICrawler
    {
        private const string Url = "http://avtogaratarnovo.eu/?mode=schedule";

        private readonly IPlacesManager placesManager;
        private readonly ITimeEstimator timeEstimator;
        private readonly ILegRepository repo;

        public Crawler(
            IPlacesManager placesManager,
            ITimeEstimator timeEstimator,
            ILegRepository repo,
            ICultureProvider cultureProvider)
        {
            this.placesManager = placesManager;
            this.timeEstimator = timeEstimator;
            this.repo = repo;
            Console.OutputEncoding = cultureProvider.GetEncoding();
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var legs = await this.GetLegsAsync(Url);

                await this.repo.UpdateLegsAsync(legs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var legs = new List<Leg>();
            var trs = doc.DocumentNode.SelectNodes("//div[@class='table-responsive']//tr")
                .TakeAllButLast(1)
                .ToList();
            var from = this.placesManager.GetPlace(
                BulgarianCultureProvider.CountryName,
                BulgarianCultureProvider.City.VelikoTarnovo);

            foreach (var tr in trs)
            {
                try
                {
                    var tds = tr.SelectNodes("td").ToList();
                    var region = this.GetRegion(tds[2].InnerText);
                    var to = this.placesManager.GetPlace(
                        BulgarianCultureProvider.CountryName, tds[2].InnerText, region);
                    var departureTime = tds[3].InnerText;
                    var carrier = tds[4].InnerText;
                    var price = tds[5].InnerText.StripCurrency();
                    var utcDepartures = Constants.AllWeek.GetValidUtcTimesAhead(departureTime, 10);
                    var utcArrivals = utcDepartures
                        .Select(async d => await this.timeEstimator.EstimateArrivalTimeAsync(
                            from, to, d, Mode.Bus))
                        .Select(t => t.Result)
                        .ToList();

                    foreach (var (departure, arrival) in utcDepartures.Zip(utcArrivals))
                    {
                        var leg = new Leg(
                            from: from,
                            to: to,
                            utcDeparture: departure,
                            utcArrival: arrival,
                            carrier: carrier,
                            mode: Mode.Bus,
                            info: Url,
                            price: price,
                            arrivalEstimated: true);

                        legs.Add(leg);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return legs;
        }

        private string GetRegion(string to)
        {
            var formattedTo = this.placesManager.FormatPlace(to);
            var regionByPlace = new Dictionary<string, string>
            {
                { "търговище", BulgarianCultureProvider.Region.TGV },
                { "добрич", BulgarianCultureProvider.Region.DOB },
                { "попово", BulgarianCultureProvider.Region.TGV },
                { "разград", BulgarianCultureProvider.Region.RAZ },
            };

            return regionByPlace.ContainsKey(formattedTo) ? regionByPlace[formattedTo] : null;
        }
    }
}
