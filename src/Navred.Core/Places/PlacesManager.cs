using Navred.Core.Configuration;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Models;
using Navred.Core.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Navred.Core.Places
{
    public class PlacesManager : IPlacesManager
    {
        private readonly IDictionary<string, IEnumerable<Place>> cache;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IPlaceGeneratorFactory placeGeneratorFactory;
        private readonly ICultureProvider cultureProvider;
        private readonly Settings settings;

        public PlacesManager(
            IHttpClientFactory httpClientFactory,
            IPlaceGeneratorFactory placeGeneratorFactory,
            ICultureProvider cultureProvider,
            Settings settings)
        {
            this.cache = new Dictionary<string, IEnumerable<Place>>();
            this.httpClientFactory = httpClientFactory;
            this.placeGeneratorFactory = placeGeneratorFactory;
            this.cultureProvider = cultureProvider;
            this.settings = settings;
        }

        public void GeneratePlacesFor(string country)
        {
            var generator = this.placeGeneratorFactory.CreateGenerator(country);

            generator.GeneratePlaces();
        }

        public IEnumerable<Place> LoadPlacesFor(string country)
        {
            Validator.ThrowIfNullOrWhiteSpace(country);

            if (this.cache.ContainsKey(country))
            {
                return this.cache[country];
            }

            var path = $"{country.ToLower()}_places.json".GetFirstFilePathMatch();
            var places = File.ReadAllText(path);
            var models = JsonConvert.DeserializeObject<IEnumerable<Place>>(places);
            this.cache[country] = models;

            return models;
        }

        public void UpdatePlacesFor(string country, IEnumerable<Place> places)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(country, places);

            var placesString = JsonConvert.SerializeObject(places);

            File.WriteAllText($"Resources/{country.ToLower()}_places.json", placesString);
        }

        public async Task<IEnumerable<Place>> UpdateCoordinatesForCountryAsync(
            string country)
        {
            Validator.ThrowIfNullOrWhiteSpace(country);

            var places = this.LoadPlacesFor(country);
            var client = httpClientFactory.CreateClient();
            var notUpdated = new List<Place>();

            foreach (var place in places)
            {
                if (place.Latitude.HasValue && place.Longitude.HasValue)
                {
                    continue;
                }

                var regionMunicipality = place.Region.Equals(place.Municipality) ?
                    place.Region : $"{place.Region}, {place.Municipality}";
                var url = this.settings.BuildGeocodingUrl(
                    $"{country}, {regionMunicipality}, {place.Name}");
                var result = await client.GetAsync(url);
                var resultString = await result.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<GeocodingResult>(resultString);

                await Task.Delay(50);

                if (!model.status.Equals("OK"))
                {
                    notUpdated.Add(place);

                    continue;
                }

                if (model.results.Length > 1)
                {
                    notUpdated.Add(place);

                    continue;
                }

                var first = model.results.First();
                place.Latitude = first.geometry.location.lat;
                place.Longitude = first.geometry.location.lng;
            }

            this.UpdatePlacesFor(country, places);

            return notUpdated;
        }

        public string FormatPlace(string place)
        {
            var formattedPlace = place
                .Replace(".", " ")
                .Replace("-", " ");
            var pattern = $@"[^{this.cultureProvider.Letters}\s]";
            formattedPlace = Regex.Replace(formattedPlace, pattern, "").Trim().ToLower();

            return formattedPlace;
        }

        public IEnumerable<Place> GetPlaces(string country, string name)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(country, name);

            var places = this.LoadPlacesFor(country);
            var normalizedName = this.FormatPlace(name);
            var results = places
                .Where(p => normalizedName.Equals(this.FormatPlace(p.Name)))
                .ToList();

            return results;
        }

        public Place GetPlace(
            string country,
            string name,
            string regionCode = null,
            string municipalityCode = null,
            bool throwOnFail = true,
            bool doFuzzyMatch = false)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(country, name);

            var places = this.LoadPlacesFor(country);
            var normalizedName = this.FormatPlace(name);
            var results = places
                .Where(p => normalizedName.Equals(this.FormatPlace(p.Name)))
                .ToList();
            var result = this.GetPlace(results, regionCode, municipalityCode);

            if (result == null && doFuzzyMatch)
            {
                var fuzzyResults = places
                    .Where(p => normalizedName.IsFuzzyMatch(this.FormatPlace(p.Name)))
                    .ToList();
                result = this.GetPlace(fuzzyResults, regionCode, municipalityCode);
            }

            if (result == null && throwOnFail)
            {
                throw new ArgumentException($"Could not find a match for {country}/{name}.");
            }

            return result;
        }

        public IEnumerable<Stop> DeducePlacesFromStops(
            string country, IList<Stop> stops, bool throwOnUnresolvable = true)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(country, stops);

            var uniqueStops = stops.Select(s =>
            {
                s.Name = $"{s.Name}_{Guid.NewGuid()}";

                return s;
            }).ToList();
            var stopsByName = uniqueStops.ToDictionary(s => s.Name, s => new Stop(
                s.Name,
                s.Region,
                s.Municipality,
                s.Time,
                s.Address,
                s.Price,
                this.GetPlace(country, s.Name, s.Region, s.Municipality, false)));

            Validator.ThrowIfAllNull(stopsByName.Values, "Unresolvable itinerary.");

            if (stops.Count <= 2 && stopsByName.Values.Any(s => s == null))
            {
                throw new InvalidOperationException("Cannot resolve from two stops only.");
            }

            var maxIterations = 5;
            var iteration = 0;

            while (iteration < maxIterations)
            {
                for (int s = 1; s < stops.Count - 1; s++)
                {
                    var prevValue = uniqueStops[s - 1].Name;
                    var currentValue = uniqueStops[s].Name;
                    var nextValue = uniqueStops[s + 1].Name;
                    var previous = stopsByName[prevValue].Place;
                    var current = stopsByName[currentValue].Place;
                    var next = stopsByName[nextValue].Place;

                    if (current == null)
                    {
                        if (previous != null)
                        {
                            this.TrySetClosest(stopsByName, country, currentValue, previous);
                        }

                        continue;
                    }

                    if (previous == null)
                    {
                        this.TrySetClosest(stopsByName, country, prevValue, current);
                    }

                    if (next == null)
                    {
                        this.TrySetClosest(stopsByName, country, nextValue, current);
                    }
                }

                iteration++;
            }

            if (throwOnUnresolvable && Validator.AnyNull(stopsByName.Values))
            {
                throw new Exception($"Unresolvable route: {string.Join(',', stops)}");
            }

            var result = stopsByName.Values.Select(s =>
            {
                s.Name = s.Name.Split('_').First();

                return s;
            }).ToList();

            return result;
        }

        private void TrySetClosest(
            IDictionary<string, Stop> stopsByName, string country, string name, Place current)
        {
            if (current == null)
            {
                return;
            }

            var places = this.GetPlaces(country, name);

            if (places.Any())
            {
                var closestPlace = places.GetMin(p => current.DistanceToInKm(p));
                stopsByName[name].Place = closestPlace;
            }
        }

        private Place GetPlace(
            IEnumerable<Place> results, string regionCode, string municipalityCode)
        {
            if (results.ContainsOne())
            {
                return results.First();
            }

            var result = default(Place);

            if (results.Count() > 1)
            {
                result = results.FirstOrDefault(r =>
                    r.Region == regionCode &&
                    string.IsNullOrWhiteSpace(municipalityCode) ?
                    true : r.Municipality == municipalityCode);
            }

            return result;
        }
    }
}
