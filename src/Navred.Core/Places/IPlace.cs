namespace Navred.Core.Places
{
    public interface IPlace
    {
        string Country { get; set; }

        string Name { get; set; }

        string Region { get; set; }

        string Municipality { get; set; }

        double? Longitude { get; set; }

        double? Latitude { get; set; }

        double DistanceToInKm(IPlace other);
    }
}
