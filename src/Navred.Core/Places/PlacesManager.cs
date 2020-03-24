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
        private readonly IDictionary<string, object> cache;
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
            this.cache = new Dictionary<string, object>();
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

        public IEnumerable<T> LoadPlacesFor<T>(string country) where T : IPlace
        {
            Validator.ThrowIfNullOrWhiteSpace(country);

            if (this.cache.ContainsKey(country))
            {
                return (IEnumerable<T>)this.cache[country];
            }

            var places = File.ReadAllText($"Resources/{country.ToLower()}_places.json");
            var models = JsonConvert.DeserializeObject<IEnumerable<T>>(places);
            this.cache[country] = models;

            return models;
        }

        public void UpdatePlacesFor<T>(string country, IEnumerable<T> places) where T : IPlace
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(country, places);

            var placesString = JsonConvert.SerializeObject(places);

            File.WriteAllText($"Resources/{country.ToLower()}_places.json", placesString);
        }

        public async Task<IEnumerable<T>> UpdateCoordinatesForCountryAsync<T>(
            string country) where T : IPlace
        {
            Validator.ThrowIfNullOrWhiteSpace(country);

            var places = this.LoadPlacesFor<T>(country);
            var client = httpClientFactory.CreateClient();
            var notUpdated = new List<T>();

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

        public T GetPlace<T>(
            string country, 
            string name, 
            string regionCode = null, 
            string municipalityCode = null,
            IEnumerable<string> neighbors = null) where T : IPlace
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(country, name);

            var places = this.LoadPlacesFor<T>(country);
            var normalizedName = this.FormatPlace(name);
            var results = places
                .Where(p => normalizedName.Contains(this.FormatPlace(p.Name)))
                .ToList();
            regionCode = string.IsNullOrWhiteSpace(regionCode) ? 
                this.TryGetRegionFromNeighbors<T>(neighbors, country) : regionCode;
            var result = this.GetPlace(results, regionCode, municipalityCode);

            if (result == null)
            {
                results = places
                    .Where(p => this.FormatPlace(p.Name).Contains(normalizedName))
                    .ToList();
                result = this.GetPlace(results, regionCode, municipalityCode);
            }

            result = (result == null) ? this.DoFuzzyMatch(places, normalizedName) : result;

            return (result == null) ?
                throw new ArgumentException($"Could not find a match for {country}/{name}.") :
                result;
        }

        public string NormalizePlaceName<T>(
            string country, 
            string name, 
            string regionCode = null, 
            string municipalityCode = null,
            IEnumerable<string> neighbors = null) where T : IPlace
        {
            var place = this.GetPlace<T>(country, name, regionCode, municipalityCode, neighbors);

            return place.Name;
        }

        private T GetPlace<T>(
            IEnumerable<T> results, string regionCode, string municipalityCode) where T : IPlace
        {
            if (results.ContainsOne())
            {
                return results.First();
            }

            var result = default(T);

            if (results.Count() > 1)
            {
                result = results.FirstOrDefault(r =>
                    r.Region == regionCode &&
                    string.IsNullOrWhiteSpace(municipalityCode) ?
                    true : r.Municipality == municipalityCode);
            }

            return result;
        }

        private T DoFuzzyMatch<T>(IEnumerable<T> places, string normalizedPlace) where T : IPlace
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

        private string TryGetRegionFromNeighbors<T>(
           IEnumerable<string> neighbors, string country) where T : IPlace
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
                    var neighborPlace = this.GetPlace<T>(country, neighbor);

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
