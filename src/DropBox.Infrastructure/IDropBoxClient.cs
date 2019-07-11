using System.Threading.Tasks;
using CluedIn.Core.Providers;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using Dropbox.Api.Users;

namespace CluedIn.Crawling.DropBox.Infrastructure
{
    public interface IDropBoxClient
    {
        Task<AccountInformation> GetAccountInformationAsync();
        Task<IDownloadResponse<FileMetadata>> GetThumbnailAsync(string path, ThumbnailFormat format = null, ThumbnailSize size = null, ThumbnailMode mode = null);
        Task<FullAccount> GetCurrentAccountAsync();
    }
}
