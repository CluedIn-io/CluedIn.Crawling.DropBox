using System;
using System.Threading.Tasks;
using System.Web;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Logging;
using CluedIn.Crawling.DropBox.Factories;
using CluedIn.Crawling.DropBox.Infrastructure.UriBuilders;
using CluedIn.Crawling.DropBox.Vocabularies;
using CluedIn.Crawling.Factories;
using Dropbox.Api.Files;

namespace CluedIn.Crawling.DropBox.ClueProducers
{
    public class FolderClueProducer : BaseClueProducer<Metadata>
    {
        private readonly IClueFactory _factory;
        private readonly ILogger _log;
        private readonly BoxFolderUriBuilder _uriBuilder;
        private readonly Clue _providerRoot;
        private readonly char[] _trimChars = { '/' };

        public FolderClueProducer([NotNull] IClueFactory factory, ILogger log, BoxFolderUriBuilder uriBuilder)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _uriBuilder = uriBuilder ?? throw new ArgumentNullException(nameof(uriBuilder));

            if (factory is DropBoxClueFactory dropBoxClueFactory) _providerRoot = dropBoxClueFactory.ProviderRoot; // TODO think of better way of doing referencing the base provider clue
        }

        protected override Clue MakeClueImpl(Metadata input, Guid accountId)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var type = input.IsFolder ? EntityType.Files.Directory : EntityType.Files.File;
            var clue = _factory.Create(type, input.PathLower, accountId);

            var data = clue.Data.EntityData;

            if (input.Name != null)
            {
                if (input.PathLower == "/")
                {
                    data.Name = input.Name;
                    data.DisplayName = "Root Folder";
                    data.Properties[DropBoxVocabulary.File.ItemName] = "Root Folder";
                    _factory.CreateOutgoingEntityReference(clue, EntityType.Files.Directory, EntityEdgeType.Parent, _providerRoot, _providerRoot.Id.ToString());
                }
                else
                {
                    data.Name = input.Name;
                    data.DisplayName = input.Name;
                    data.Properties[DropBoxVocabulary.Folder.ItemName] = input.Name;
                }
            }

            try
            {
                data.Uri = _uriBuilder.GetUri(input);
            }
            catch (Exception exc)
            {
                _log.Warn(() => "Could not create Uri for Dropbox File", exc); //Handle error
            }

            data.Properties[DropBoxVocabulary.Folder.IsDeleted] = input.IsDeleted.ToString();
            data.Properties[DropBoxVocabulary.Folder.IsDirectory] = input.IsFolder.ToString();

            if (input.PathLower != null)
            {
                if (input.PathLower != "/")
                {
                    data.Properties[DropBoxVocabulary.Folder.Path] = input.PathLower;
                    _factory.CreateOutgoingEntityReference(clue, EntityType.Files.Directory, EntityEdgeType.Parent, input, "/" + VirtualPathUtility.GetDirectory(input.PathLower).Trim(_trimChars));
                }
            }

            if (input.ParentSharedFolderId != null)
                data.Properties[DropBoxVocabulary.File.ParentSharedFolderId] = input.ParentSharedFolderId;

            _factory.CreateOutgoingEntityReference(clue, EntityType.Provider.Root, EntityEdgeType.ManagedIn, _providerRoot, _providerRoot.OriginEntityCode.Value);

            return clue;
        }
    }
}
