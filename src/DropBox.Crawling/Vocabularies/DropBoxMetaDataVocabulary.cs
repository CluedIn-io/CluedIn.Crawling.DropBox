// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropBoxMetaDataVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the DropBoxMetaDataVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.Crawling.DropBox.Vocabularies
{
    /// <summary>The drop box meta data vocabulary.</summary>
    public abstract class DropBoxMetaDataVocabulary : SimpleVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DropBoxMetaDataVocabulary"/> class.
        /// </summary>
        protected DropBoxMetaDataVocabulary()
        {
            VocabularyName = "DropBox Metadata";
            KeyPrefix      = "dropbox.metadata";
            KeySeparator   = ".";
            Grouping       = EntityType.Files.File;

            AddGroup("Dropbox Metadata Details", group =>
            {
                ItemName               = group.Add(new VocabularyKey("name", VocabularyKeyVisibility.Hidden));
                Path                   = group.Add(new VocabularyKey("path"));
                Extension              = group.Add(new VocabularyKey("extension"));
                Size                   = group.Add(new VocabularyKey("size", VocabularyKeyDataType.Number));
                Revision               = group.Add(new VocabularyKey("revision", VocabularyKeyDataType.Number));
                Rev                    = group.Add(new VocabularyKey("rev", VocabularyKeyDataType.Number, VocabularyKeyVisibility.Hidden));
                Modifier = group.Add(new VocabularyKey("Modifier", VocabularyKeyVisibility.Hidden));
                Bytes                  = group.Add(new VocabularyKey("bytes", VocabularyKeyDataType.Number, VocabularyKeyVisibility.Hidden));
                Root                   = group.Add(new VocabularyKey("root"));
                ClientMTime            = group.Add(new VocabularyKey("clientMTime", VocabularyKeyVisibility.Hidden));
                ClientMTimeDate        = group.Add(new VocabularyKey("clientMTimeDate", VocabularyKeyVisibility.Hidden));
                Contents               = group.Add(new VocabularyKey("contents"));
                Hash                   = group.Add(new VocabularyKey("hash", VocabularyKeyVisibility.Hidden));
                Icon = group.Add(new VocabularyKey("icon", VocabularyKeyVisibility.Hidden));
                IsDeleted              = group.Add(new VocabularyKey("isDeleted", VocabularyKeyVisibility.Hidden));
                IsDirectory            = group.Add(new VocabularyKey("isDir", VocabularyKeyVisibility.Hidden));
                Modified               = group.Add(new VocabularyKey("modified", VocabularyKeyDataType.Boolean, VocabularyKeyVisibility.Hidden));
                ModifiedDate           = group.Add(new VocabularyKey("modifiedDate", VocabularyKeyDataType.DateTime));
                HasThumbnail           = group.Add(new VocabularyKey("thumbExists", VocabularyKeyVisibility.Hidden));
                UTCDateClientMTime     = group.Add(new VocabularyKey("utcDateClientMTime", VocabularyKeyVisibility.Hidden));
                UTCDateModified        = group.Add(new VocabularyKey("utcDateModified", VocabularyKeyVisibility.Hidden));
                ParentSharedFolderId   = group.Add(new VocabularyKey("ParentSharedFolderId", VocabularyKeyVisibility.Hidden));
                PhotoInfo              = group.Add(new VocabularyKey("PhotoInfo", VocabularyKeyVisibility.Hidden));
                ReadOnly               = group.Add(new VocabularyKey("ReadOnly", VocabularyKeyVisibility.Hidden));
            });

            EmbedUrl               = Add(new VocabularyKey("embedUrl"));
            EditUrl                = Add(new VocabularyKey("editUrl"));

            AddMapping(EmbedUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInFile.EmbedUrl);
            AddMapping(EditUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInFile.EditUrl);
            //this.AddMapping(this.Path, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInFile.Pa)
        }

        public VocabularyKey EditUrl { get; set; }
        public VocabularyKey Modifier { get; set; }
        public VocabularyKey ParentSharedFolderId { get; set; }
        public VocabularyKey PhotoInfo { get; set; }
        public VocabularyKey ReadOnly { get; set; }

        public VocabularyKey Bytes { get; set; }

        public VocabularyKey ClientMTime { get; set; }

        public VocabularyKey ClientMTimeDate { get; set; }

        public VocabularyKey Contents { get; set; }

        public VocabularyKey Extension { get; set; }

        public VocabularyKey Hash { get; set; }

        public VocabularyKey Icon { get; set; }

        public VocabularyKey IsDeleted { get; set; }

        public VocabularyKey IsDirectory { get; set; }

        public VocabularyKey Modified { get; set; }

        public VocabularyKey ModifiedDate { get; set; }

        public VocabularyKey Path { get; set; }

        public VocabularyKey Rev { get; set; }

        public VocabularyKey Revision { get; set; }

        public VocabularyKey Root { get; set; }

        public VocabularyKey Size { get; set; }

        public VocabularyKey HasThumbnail { get; set; }

        public VocabularyKey UTCDateClientMTime { get; set; }

        public VocabularyKey UTCDateModified { get; set; }

        public VocabularyKey EmbedUrl { get; private set; }

        public VocabularyKey ItemName { get; private set; }
    }
}
