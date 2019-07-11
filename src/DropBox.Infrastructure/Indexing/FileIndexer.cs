using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Agent.Jobs;
using CluedIn.Core.Configuration;
using CluedIn.Core.Data;
using CluedIn.Core.IO;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace CluedIn.Crawling.DropBox.Infrastructure.Indexing
{
    /// <summary>The file indexer.</summary>
    public class FileIndexer : IFileIndexer
    {
        private readonly DropboxClient _client;
        private readonly IAgentJobProcessorArguments _args;
        private readonly ApplicationContext _context;

        public FileIndexer([NotNull] DropboxClient client, [NotNull] IAgentJobProcessorArguments args, [NotNull] ApplicationContext context)
        {
            _client  = client ?? throw new ArgumentNullException(nameof(client));
            _args    = args ?? throw new ArgumentNullException(nameof(args));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Index(Metadata file, Clue clue)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (clue == null)
                throw new ArgumentNullException(nameof(clue));

            if (file.AsFile.Size == 0)
                return;

            if (!ConfigurationManager.AppSettings.GetFlag("Crawl.InitialCrawl.FileIndexing", true))
                return;

            if ((long)file.AsFile.Size > Constants.MaxFileIndexingFileSize)
                return;

            var f = await _client.Files.DownloadAsync(file.PathLower, file.AsFile.Rev);

            using (var tempFile = new TemporaryFile(CleanFileName(file.Name)))
            {
                using (var stream = await f.GetContentAsStreamAsync())
                using (var fs = new FileStream(tempFile.FileInfo.FullName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    await stream.CopyToAsync(fs).ConfigureAwait(false);
                }

                FileCrawlingUtility.IndexFile(tempFile, clue.Data, clue, _args, _context);
            }
        }

        private string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}
