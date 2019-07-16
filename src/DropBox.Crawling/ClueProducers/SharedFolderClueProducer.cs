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

        protected override Clue MakeClueImpl(SharedFolderMetadata input, Guid accountId)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var clue = _factory.Create(EntityType.Files.Directory, input.SharedFolderId.ToString(CultureInfo.InvariantCulture), accountId);

            var data = clue.Data.EntityData;

            if (input.Name != null)
            {
                data.Name = input.Name;
                data.DisplayName = input.Name;
            }

            if (input.OwnerTeam != null)
            {
                data.Properties[DropBoxVocabulary.SharedFolder.Owner] = input.OwnerTeam.Name;
                try
                {
                    _factory.CreateOutgoingEntityReference(clue, EntityType.Infrastructure.Group, EntityEdgeType.Owns, input.OwnerTeam, input.OwnerTeam.Id);
                }
                catch (Exception)
                {
                    _log.Warn(() => "Could not parse Owner for Dropbox Shared Folder");
                }
            }

            if (input.PathLower != null)
                data.Properties[DropBoxVocabulary.SharedFolder.FolderPath] = input.PathLower;

            _factory.CreateOutgoingEntityReference(clue, EntityType.Provider.Root, EntityEdgeType.ManagedIn, _providerRoot, _providerRoot.OriginEntityCode.Value);

            return clue;
        }
    }
}
