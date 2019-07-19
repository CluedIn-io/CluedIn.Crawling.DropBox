using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using CluedIn.Core.Data;
using CluedIn.Crawling.Factories;
using CluedIn.Core;
using CluedIn.Core.Agent.Jobs;
using CluedIn.Core.Crawling;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Logging;
using CluedIn.Crawling.DropBox.Core;
using CluedIn.Crawling.DropBox.Factories;
using CluedIn.Crawling.DropBox.Infrastructure.Factories;
using CluedIn.Crawling.DropBox.Infrastructure.Indexing;
using CluedIn.Crawling.DropBox.Infrastructure.UriBuilders;
using CluedIn.Crawling.DropBox.Vocabularies;
using CluedIn.Crawling.Helpers;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace CluedIn.Crawling.DropBox.ClueProducers
{
    public class FileMetadataClueProducer : BaseClueProducer<FileMetadata>
    {
        private readonly IClueFactory _factory;
        private readonly ILogger _log;
        private readonly BoxFileUriBuilder _uriBuilder;
        private readonly IAgentJobProcessorState<CrawlJobData> _state;
        private readonly ApplicationContext _context;
        private readonly DropBoxCrawlJobData _jobData;
        private readonly Clue _providerRoot;
        private readonly char[] _trimChars = { '/' };
        private readonly IDropBoxClientFactory _clientFactory;

        public FileMetadataClueProducer([NotNull] IClueFactory factory, ILogger log, BoxFileUriBuilder uriBuilder, IDropBoxClientFactory clientFactory, IAgentJobProcessorState<CrawlJobData> state, ApplicationContext context)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _uriBuilder = uriBuilder ?? throw new ArgumentNullException(nameof(uriBuilder));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _context = context;

            if (state.JobData is DropBoxCrawlJobData jobData)
                _jobData = jobData;
            else
                throw new ArgumentException("state parameter must have JobData of type DropBoxCrawlJobData");
       
            if (factory is DropBoxClueFactory dropBoxClueFactory)
                _providerRoot = dropBoxClueFactory.ProviderRoot; // TODO think of better way of doing referencing the base provider clue
        }

        protected override Clue MakeClueImpl([NotNull] FileMetadata input, Guid accountId)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var clue = _factory.Create(EntityType.Files.File, input.PathLower, accountId);
            clue.ValidationRuleSuppressions.Add(Constants.Validation.Rules.DATA_001_File_MustBeIndexed);

            var data = clue.Data.EntityData;

            var value = input.AsFile;
            
            if (value.Name != null)
            {
                data.Name = value.Name;
                data.DisplayName = value.Name;
                data.Properties[DropBoxVocabulary.File.ItemName] = value.Name;
            }

            data.DocumentSize = (long)value.Size;
            data.ModifiedDate = value.ServerModified;

            try
            {
                var url = _uriBuilder.GetUri(value);
                data.Uri = url;
                data.Properties[DropBoxVocabulary.File.EditUrl] = url.ToString().Replace("www.dropbox.com/", "www.dropbox.com/ow/msft/edit/").Replace("?preview=", "/").Replace("+", "%20");
            }
            catch (Exception exc)
            {
                _log.Warn(() => "Could not create ShareTask Dropbox File", exc); //Handle error
            }

            data.Properties[DropBoxVocabulary.File.Bytes] = value.Size.PrintIfAvailable();
            data.Properties[DropBoxVocabulary.File.ClientMTime] = value.ClientModified.PrintIfAvailable();

           
            if (value.PathLower != null)
            {
                data.Properties[DropBoxVocabulary.File.Path] = value.PathLower.PrintIfAvailable();
                _factory.CreateOutgoingEntityReference(clue, EntityType.Files.Directory, "Parent", value, "/" + VirtualPathUtility.GetDirectory(value.PathLower).Trim(_trimChars));
            }

            data.Properties[DropBoxVocabulary.File.Rev] = value.Rev.PrintIfAvailable();

            if (value.Rev != null)
            {
                data.Revision = value.Rev.ToString(CultureInfo.InvariantCulture);

            }

            data.Properties[DropBoxVocabulary.File.ParentSharedFolderId] = value.ParentSharedFolderId.PrintIfAvailable();
            _factory.CreateOutgoingEntityReference(clue, EntityType.Provider.Root, EntityEdgeType.ManagedIn, _providerRoot, _providerRoot.OriginEntityCode.Value);

            


            var shouldIndexFile = _jobData.FileSizeLimit == null || _jobData.FileSizeLimit.Value == 0 || (long)value.Size < _jobData.FileSizeLimit.Value;

            var client = _clientFactory.CreateNew(_jobData);
            if (shouldIndexFile)
            {
                try
                {
                    var indexer = new FileIndexer(client, _state, _context);
                    Task.Run(() => indexer.Index(value, clue).ConfigureAwait(false));
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exc)
                {
                    _log.Warn(() => "Could not index Dropbox File", exc); //Handle error
                }
            }

            if (data.PreviewImage == null)
            {
                var extension = string.Empty;

                try
                {
                    extension = new FileInfo(input.Name).Extension;
                }
                catch (ArgumentException)
                {
                }

                var allowedExtensions = new[]
                    {
                        ".jpg", ".jpeg", ".tif", ".tiff", ".png", ".gif", ".bmp"
                    }.ToHashSet();

                if (!string.IsNullOrEmpty(extension) && allowedExtensions.Contains(extension.ToLowerInvariant()))
                {
                    try
                    {
                        var thumbnail = client.GetThumbnailAsync(value.PathLower, ThumbnailFormat.Jpeg.Instance, ThumbnailSize.W1024h768.Instance).Result;

                        var bytes = thumbnail.GetContentAsByteArrayAsync().Result;
                        var rawDataPart = new RawDataPart()
                        {
                            Type = "/RawData/PreviewImage",
                            MimeType = CluedIn.Core.FileTypes.MimeType.Jpeg.Code,
                            FileName = "preview_{0}".FormatWith(data.OriginEntityCode.Key),
                            RawDataMD5 = FileHashUtility.GetMD5Base64String(bytes),
                            RawData = Convert.ToBase64String(bytes)
                        };

                        clue.Details.RawData.Add(rawDataPart);

                        data.PreviewImage = new ImageReferencePart(rawDataPart);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (DropboxException exc)
                    {
                        _log.Warn(new { FileExtention = extension }, () => "Could not get thumbnail from Dropbox: " + extension, exc);
                    }
                }
            }

            return clue;
        }
    }
}
