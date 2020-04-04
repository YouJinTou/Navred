
using Amazon.Lambda.Core;
using Moq;
using Navred.Core.Abstractions;
using System;
using Xunit;

namespace Navred.Crawling.Tests
{
    public class FunctionTest
    {
        private const string Input = "Test string.";

        private readonly Mock<ICrawler> crawler;
        private readonly Mock<ILambdaContext> context;
        private readonly Mock<ILambdaLogger> logger;

        public FunctionTest()
        {
            this.crawler = new Mock<ICrawler>();
            this.context = new Mock<ILambdaContext>();
            this.logger = new Mock<ILambdaLogger>();

            this.context.Setup(c => c.Logger).Returns(this.logger.Object);

            Function.SetCrawler(this.crawler.Object);
        }

        [Fact]
        public void RunsCrawler()
        {
            Function.HandleRequest(Input, this.context.Object);

            this.crawler.Verify(c => c.UpdateLegsAsync(), Times.Once);
        }

        [Fact]
        public void LogsException()
        {
            this.crawler.Setup(c => c.UpdateLegsAsync()).ThrowsAsync(new Exception(Input));

            Function.HandleRequest(Input, this.context.Object);

            this.logger.Verify(l => l.Log(It.Is<string>(e => e.Contains(Input))), Times.Once);
        }
    }
}
