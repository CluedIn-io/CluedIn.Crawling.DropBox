using System.Threading.Tasks;
using CluedIn.Core.Providers;
using CluedIn.Crawling.DropBox.Core.Models;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using Dropbox.Api.Stone;
using Dropbox.Api.Users;

namespace CluedIn.Crawling.DropBox.Infrastructure
{
    public interface IDropBoxClient
    {
        Task<AccountInformation> GetAccountInformationAsync();
        Task<SpaceUsage> GetSpaceUsageAsync();
        Task<FullAccount> GetCurrentAccountAsync();
        Task<ListFoldersResult> ListFoldersAsync();
        Task<ListFolderResult> ListFolderContinueAsync(string cursor);
        Task<ListFoldersResult> ListFoldersContinueAsync(string cursor);
        Task<ListFolderResult> ListFolderAsync(string path, bool includeDeleted = false);
        Task<ListRevisionsResult> ListRevisionsAsync(string path, ulong limit = 100);
        Task<Metadata> GetMetadataAsync(string path, bool includeMediaInfo = false, bool includeDeleted = false);
        Task<ListFolderGetLatestCursorResult> ListFolderGetLatestCursorAsync(string path, bool recursive = true, bool includeMediaInfo = false);
        Task<IDownloadResponse<FileMetadata>> GetThumbnailAsync(string path, ThumbnailFormat format = null, ThumbnailSize size = null, ThumbnailMode mode = null);
        Task<FolderList> GetFolderListViaRestAsync();
        Task<Permissions> GetFolderPermissions(Entry folder, int limit = 10);
    }
}
