using System;
using System.Linq;
using CluedIn.Core.Crawling;
using AutoFixture.Xunit2;
using Should;
using Xunit;
using CluedIn.Crawling.DropBox.Core;
using CluedIn.Crawling.DropBox.Test.Common;
using Dropbox.Api;

namespace Provider.DropBox.Unit.Test.DropBoxProvider
{
  public class GetHelperConfigurationBehaviour : DropBoxProviderTest
  {
    private readonly CrawlJobData _jobData;

    public GetHelperConfigurationBehaviour()
    {
      _jobData = new DropBoxCrawlJobData(DropBoxConfiguration.Create());
    }

    [Fact]
    public void Throws_ArgumentNullException_With_Null_CrawlJobData_Parameter()
    {
      var ex = Assert.Throws<AggregateException>(
          () => Sut.GetHelperConfiguration(null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())
              .Wait());

      ((ArgumentNullException)ex.InnerExceptions.Single())
          .ParamName
          .ShouldEqual("jobData");
    }

    [Theory]
    [InlineAutoData]
    public void Returns_ValidDictionary_Instance(Guid organizationId, Guid userId, Guid providerDefinitionId)
    {
      Sut.GetHelperConfiguration(null, _jobData, organizationId, userId, providerDefinitionId)
          .Result
          .ShouldNotBeNull();
    }


    [Theory]
    [InlineAutoData("Providers.DropBoxClientId", "Providers.DropBoxClientId", "f204fhceqpf0gnk")]
    [InlineAutoData("Providers.DropBoxClientSecret", "Providers.DropBoxClientSecret", "6iz7w5bslf3c9g9")]
    public void Returns_Expected_Data(string key, string propertyName, object expectedValue, Guid organizationId, Guid userId, Guid providerDefinitionId) 
    {
      var property = _jobData.GetType().GetProperty(propertyName);
      property?.SetValue(_jobData, expectedValue);

      var result = Sut.GetHelperConfiguration(null, _jobData, organizationId, userId, providerDefinitionId)
                      .Result;

      result
          .ContainsKey(key)
          .ShouldBeTrue(
              $"{key} not found in results");

      result[key]
          .ShouldEqual(expectedValue);
    }
  }
}
