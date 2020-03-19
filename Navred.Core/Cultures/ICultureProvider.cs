namespace Navred.Core.Cultures
{
    public interface ICultureProvider
    {
        string Name { get; }

        string Latinize(string s);

        string NormalizePlaceName(string place, string discerningCode = null);
    }
}
