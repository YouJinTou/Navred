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

        public Place GetPlace(string id)
        {
            var place = (Place)id;

            return this.GetPlace(
                place.Country, place.Name, place.Region, place.Municipality, doFuzzyMatch: false);
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
            IEnumerable<string> neighbors = null,
            bool throwOnFail = true,
            bool doFuzzyMatch = true)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(country, name);

            var places = this.LoadPlacesFor(country);
            var normalizedName = this.FormatPlace(name);
            var results = places
                .Where(p => normalizedName.Contains(this.FormatPlace(p.Name)))
                .ToList();
            regionCode = string.IsNullOrWhiteSpace(regionCode) ? 
                this.TryGetRegionFromNeighbors(neighbors, country) : regionCode;
            var result = this.GetPlace(results, regionCode, municipalityCode);

            if (result == null)
            {
                results = places
                    .Where(p => this.FormatPlace(p.Name).Contains(normalizedName))
                    .ToList();
                result = this.GetPlace(results, regionCode, municipalityCode);
            }

            result = (result == null) ? 
                doFuzzyMatch ? this.DoFuzzyMatch(places, normalizedName) : null :
                result;

            if (result != null)
            {
                return result;
            }

            if (throwOnFail)
            {
                throw new ArgumentException($"Could not find a match for {country}/{name}.");
            }

            return null;
        }

        public IDictionary<string, Place> DeducePlacesFromStops(string country, IList<string> stops)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(country, stops);

            var placesByStop = stops.ToDictionary(
                kvp => kvp, kvp => this.GetPlace(country, kvp, throwOnFail: false));

            Validator.ThrowIfAllNull(placesByStop.Values, "Unresolvable itinerary.");

            if (stops.Count <= 2 && placesByStop.Any(p => p.Value == null))
            {
                throw new InvalidOperationException("Cannot resolve from two stops only.");
            }

            while (Validator.AnyNull(placesByStop.Values))
            {
                for (int s = 1; s < stops.Count - 1; s++)
                {
                    var prevValue = stops[s - 1];
                    var nextValue = stops[s + 1];
                    var previous = placesByStop[prevValue];
                    var current = placesByStop[stops[s]];
                    var next = placesByStop[nextValue];

                    if (current == null)
                    {
                        continue;
                    }

                    if (previous == null)
                    {
                        var places = this.GetPlaces(country, prevValue);
                        var closestPlace = places.GetMin(p => current.DistanceToInKm(p));
                        placesByStop[prevValue] = closestPlace;
                    }

                    if (next == null)
                    {
                        var places = this.GetPlaces(country, nextValue);
                        var closestPlace = places.GetMin(p => current.DistanceToInKm(p));
                        placesByStop[nextValue] = closestPlace;
                    }
                }
            }

            return placesByStop;
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

        private Place DoFuzzyMatch(IEnumerable<Place> places, string normalizedPlace)
        {
            var separators = new string[] { ".", "-", " " };

            foreach (var separator in separators)
            {
                var tokens = normalizedPlace.Split(separator);

                if (tokens.Length <= 1)
                {
                    continue;
                }

                foreach (var p in places)
                {
                    if (p.Name.IsFuzzyMatch(normalizedPlace))
                    {
                        return p;
                    }
                }
            }

            if (normalizedPlace.Split(" ").Length > 1)
            {
                foreach (var p in places)
                {
                    if (p.Name.Replace(" ", "").ToLower() == normalizedPlace.Replace(" ", ""))
                    {
                        return p;
                    }
                }
            }

            return default;
        }

        private string TryGetRegionFromNeighbors(
           IEnumerable<string> neighbors, string country)
        {
            if (neighbors.IsNullOrEmpty())
            {
                return null;
            }
            
            var regions = new List<string>();
            var allFound = true;

            foreach (var neighbor in neighbors)
            {
                try
                {
                    var neighborPlace = this.GetPlace(country, neighbor, doFuzzyMatch: false);

                    regions.Add(neighborPlace.Region);
                }
                catch
                {
                    allFound = false;
                }
            }

            var areInSameRegion = regions.Distinct().ContainsOne();

            return allFound && areInSameRegion ? regions.First() : null;
        }
    }
}
