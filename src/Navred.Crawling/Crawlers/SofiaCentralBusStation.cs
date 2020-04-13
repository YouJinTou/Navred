using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
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

        private readonly ILegRepository repo;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IPlacesManager placesManager;
        private readonly ITimeEstimator estimator;
        private readonly ICultureProvider cultureProvider;
        private readonly ILogger<SofiaCentralBusStation> logger;
        private readonly Place sofia;
        private readonly IDictionary<string, string> replacements;

        public SofiaCentralBusStation(
            ILegRepository repo,
            IHttpClientFactory httpClientFactory,
            IPlacesManager placesManager,
            ITimeEstimator estimator,
            ICultureProvider cultureProvider,
            ILogger<SofiaCentralBusStation> logger)
        {
            this.repo = repo;
            this.httpClientFactory = httpClientFactory;
            this.placesManager = placesManager;
            this.estimator = estimator;
            this.cultureProvider = cultureProvider;
            this.logger = logger;
            this.sofia = this.placesManager.GetPlace(BCP.CountryName, BCP.City.Sofia);
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
            var isDeparture = url.Equals(DeparturesUrl);

            foreach (var table in tables)
            {
                var dataRow = table.FirstChild;

                if (!this.IsValidDate(date, dataRow))
                {
                    continue;
                }

                var place = this.ResolvePlace(table, placeString);
                var from = isDeparture ? sofia : place;
                var to = isDeparture ? place : sofia;
                var carrier = dataRow.ChildNodes[1].InnerText;
                var departureTimeString =
                    Regex.Match(dataRow.ChildNodes[3].InnerText, @"(\d+:\d+)").Groups[1].Value;
                var departure = date + TimeSpan.Parse(departureTimeString);
                var arrival = await this.estimator.EstimateArrivalTimeAsync(
                    from, to, departure, Mode.Bus);
                var priceString = dataRow.ChildNodes[5].InnerText;
                var price = priceString.StripCurrency();
                var leg = new Leg(
                    from,
                    to,
                    departure.ToUtcDateTime(Constants.BulgariaTimeZone),
                    arrival.ToUtcDateTime(Constants.BulgariaTimeZone),
                    carrier,
                    Mode.Bus,
                    url,
                    price,
                    arrivalEstimated: true);

                legs.Add(leg);
            }

            return legs;
        }

        private bool IsValidDate(DateTime date, HtmlNode dataRow)
        {
            var resultDays = dataRow.SelectNodes("//li[@class='rd_green']//text()")
                ?.Select(n => n.InnerText)?.ToList() ?? new List<string>();

            if (resultDays.IsNullOrEmpty())
            {
                this.logger.LogWarning($"Could not validate date: {dataRow.InnerText}");

                return true;
            }

            return date.DayOfWeek switch
            {
                DayOfWeek.Sunday => resultDays.Contains("нд"),
                DayOfWeek.Monday => resultDays.Contains("пн"),
                DayOfWeek.Tuesday => resultDays.Contains("вт"),
                DayOfWeek.Wednesday => resultDays.Contains("ср"),
                DayOfWeek.Thursday => resultDays.Contains("чт"),
                DayOfWeek.Friday => resultDays.Contains("пк"),
                DayOfWeek.Saturday => resultDays.Contains("сб"),
                _ => true,
            };
        }

        private string GetRegionCode(string place)
        {
            place = place.ToLower().Trim();
            var codeByPlace = new Dictionary<string, string>
            {
                { "абланица", BCP.Region.LOV },
                { "добрич", BCP.Region.DOB },
                { "априлци", BCP.Region.LOV },
                { "габрово", BCP.Region.GAB },
                { "падина", BCP.Region.KRZ },
                { "оряхово", BCP.Region.VRC },
                { "батак", BCP.Region.PAZ },
                { "копиловци", BCP.Region.MON },
                { "искър", BCP.Region.PVN },
                { "огняново", BCP.Region.BLG },
                { "разград", BCP.Region.RAZ },
                { "елхово", BCP.Region.JAM },
                { "петрич", BCP.Region.BLG },
                { "рибарица", BCP.Region.LOV },
                { "троян", BCP.Region.LOV },
                { "бенковски", BCP.Region.KRZ },
            };

            return codeByPlace.ContainsKey(place) ? codeByPlace[place] : null;
        }

        private string GetMunicipalityCode(string place)
        {
            place = place.ToLower().Trim();
            var codeByPlace = new Dictionary<string, string>
            {
                { "искър", "Искър" }
            };

            return codeByPlace.ContainsKey(place) ? codeByPlace[place] : null;
        }

        private Place ResolvePlace(HtmlNode table, string placeString)
        {
            var formattedPlace = this.placesManager.FormatPlace(placeString);
            formattedPlace = this.replacements.ContainsKey(formattedPlace) ? 
                this.replacements[formattedPlace] : formattedPlace;
            var place = this.placesManager.GetPlace(
                BCP.CountryName,
                formattedPlace,
                this.GetRegionCode(formattedPlace),
                this.GetMunicipalityCode(formattedPlace),
                throwOnFail: false);

            if (place != null)
            {
                return place;
            }

            var routeTd = table.Descendants("td").FirstOrDefault(
                n => n.HasClass("sr_full_route"));

            if (routeTd == null)
            {
                throw new Exception($"Unresolvable route for {placeString}");
            }

            var route = routeTd.InnerText;
            var stops = route.Split('-').Select(s => this.placesManager.FormatPlace(s)).ToList();
            var placesByName = this.placesManager.DeducePlacesFromStops(
                BCP.CountryName, stops, false);

            if (placesByName[formattedPlace] == null)
            {
                throw new Exception($"Unresolvable route for {placeString}");
            }

            return placesByName[formattedPlace];
        }
    }
}
