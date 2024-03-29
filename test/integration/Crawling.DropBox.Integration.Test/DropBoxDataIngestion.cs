using Xunit;
using System.Linq;
using CrawlerIntegrationTesting.Clues;
using Xunit.Abstractions;

namespace Crawling.DropBox.Integration.Test
{
    public class DataIngestion : IClassFixture<DropBoxTestFixture>
    {
        private readonly DropBoxTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public DataIngestion(DropBoxTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Theory]
        [InlineData("/Provider/Root", 1)] 
        [InlineData("/Files/Directory", 3)]
        [InlineData("/Files/File", 3)]
        public void CorrectNumberOfEntityTypes(string entityType, int expectedCount)
        {
            var foundCount = _fixture.ClueStorage.CountOfType(entityType);
            Assert.Equal(expectedCount, foundCount);
        }

        [Fact]
        public void EntityCodesAreUnique()
        {            
            var count = _fixture.ClueStorage.Clues.Count();
            var unique = _fixture.ClueStorage.Clues.Distinct(new ClueComparer()).Count();

            //You could use this method to output info of all clues
            PrintClues();

            Assert.Equal(unique, count);
        }

        private void PrintClues()
        {
            foreach(var clue in _fixture.ClueStorage.Clues)
            {
                _output.WriteLine(clue.OriginEntityCode.ToString());
            }
        }
    }
}
