using System.Text;

namespace Navred.Core.Cultures
{
    public interface ICultureProvider
    {
        string Name { get; }

        string Letters { get; }

        string Latinize(string s);

        Encoding GetEncoding();
    }
}
