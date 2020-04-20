using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Models;
using Navred.Core.Places;
using Navred.Core.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers.Regions
{
    public class SofiaCentralBusStation : ICrawler
    {
        private const string DeparturesUrl =
            "https://www.centralnaavtogara.bg/index.php?mod=0461ebd2b773878eac9f78a891912d65";
        private const string ArrivalsUrl =
            "https://www.centralnaavtogara.bg/index.php?mod=06a943c59f33a34bb5924aaf72cd2995&d=c#b";

        private readonly IPlacesManager placesManager;
        private readonly IRouteParser routeParser;
        private readonly ILegRepository repo;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ICultureProvider cultureProvider;
        private readonly ILogger<SofiaCentralBusStation> logger;
        private readonly IDictionary<string, string> replacements;
        private readonly IDictionary<string, string> regions;

        public SofiaCentralBusStation(
            IPlacesManager placesManager,
            IRouteParser routeParser,
            ILegRepository repo,
            IHttpClientFactory httpClientFactory,
            ICultureProvider cultureProvider,
            ILogger<SofiaCentralBusStation> logger)
        {
            this.placesManager = placesManager;
            this.routeParser = routeParser;
            this.repo = repo;
            this.httpClientFactory = httpClientFactory;
            this.cultureProvider = cultureProvider;
            this.logger = logger;
            this.replacements = new Dictionary<string, string>
            {
                { "БЕЛИ МЕЛ", "Белимел" },
                { "В.ТЪРНОВО", "Велико Търново" },
                { "Г.ДЕЛЧЕВ", "Гоце Делчев" },
                { "ГЕН.ТОШЕВО", "Генерал Тошево" },
                { "Д.ЦЕРОВЕНЕ", "Долно Церовене" },
                { "СТ.ЗАГОРА", "Стара Загора" },
                { "СЛ.БРЯГ", "Слънчев бряг" },
                { "СВ.ВЛАС", "Свети Влас" },
                { "ЗЛ.ПЯСЪЦИ", "Златни пясъци" },
                { "ЛЮТИ ДОЛ", "Лютидол" },
                { "НАР.БАНИ", "Нареченски бани" },
                { "Г.ГЕНОВО", "Гаврил Геново" },
                { "Г.ДАМЯНОВО", "Георги Дамяново" },
                { "Н.КОНОМЛАДИ", "Ново Кономлади" },
                { "Д.ВЕРЕНИЦА", "Долна Вереница" },
                { "Д.ЦИБЪР", "Долни Цибър" },
                { "Г.ВЕРЕНИЦА", "Горна Вереница" },
                { "Г.КОВАЧИЦА", "Горна Ковачица" },
                { "БЯЛА/РУСЕ", "Бяла" },
                { "БЯЛА/ВАРНА", "Бяла" },
                { "с.РАЗГРАД", "РАЗГРАД" },
            };
            this.regions = new Dictionary<string, string>
            {
                { "ГАБРОВО", BCP.Region.GAB },
                { "ДОБРИЧ", BCP.Region.DOB },
                { "МОКРЕШ", BCP.Region.MON },
                { "ПЕТРИЧ", BCP.Region.BLG },
                { "ТЪРГОВИЩЕ", BCP.Region.TGV },
                { "ЧЕРНИ ВРЪХ", BCP.Region.MON },
                { "ГЕН.ТОШЕВО", BCP.Region.DOB },
                { "АСПАРУХОВО", BCP.Region.MON },
                { "АПРИЛЦИ", BCP.Region.LOV },
                { "БЕЖАНОВО", BCP.Region.LOV },
                { "БЕНКОВСКИ", BCP.Region.KRZ },
                { "БОРОВИЦА", BCP.Region.VID },
                { "БЯЛА/РУСЕ", BCP.Region.RSE },
                { "БЯЛА/ВАРНА", BCP.Region.VAR },
                { "ПОПОВО", BCP.Region.TGV },
                { "РАЗГРАД", BCP.Region.RAZ },
                { "с.РАЗГРАД", BCP.Region.MON },
            };
            Console.OutputEncoding = this.cultureProvider.GetEncoding();
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var departures = await this.UpdateAsync(DeparturesUrl);
                var arrivals = await this.UpdateAsync(ArrivalsUrl);
                var all = new List<Leg>(departures);

                all.AddRange(arrivals);

                await this.repo.UpdateLegsAsync(all);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Update failed.");
            }
        }

        private async Task<IEnumerable<Leg>> UpdateAsync(string url)
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
            var dates = DateTime.UtcNow.GetDateTimesAhead(Constants.CrawlLookaheadDays)
                .Select(dt => dt.ToString("dd.MM.yyyy")).ToList();
            var legs = new List<Leg>();

            foreach (var date in dates)
            {
                foreach (var d in destinations)
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
                }
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
            }

            return legs;
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
            var formattedName = this.placesManager.FormatPlace(placeString);

            foreach (var table in tables)
            {
                var dataRow = table.FirstChild;
                var resultDays = dataRow.SelectNodes("//li[@class='rd_green']//text()")
                    ?.Select(n => n.InnerText)?.ToList() ?? new List<string>();
                var dow = this.cultureProvider.ToDaysOfWeek(resultDays).IsEmpty() ?
                    Constants.AllWeek : this.cultureProvider.ToDaysOfWeek(resultDays);
                var carrier = dataRow.ChildNodes[1].InnerText;
                var timeString =
                    Regex.Match(dataRow.ChildNodes[3].InnerText, @"(\d+:\d+)").Groups[1].Value;
                var fromTo = new Dictionary<string, string>
                {
                    { BCP.City.Sofia, BCP.Region.SOF },
                    { 
                        this.replacements.GetOrReturn(placeString), 
                        this.regions.GetOrDefault(placeString) 
                    }
                };
                var fullRoute = table.SelectSingleNode(
                    ".//td[contains(@class, 'sr_full_route')]")?.InnerText
                    ?.Split("-", StringSplitOptions.RemoveEmptyEntries)
                    ?.ToDictionary(
                        kvp => this.replacements.GetOrReturn(kvp.Trim()),
                        kvp => this.regions.GetOrDefault(kvp.Trim()));
                var names = isDeparture ?
                    fullRoute.IsNull() ? fromTo : fullRoute :
                    fullRoute.IsNull() ? fromTo.ReverseDict() : fullRoute.ReverseDict();
                var times = isDeparture ?
                    new[] { timeString, null }.InsertBetween(null, names.Count) :
                    new[] { null, timeString }.InsertBetween(null, names.Count);
                var prices = names.Select(
                    n => this.placesManager.FormatPlace(n.Key).Equals(formattedName) ? 
                        dataRow.ChildNodes[5].InnerText : null).ToList();
                var stops = Stop.CreateMany(names.Keys, times, prices, regions: names.Values);
                var route = new Route(BCP.CountryName, dow, carrier, Mode.Bus, stops, url);
                var legs = await this.routeParser.ParseRouteAsync(route);

                all.AddRange(legs);
            }

            return all;
        }
    }
}
