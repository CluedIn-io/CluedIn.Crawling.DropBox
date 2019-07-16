using CluedIn.Crawling;
using CluedIn.Crawling.DropBox.Core;
using System.IO;
using System.Reflection;
using CluedIn.Crawling.DropBox.Test.Common;
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

            p.ProcessClue += CrawlerHost_ProcessClue;            

            p.Execute(DropBoxConfiguration.Create(), DropBoxConstants.ProviderId);
        }

        public ClueStorage ClueStorage { get; }

        private void CrawlerHost_ProcessClue(CluedIn.Core.Data.Clue clue)
        {
            //_outputHelper.WriteLine($"Processing crawler clue {JsonConvert.SerializeObject(clue)}");

            ClueStorage.AddClue(clue);
        }

    }
}


