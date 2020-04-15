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
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers
{
    public class SofiaCentralBusStation : ICrawler
    {
        private const string DeparturesUrl =
            "https://www.centralnaavtogara.bg/index.php?mod=0461ebd2b773878eac9f78a891912d65";
        private const string ArrivalsUrl = 
            "https://www.centralnaavtogara.bg/index.php?mod=06a943c59f33a34bb5924aaf72cd2995&d=c#b";

        private readonly IRouteParser routeParser;
        private readonly ILegRepository repo;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ICultureProvider cultureProvider;
        private readonly ILogger<SofiaCentralBusStation> logger;
        private readonly IDictionary<string, string> replacements;

        public SofiaCentralBusStation(
            IRouteParser routeParser,
            ILegRepository repo,
            IHttpClientFactory httpClientFactory,
            ICultureProvider cultureProvider,
            ILogger<SofiaCentralBusStation> logger)
        {
            this.routeParser = routeParser;
            this.repo = repo;
            this.httpClientFactory = httpClientFactory;
            this.cultureProvider = cultureProvider;
            this.logger = logger;
            this.replacements = new Dictionary<string, string>
            {
                { "бели мел", "Белимел" }
            };
            Console.OutputEncoding = this.cultureProvider.GetEncoding();
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                await this.UpdateAsync(DeparturesUrl);

                await this.UpdateAsync(ArrivalsUrl);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Update failed.");
            }
        }

        private async Task UpdateAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(
                url, encoding: this.cultureProvider.GetEncoding());
            var destinations = doc.DocumentNode
                .SelectNodes("//select[@id='city_menu']/option")
                .Skip(3)
                .Select(v => Regex.Match(v.OuterHtml, "value=\"(.*?)\">").Groups[1].Value)
                .Distinct()
                .ToList();
            var httpClient = this.httpClientFactory.CreateClient();
            var dates = DateTime.UtcNow.GetDateTimesAhead(Defaults.DaysAhead)
                .Select(dt => dt.ToString("dd.MM.yyyy")).ToList();

            foreach (var date in dates)
            {
                var legs = new List<Leg>();

                await destinations.RunBatchesAsync(20, async (d) =>
                {
                    try
                    {
                        var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("city_menu", d),
                            new KeyValuePair<string, string>("for_date", date),
                        });
                        var response = await httpClient.PostAsync(url, content);
                        var responseText = await response.Content.ReadAsByteArrayAsync();
                        var encodedText = this.cultureProvider.GetEncoding().GetString(responseText);
                        var currentLegs = await this.GetLegsAsync(encodedText, date, d, url);

                        legs.AddRange(currentLegs);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"{d} failed.");
                    }
                }, 200, 5, maxRetries: 3);

                await this.repo.UpdateLegsAsync(legs);
            }
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync(
            string encodedText, string dateString, string placeString, string url)
        {
            var all = new List<Leg>();
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
                return all;
            }

            var date = DateTime.ParseExact(dateString, "dd.MM.yyyy", null);
            var isDeparture = url.Equals(DeparturesUrl);

            foreach (var table in tables)
            {
                var dataRow = table.FirstChild;
                var resultDays = dataRow.SelectNodes("//li[@class='rd_green']//text()")
                    ?.Select(n => n.InnerText)?.ToList() ?? new List<string>();
                var dow = this.cultureProvider.ToDaysOfWeek(resultDays);
                var carrier = dataRow.ChildNodes[1].InnerText;
                var timeString =
                    Regex.Match(dataRow.ChildNodes[3].InnerText, @"(\d+:\d+)").Groups[1].Value;
                var times = isDeparture ?
                    new List<string> { timeString, null } :
                    new List<string> { null, timeString };
                var names = isDeparture ? 
                    new List<string> { BCP.City.Sofia, placeString } : 
                    new List<string> { placeString, BCP.City.Sofia };
                var prices = new List<string> { dataRow.ChildNodes[5].InnerText, null };
                var stops = Stop.CreateMany(names, times, prices);
                var route = new Route(BCP.CountryName, dow, carrier, Mode.Bus, stops, url);
                var legs = await this.routeParser.ParseRouteAsync(route);

                all.AddRange(legs);
            }

            return all;
        }
    }
}
