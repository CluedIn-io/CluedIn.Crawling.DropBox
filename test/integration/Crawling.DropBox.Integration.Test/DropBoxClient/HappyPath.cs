using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Logging;
using CluedIn.Crawling.DropBox.Core;
using CluedIn.Crawling.DropBox.Test.Common;
using Dropbox.Api.Files;
using Moq;
using RestSharp;
using Xunit;

namespace Crawling.DropBox.Integration.Test.DropBoxClient
{
    public class HappyPath
    {
        private readonly CluedIn.Crawling.DropBox.Infrastructure.DropBoxClient _sut;

        public HappyPath()
        {
            var logger = new Mock<ILogger>();
            var jobData = new DropBoxCrawlJobData(DropBoxConfiguration.Create());

            _sut = new CluedIn.Crawling.DropBox.Infrastructure.DropBoxClient(logger.Object, jobData, new RestClient());
        }

        [Fact]
        public void IsNotNull()
        {
            Assert.NotNull(_sut);
        }

        [Fact]
        public async Task AccountInformationIsAvailable()
        {
            Assert.NotNull(await _sut.GetAccountInformationAsync());
        }

        [Fact]
        public async Task CurrentAccountIsAvailable()
        {
            Assert.NotNull(await _sut.GetCurrentAccountAsync());
        }

        [Fact]
        public async Task SpaceUsageIsAvailable()
        {
            Assert.NotNull(await _sut.GetSpaceUsageAsync());
        }

        [Fact]
        public async Task FoldersAreAvailable()
        {
            Assert.NotNull(await _sut.ListFoldersAsync());
        }

        [Fact]
        public async Task CanListFolderContent()
        {
            var root = await _sut.ListFolderAsync("");
            Assert.NotNull(root);
            Assert.NotEqual(0, root.Entries.Count);
        }

        [Fact]
        public async Task CanCreateThumbnail()
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".tif", ".tiff", ".png", ".gif", ".bmp" }.ToHashSet();

            var rootItems = await _sut.ListFolderAsync("");
            foreach (var item in rootItems.Entries.Where(n => n.IsFile))
            {
                var extension = Path.GetExtension(item.Name);
                if (!string.IsNullOrEmpty(extension) && allowedExtensions.Contains(extension.ToLowerInvariant()))
                {
                    var thumbnail = await _sut.GetThumbnailAsync(item.PathLower, ThumbnailFormat.Jpeg.Instance, ThumbnailSize.W1024h768.Instance);
                    Assert.NotNull(thumbnail);

                    var bytes = await thumbnail.GetContentAsByteArrayAsync();

                    Assert.NotNull(bytes);
                    Assert.NotEmpty(bytes);
                }
            }
        }

        [Fact]
        public async Task CanGetFolderListViaRest()
        {
            Assert.NotNull(await _sut.GetFolderListViaRestAsync());
        }

        [Fact]
        public async Task CanGetFolderPermissions()
        {
            var folders = await _sut.GetFolderListViaRestAsync();
            foreach (var entry in folders.entries)
            {
                Assert.NotNull(await _sut.GetFolderPermissions(entry));
            }
        }

        [Fact]
        public async Task CanGetMetaData()
        {
            var root = await _sut.ListFolderAsync("");
            Assert.NotEmpty(root.Entries);

            Assert.NotNull(await _sut.GetMetadataAsync(root.Entries[0].PathLower));
        }

        [Fact]
        public async Task CanUseCursor()
        {
            var folder = await _sut.ListFolderAsync("", 1);
            do
            {
                folder = await _sut.ListFolderContinueAsync(folder.Cursor);
            } while (folder != null && folder.HasMore);
        }
    }
}
