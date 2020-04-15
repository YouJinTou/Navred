using Navred.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Places
{
    public interface IPlacesManager
    {
        void GeneratePlacesFor(string country);

        IEnumerable<Place> LoadPlacesFor(string country);

        Task<IEnumerable<Place>> UpdateCoordinatesForCountryAsync(string country);

        void UpdatePlacesFor(string country, IEnumerable<Place> places);

        string FormatPlace(string place);

        IEnumerable<Place> GetPlaces(string country, string name);

        Place GetPlace(
            string country, 
            string name, 
            string regionCode = null, 
            string municipalityCode = null,
            bool throwOnFail = true,
            bool doFuzzyMatch = false);

        IEnumerable<Stop> DeducePlacesFromStops(
            string country, IList<Stop> stops, bool throwOnUnresolvable = true);
    }
}