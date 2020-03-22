namespace Navred.Core.Places
{
    public interface IPlaceGeneratorFactory
    {
        IPlaceGenerator CreateGenerator(string country);
    }
}