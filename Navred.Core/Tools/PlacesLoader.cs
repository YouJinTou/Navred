using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Navred.Core.Tools
{
    public static class PlacesLoader
    {
        private static IDictionary<string, object> cache = new Dictionary<string, object>();

        public static IEnumerable<T> LoadPlacesFor<T>(string country)
        {
            Validator.ThrowIfNullOrWhiteSpace(country);

            if (cache.ContainsKey(country))
            {
                return (IEnumerable<T>)cache[country];
            }

            var places = File.ReadAllText($"Resources/{country.ToLower()}_places.json");
            var models = JsonConvert.DeserializeObject<IEnumerable<T>>(places);
            cache[country] = models;

            return models;
        }
    }
}
