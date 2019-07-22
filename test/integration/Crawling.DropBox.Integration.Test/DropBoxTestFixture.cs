using CluedIn.Crawling;
using CluedIn.Crawling.DropBox.Core;
using System.IO;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CluedIn.Core;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.DataStore;
using CluedIn.Crawling.DropBox.Test.Common;
using CrawlerIntegrationTesting.Clues;
using Crawling.DropBox.Integration.Test.Stubs;

namespace Crawling.DropBox.Integration.Test
{
    public class DropBoxTestFixture
    {
        public DropBoxTestFixture()
        {
            var executingFolder = new FileInfo(Assembly.GetExecutingAssembly().CodeBase.Substring(8)).DirectoryName;
            var p = new TestCrawlerHost(executingFolder, DropBoxConstants.ProviderName);

            // Use stub here to allow database logic to work in Provider.GetCrawlJobData
            if (!p.ContainerInstance.Kernel.HasComponent(typeof(IRelationalDataStore<Token>)))
            {
                p.ContainerInstance.Register(Component.For<IRelationalDataStore<Token>>()
                    .Forward<ISimpleDataStore<Token>>()
                    .Forward<IDataStore<Token>>()
                    .Forward<IDataStore>()
                    .Instance(new TokenDataStoreStub())
                    .LifestyleSingleton());
            }

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

    public class TestCrawlerHost : DebugCrawlerHost<DropBoxCrawlJobData>
    {
        public TestCrawlerHost(string binFolder, string providerName) : base(binFolder, providerName)
        {
        }

        public IWindsorContainer ContainerInstance => base.Container;
    }
}


