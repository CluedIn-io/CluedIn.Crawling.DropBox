using System;
using System.Globalization;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Logging;
using CluedIn.Crawling.DropBox.Factories;
using CluedIn.Crawling.DropBox.Vocabularies;
using CluedIn.Crawling.Factories;
using Dropbox.Api.Sharing;

namespace CluedIn.Crawling.DropBox.ClueProducers
{
    public class SharedFolderClueProducer : BaseClueProducer<SharedFolderMetadata>
    {
        private readonly IClueFactory _factory;
        private readonly ILogger _log;
        private readonly Clue _providerRoot;

        public SharedFolderClueProducer([NotNull] IClueFactory factory, ILogger log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            if (factory is DropBoxClueFactory dropBoxClueFactory)
                _providerRoot = dropBoxClueFactory.ProviderRoot; // TODO think of better way of doing referencing the base provider clue
        }

        protected override Clue MakeClueImpl(SharedFolderMetadata value, Guid accountId)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var clue = _factory.Create(EntityType.Files.Directory, value.SharedFolderId.ToString(CultureInfo.InvariantCulture), accountId);

            var data = clue.Data.EntityData;

            if (value.Name != null)
            {
                data.Name = value.Name;
                data.DisplayName = value.Name;
            }

            if (value.OwnerTeam != null)
            {
                data.Properties[DropBoxVocabulary.SharedFolder.Owner] = value.OwnerTeam.Name;
                try
                {
                    _factory.CreateOutgoingEntityReference(clue, EntityType.Infrastructure.Group, EntityEdgeType.Owns, value.OwnerTeam, value.OwnerTeam.Id);
                }
                catch (Exception)
                {
                    _log.Warn(() => "Could not parse Owner for Dropbox Shared Folder");
                }
            }

            if (value.PathLower != null)
                data.Properties[DropBoxVocabulary.SharedFolder.FolderPath] = value.PathLower;

            _factory.CreateOutgoingEntityReference(clue, EntityType.Provider.Root, EntityEdgeType.ManagedIn, _providerRoot, _providerRoot.OriginEntityCode.Value);

            return clue;
        }
    }
}