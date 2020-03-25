
using Amazon.Lambda.TestUtilities;
using Xunit;

namespace Navred.Providers.Bulgaria.VelikoTarnovoSouthBusStation.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void RunsCrawler()
        {
            var context = new TestLambdaContext();
            
            Function.FunctionHandler("hello world", context);
        }
    }
}
