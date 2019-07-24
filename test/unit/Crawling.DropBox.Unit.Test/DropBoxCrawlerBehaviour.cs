using System;
using System.Threading.Tasks;
using CluedIn.Core.Agent.Jobs;
using CluedIn.Core.Crawling;
using CluedIn.Core.Providers;
using CluedIn.Crawling;
using CluedIn.Crawling.DropBox.Core;
using CluedIn.Crawling.DropBox.Infrastructure;
using CluedIn.Crawling.DropBox.Infrastructure.Factories;
using CluedIn.Crawling.DropBox.Test.Common;
using Crawling.DropBox.Unit.Test.Stubs;
using Dropbox.Api.Users;
using Moq;
using Xunit;
using ILogger = CluedIn.Core.Logging.ILogger;

namespace Crawling.DropBox.Unit.Test
{
  public class DropBoxCrawlerBehaviour
  {
        public class ConstructorTests
        {
            [Fact]
            public void ConstructorRequiresClientFactoryParameter()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new CluedIn.Crawling.DropBox.DropBoxCrawler(default(IDropBoxClientFactory), default(ILogger), default(IAgentJobProcessorState<CrawlJobData>)));
            }
        }

        public class GetDataTests
        {
            private readonly ICrawlerDataGenerator _sut;
            private readonly DropBoxCrawlJobData _crawlJobData;
            private readonly Mock<IDropBoxClient> _clientMock;
            private readonly Mock<ILogger> _logMock;

            public GetDataTests()
            {
                var nameClientFactory = new Mock<IDropBoxClientFactory>();
                var stateMock = new Mock<IAgentJobProcessorState<CrawlJobData>>();

                _clientMock = new Mock<IDropBoxClient>();

                _clientMock
                    .Setup(x => x.GetCurrentAccountAsync())
                    .Returns(Task.FromResult(new FullAccount()));

                _logMock = new Mock<ILogger>();

                nameClientFactory.Setup(x => x.CreateNew(It.IsAny<DropBoxCrawlJobData>())).Returns(_clientMock.Object);

                _sut = new CluedIn.Crawling.DropBox.DropBoxCrawler(nameClientFactory.Object, _logMock.Object, stateMock.Object);

                _crawlJobData = new DropBoxCrawlJobData(DropBoxConfiguration.Create());
            }

            [Fact]
            public void RequiresCrawlJobDataParameter()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    _sut.GetData(default(CrawlJobData)));
            }

            [Theory]
            [InlineData(typeof(CrawlJobData))]
            [InlineData(typeof(SomeOtherCrawlJobData))]
            public void ReturnsEmptyDataForNonDropBoxCrawlJobData(Type jobDataType)
            {
                var instance = Activator.CreateInstance(jobDataType);

                Assert.Empty(
                    _sut.GetData((CrawlJobData)instance));
            }

            [Fact]
            public void ReturnsEmptyDataWhenAccountInfoAreNotAvailable()
            {
                _clientMock
                    .Setup(x => x.GetCurrentAccountAsync())
                    .Returns(Task.FromResult(default(FullAccount)));

                Assert.Empty(
                    _sut.GetData(_crawlJobData));
            }
        }
    }
}
