
using Amazon.Lambda.TestUtilities;
using Xunit;

namespace Navred.Providers.Bulgaria.Boydevi.Tests
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
