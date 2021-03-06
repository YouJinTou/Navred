﻿using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using Navred.Core.Tools;
using System;

namespace Navred.Core.Places
{
    public class Place : IEquatable<Place>, ICopyable<Place>
    {
        public string Country { get; set; }

        public string Name { get; set; }

        public string Region { get; set; }

        public string Municipality { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public static implicit operator Place(string id)
        {
            id.ThrowIfNullOrWhiteSpace("Cannot create a place from an empty ID.");

            var tokens = id.Split('|');
            var place = new Place
            {
                Country = tokens[0],
                Name = tokens[1],
                Region = string.IsNullOrWhiteSpace(tokens[2]) ? null : tokens[2],
                Municipality = string.IsNullOrWhiteSpace(tokens[3]) ? null : tokens[3]
            };

            return place;
        }

        public static implicit operator string(Place place)
        {
            place.ThrowIfNull("Place is empty.");

            return place.GetId();
        }

        public double DistanceToInKm(Place other)
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

        public string GetId()
        {
            return
                $"{this.Country}|" +
                $"{this.Name}|" +
                (string.IsNullOrWhiteSpace(this.Region) ? string.Empty : $"{this.Region}|") +
                (string.IsNullOrWhiteSpace(this.Municipality) ? string.Empty : this.Municipality);
        }

        public override string ToString() => this.GetId().FormatId();

        public bool Equals(Place other)
        {
            if (other == null)
            {
                return false;
            }

            return
                this.Country.Equals(other.Country) &&
                this.Name.Equals(other.Name) &&
                this.Region.Equals(other.Region) &&
                this.Municipality.Equals(other.Municipality);
        }

        public Place Copy()
        {
            return new Place
            {
                Country = this.Country,
                Latitude = this.Latitude,
                Longitude = this.Longitude,
                Municipality = this.Municipality,
                Name = this.Name,
                Region = this.Region
            };
        }
    }
}
