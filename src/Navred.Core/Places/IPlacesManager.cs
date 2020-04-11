﻿using System.Collections.Generic;
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

        Place GetPlace(string id);

        IEnumerable<Place> GetPlaces(string country, string name);

        Place GetPlace(
            string country, 
            string name, 
            string regionCode = null, 
            string municipalityCode = null,
            IEnumerable<string> neighbors = null,
            bool throwOnFail = true,
            bool useExactMatching = true,
            bool fallbackToFuzzyMatch = true);

        IDictionary<string, Place> DeducePlacesFromStops(
            string country, IList<string> stops, bool throwOnUnresolvable = true);
    }
}