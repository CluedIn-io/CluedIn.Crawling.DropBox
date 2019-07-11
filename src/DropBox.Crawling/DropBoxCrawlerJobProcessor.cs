using CluedIn.Crawling.DropBox.Core;

namespace CluedIn.Crawling.DropBox
{
    public class DropBoxCrawlerJobProcessor : GenericCrawlerTemplateJobProcessor<DropBoxCrawlJobData>
    {
        public DropBoxCrawlerJobProcessor(DropBoxCrawlerComponent component) : base(component)
        {
        }
    }
}
