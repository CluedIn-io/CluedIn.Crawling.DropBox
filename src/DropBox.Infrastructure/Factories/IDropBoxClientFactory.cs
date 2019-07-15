using CluedIn.Crawling.DropBox.Core;

namespace CluedIn.Crawling.DropBox.Infrastructure.Factories
{
    public interface IDropBoxClientFactory
    {
        IDropBoxClient CreateNew(DropBoxCrawlJobData dropboxCrawlJobData);
    }
}
