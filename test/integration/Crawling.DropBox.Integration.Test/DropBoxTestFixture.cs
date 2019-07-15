using CluedIn.Crawling;
using CluedIn.Crawling.DropBox.Core;
using System.IO;
using System.Reflection;
using CrawlerIntegrationTesting.Clues;

namespace Crawling.DropBox.Integration.Test
{
    public class DropBoxTestFixture
    {
        public DropBoxTestFixture()
        {
            var executingFolder = new FileInfo(Assembly.GetExecutingAssembly().CodeBase.Substring(8)).DirectoryName;
            var p = new DebugCrawlerHost<DropBoxCrawlJobData>(executingFolder, DropBoxConstants.ProviderName);

            ClueStorage = new ClueStorage();

            p.ProcessClue += ClueStorage.AddClue;            

            p.Execute(DropBoxConfiguration.Create(), DropBoxConstants.ProviderId);
        }

        public ClueStorage ClueStorage { get; }

        public void Dispose()
        {
        }

    }
}


