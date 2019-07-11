using CluedIn.Core.Crawling;
using CluedIn.Core.Logging;
using CluedIn.Crawling;
using CluedIn.Crawling.DropBox;
using CluedIn.Crawling.DropBox.Infrastructure.Factories;
using Moq;
using Should;
using Xunit;

namespace Crawling.DropBox.Test
{
  public class DropBoxCrawlerBehaviour
  {
    private readonly ICrawlerDataGenerator _sut;

    public DropBoxCrawlerBehaviour()
    {
        var nameClientFactory = new Mock<IDropBoxClientFactory>();
        var log = new Mock<ILogger>();

        _sut = new DropBoxCrawler(nameClientFactory.Object, log.Object);
    }

    [Fact]
    public void GetDataReturnsData()
    {
      var jobData = new CrawlJobData();

      _sut.GetData(jobData)
          .ShouldNotBeNull();
    }
  }
}
