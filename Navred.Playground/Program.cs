using Navred.Core.Cultures;

namespace Navred.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            BulgarianCultureProvider provider = new BulgarianCultureProvider();

            var x = provider.NormalizePlaceName("абланица");
        }
    }
}
