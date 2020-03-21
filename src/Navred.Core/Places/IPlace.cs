namespace Navred.Core.Places
{
    public interface IPlace
    {
        string Name { get; set; }

        string RegionCode { get; set; }

        double? Longitude { get; set; }

        double? Latitude { get; set; }

        double DistanceToInKm(IPlace other);
    }
}
