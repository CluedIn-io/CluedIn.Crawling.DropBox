using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Castle.Windsor;
using CluedIn.Core;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.DataStore;
using CluedIn.Core.Logging;
using CluedIn.Core.Providers;
using CluedIn.Crawling.DropBox.Core;
using CluedIn.Crawling.DropBox.Infrastructure;
using CluedIn.Crawling.DropBox.Infrastructure.Factories;
using CluedIn.Crawling.DropBox.Test.Common;
using Dropbox.Api.Users;
using Moq;
using RestSharp;
using Xunit;

namespace Provider.DropBox.Unit.Test.DropBoxProvider
{
    public abstract class DropBoxProviderTest
    {
        protected readonly ProviderBase Sut;
        protected readonly Mock<IDropBoxClientFactory> NameClientFactory;
        protected readonly ApplicationContext ApplicationContext;
        protected readonly Mock<IWindsorContainer> Container;
        protected readonly Mock<SystemContext> SystemContext;
        protected readonly Mock<ILogger> Logger;
        protected readonly Guid OrganizationId = Guid.NewGuid();
        protected readonly Dictionary<string, object> Configuration;
        protected readonly DropBoxCrawlJobData CrawlJobData;
        protected readonly Mock<DropBoxClient> Client;
        protected readonly Mock<IRelationalDataStore<Token>> TokenStore;

        protected DropBoxProviderTest()
        {
            Container = new Mock<IWindsorContainer>();
            SystemContext = new Mock<SystemContext>(Container.Object);
            TokenStore = new Mock<IRelationalDataStore<Token>>();
            NameClientFactory = new Mock<IDropBoxClientFactory>();
            ApplicationContext = new ApplicationContext(Container.Object);
            Logger = new Mock<ILogger>();
            Configuration = DropBoxConfiguration.Create();
            CrawlJobData = new DropBoxCrawlJobData(Configuration);
            Client = new Mock<DropBoxClient>(Logger.Object, CrawlJobData, new RestClient());
            Sut = new CluedIn.Provider.DropBox.DropBoxProvider(ApplicationContext, NameClientFactory.Object, Logger.Object, null, TokenStore.Object);

            NameClientFactory.Setup(n => n.CreateNew(It.IsAny<DropBoxCrawlJobData>())).Returns(() => Client.Object);
        }

        public class TestAuthenticationTests : DropBoxProviderTest
        {
            [Fact(Skip = "Requires DropBoxClient.GetSpaceUsageAsync to be virtual as we are mocking concrete class NOT the interface")]
            public async Task TestAuthenticationLogsExceptionAndReturnsFalseWhenFails()
            {
                Client.Setup(n => n.GetSpaceUsageAsync()).Throws(new Exception());

                await Sut.TestAuthentication(null, Configuration, OrganizationId, Guid.Empty, Guid.Empty);

                Logger.Verify(n => n.Warn(It.IsAny<Func<string>>(), It.IsAny<Exception>()), Times.Once);
            }

            [Fact(Skip = "Requires DropBoxClient.GetSpaceUsageAsync to be virtual as we are mocking concrete class NOT the interface")]
            public async Task TestAuthenticationReturnsTrueIfHasResult()
            {
                Client.Setup(n => n.GetSpaceUsageAsync()).ReturnsAsync(new SpaceUsage());

                var result = await Sut.TestAuthentication(null, Configuration, OrganizationId, Guid.Empty, Guid.Empty);

                Assert.True(result);
            }
        }

        public class GetHelperConfigurationTests : DropBoxProviderTest
        {
            [Fact]
            public async Task GetHelperConfigurationReturnsWebHooks()
            {
                // TODO Setup Client to fake data
                var result = await Sut.GetHelperConfiguration(null, CrawlJobData, OrganizationId, Guid.Empty, Guid.Empty);

                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.Contains(result, n => n.Key == "webhooks");
                Assert.NotEmpty(result["webhooks"] as List<object>);
            }
        }

        public class GetAccountInformationTests : DropBoxProviderTest
        {
            [Theory(Skip = "Requires DropBoxClient.GetAccountInformationAsync to be virtual as we are mocking concrete class NOT the interface"), AutoData]
            public async Task GetAccountInformationReturnsAccountInformationWithPortalId(string accountId)
            {
                Client.Setup(n => n.GetAccountInformationAsync()).ReturnsAsync(new AccountInformation(accountId, accountId));
                var result = await Sut.GetAccountInformation(null, CrawlJobData, OrganizationId, Guid.Empty, Guid.Empty);

                Assert.NotNull(result);
                Assert.Equal(accountId.ToString(), result.AccountId);
                Assert.Equal(accountId.ToString(), result.AccountDisplay);
            }

            [Fact(Skip = "Requires DropBoxClient.GetAccountInformationAsync to be virtual as we are mocking concrete class NOT the interface")]
            public async Task GetAccountInformationHandlesNullResult()
            {
                Client.Setup(n => n.GetAccountInformationAsync()).ReturnsAsync(default(AccountInformation));
                var result = await Sut.GetAccountInformation(null, CrawlJobData, OrganizationId, Guid.Empty, Guid.Empty);

                Assert.NotNull(result);
                Assert.Equal(string.Empty, result.AccountId);
                Assert.Equal(string.Empty, result.AccountDisplay);
                Assert.NotEmpty(result.Errors);
            }

            [Fact(Skip = "Requires DropBoxClient.GetAccountInformationAsync to be virtual as we are mocking concrete class NOT the interface")]
            //[Fact]
            public async Task GetAccountInformationHandlesException()
            {
                Client.Setup(n => n.GetAccountInformationAsync()).Throws(new Exception());
                var result = await Sut.GetAccountInformation(null, CrawlJobData, OrganizationId, Guid.Empty, Guid.Empty);

                Assert.NotNull(result);
                Assert.Equal(string.Empty, result.AccountId);
                Assert.Equal(string.Empty, result.AccountDisplay);
                Assert.NotEmpty(result.Errors);
            }
        }
    }
}
