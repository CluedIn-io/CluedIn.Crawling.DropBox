using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Data;
using Dropbox.Api.Files;

namespace CluedIn.Crawling.DropBox.Infrastructure.Indexing
{
    /// <summary>The FileIndexer interface.</summary>
    public interface IFileIndexer
    {
        Task Index([NotNull] Metadata file, [NotNull] Clue clue);
    }
}
