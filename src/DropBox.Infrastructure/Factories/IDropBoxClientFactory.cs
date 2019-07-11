using CluedIn.Crawling.DropBox.Core;

namespace CluedIn.Crawling.DropBox.Infrastructure.Factories
{
    public interface IDropBoxClientFactory
    {
        DropBoxClient CreateNew(DropBoxCrawlJobData dropboxCrawlJobData);
    }
}
