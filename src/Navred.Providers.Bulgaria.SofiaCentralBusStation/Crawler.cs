using HtmlAgilityPack;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Piecing;
using Navred.Core.Places;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.SofiaCentralBusStation
{
    public class Crawler : ICrawler
    {
        private const string Url = 
            "https://www.centralnaavtogara.bg/index.php?mod=0461ebd2b773878eac9f78a891912d65";

        private readonly ILegRepository repo;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IBulgarianCultureProvider provider;
        private readonly IPlacesManager placesManager;
        private readonly ITimeEstimator estimator;
        private readonly Encoding windows1251;

        public Crawler(
            ILegRepository repo, 
            IHttpClientFactory httpClientFactory, 
            IBulgarianCultureProvider provider,
            IPlacesManager placesManager,
            ITimeEstimator estimator)
        {
            this.repo = repo;
            this.httpClientFactory = httpClientFactory;
            this.provider = provider;
            this.placesManager = placesManager;
            this.estimator = estimator;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            this.windows1251 = Encoding.GetEncoding("windows-1251");
        }

        public async Task UpdateLegsAsync()
        {
            var legs = await this.GetLegsAsync(Url);

            await this.repo.UpdateLegsAsync(legs);
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url, encoding: this.windows1251);
            var destinations = doc.DocumentNode
                .SelectNodes("//form[@id='iq_form']/select[@id='city_menu']/option")
                .Skip(3)
                .Select(v => Regex.Match(v.OuterHtml, "value=\"(.*?)\">").Groups[1].Value)
                .ToList();
            var httpClient = this.httpClientFactory.CreateClient();
            var dates = DateTime.UtcNow.GetDateTimesAhead(30)
                .Select(dt => dt.ToString("dd.MM.yyyy")).ToList();
            var legs = new List<Leg>();

            foreach (var date in dates)
            {
                foreach (var destination in destinations)
                {
                    try
                    {
                        var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("city_menu", destination),
                        new KeyValuePair<string, string>("for_date", date),
                    });
                        var response = await httpClient.PostAsync(Url, content);
                        var responseText = await response.Content.ReadAsByteArrayAsync();
                        var encodedText = this.windows1251.GetString(responseText);
                        var currentLegs = this.GetLegs(encodedText, date);

                        legs.AddRange(currentLegs);
                    }
                    catch (Exception ex)
                    {
                        Console.OutputEncoding = this.windows1251;

                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        await Task.Delay(250);
                    }
                }
            }

            return legs;
        }

        private IEnumerable<Leg> GetLegs(string encodedText, string dateString)
        {
            var legs = new List<Leg>();
            var doc = new HtmlDocument();
            encodedText = encodedText.Replace("\t", "").Replace("\n", "");

            doc.LoadHtml(encodedText);

            var oddTables = doc.DocumentNode.SelectNodes("//table[@class='result_table_odd']");
            var evenTables = doc.DocumentNode.SelectNodes("//table[@class='result_table_even']");
            var tables = (oddTables == null) ? 
                new List<HtmlNode>() : evenTables == null ? oddTables : 
                oddTables.Concat(evenTables);

            if (tables.IsNullOrEmpty())
            {
                return legs;
            }

            var date = DateTime.ParseExact(dateString, "dd.MM.yyyy", null);

            foreach (var table in tables)
            {
                var dataRow = table.FirstChild;
                var fromTo = dataRow.FirstChild.FirstChild.InnerText.Split('-');
                fromTo = (fromTo.Length == 1) ? 
                    dataRow.FirstChild.InnerText.Split('-') : fromTo;
                var from = this.provider.NormalizePlaceName(
                    fromTo[0], this.GetRegionCode(fromTo[0]));
                var to = this.provider.NormalizePlaceName(
                    fromTo[1], this.GetRegionCode(fromTo[1]));
                var carrier = dataRow.ChildNodes[1].InnerText;
                var departureTimeString =
                    Regex.Match(dataRow.ChildNodes[3].InnerText, @"(\d+:\d+)").Groups[1].Value;
                var departure = date + TimeSpan.Parse(departureTimeString);
                var arrival = this.GetArrival(from, to, departure);
                var priceString = dataRow.ChildNodes[5].InnerText;
                var price = decimal.Parse(Regex.Match(priceString, @"(\d+\.\d+)").Groups[1].Value);
                var leg = new Leg(
                    from, 
                    to, 
                    departure, 
                    arrival, 
                    carrier, 
                    Mode.Bus, 
                    price, 
                    arrivalEstimated: true);

                legs.Add(leg);
            }

            return legs;
        }

        private string GetRegionCode(string place)
        {
            place = place.ToLower().Trim();
            var codeByPlace = new Dictionary<string, string>
            {
                { "добрич", BulgarianCultureProvider.Region.DOB },
                { "априлци", BulgarianCultureProvider.Region.LOV },
            };

            return codeByPlace.ContainsKey(place) ? codeByPlace[place] : null;
        }

        private DateTime GetArrival(string from, string to, DateTime departure)
        {
            var fromCode = this.GetRegionCode(from);
            var toCode = this.GetRegionCode(to);
            var fromPlace = this.placesManager.GetPlace<BulgarianPlace>(
                BulgarianCultureProvider.CountryName, from, fromCode);
            var toPlace = this.placesManager.GetPlace<BulgarianPlace>(
                BulgarianCultureProvider.CountryName, to, toCode);
            var arrival = this.estimator.EstimateArrivalTime(
                fromPlace, toPlace, departure, Mode.Bus);

            return arrival;
        }
    }
}
