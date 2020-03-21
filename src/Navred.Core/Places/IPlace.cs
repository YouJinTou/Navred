namespace Navred.Core.Places
{
    public interface IPlace
    {
        string Name { get; set; }

        string RegionCode { get; set; }

        float? Longitude { get; set; }

        float? Latitude { get; set; }
    }
}
