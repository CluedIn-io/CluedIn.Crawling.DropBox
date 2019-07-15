using Castle.Windsor;
using CluedIn.Core;
using CluedIn.Core.Logging;
using CluedIn.Core.Providers;
using CluedIn.Crawling.DropBox.Infrastructure.Factories;
using Moq;

namespace Provider.DropBox.Unit.Test.DropBoxProvider
{
    public abstract class DropBoxProviderTest
    {
        protected readonly ProviderBase Sut;

        protected Mock<IDropBoxClientFactory> NameClientFactory;
        protected Mock<IWindsorContainer> Container;
        protected readonly Mock<ILogger> Logger;

        protected DropBoxProviderTest()
        {
            Container = new Mock<IWindsorContainer>();
            NameClientFactory = new Mock<IDropBoxClientFactory>();
            Logger = new Mock<ILogger>();
            var applicationContext = new ApplicationContext(Container.Object);

            Sut = new CluedIn.Provider.DropBox.DropBoxProvider(applicationContext, NameClientFactory.Object, Logger.Object);
        }
    }
}
