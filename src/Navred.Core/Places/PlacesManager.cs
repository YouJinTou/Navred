using Navred.Core.Configuration;
using Navred.Core.Models;
using Navred.Core.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Navred.Core.Places
{
    public class PlacesManager : IPlacesManager
    {
        private readonly IDictionary<string, object> cache;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly Settings settings;

        public PlacesManager(IHttpClientFactory httpClientFactory, Settings settings)
        {
            this.cache = new Dictionary<string, object>();
            this.httpClientFactory = httpClientFactory;
            this.settings = settings;
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

                var url = this.settings.BuildGeocodingUrl(
                    $"{country}, {place.RegionCode}, {place.Name}");
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

        public T GetPlace<T>(
            string country, string name, string regionCode = null) where T : IPlace
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(country, name);

            var places = this.LoadPlacesFor<T>(country);
            var results = places.Where(p => p.Name == name).ToList();

            if (results.Count == 1)
            {
                return results.First();
            }

            var result = default(T);

            if (results.Count > 1)
            {
                result = results.FirstOrDefault(r => r.RegionCode == regionCode);
            }

            return (result == null) ?
                throw new ArgumentException($"Could not find a match for {country}/{name}.") :
                result;
        }
    }
}
