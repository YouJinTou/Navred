using Navred.Core.Extensions;
using Navred.Core.Tools;
using System;

namespace Navred.Core.Places
{
    public class BulgarianPlace : IPlace
    {
        public string Name { get; set; }

        public string RegionCode { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public double DistanceToInKm(IPlace other)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(
                other, other?.Latitude, other?.Longitude, this.Longitude, this.Latitude);

            var lat = (other.Latitude - this.Latitude).Value.ToRadians();
            var lng = (other.Longitude - this.Longitude).Value.ToRadians();
            var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                Math.Cos(this.Latitude.Value.ToRadians()) * Math.Cos(other.Latitude.Value.ToRadians()) *
                Math.Sin(lng / 2) * Math.Sin(lng / 2);
            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
            var result = Constants.EarthRadiusInKm * h2;

            return result;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
