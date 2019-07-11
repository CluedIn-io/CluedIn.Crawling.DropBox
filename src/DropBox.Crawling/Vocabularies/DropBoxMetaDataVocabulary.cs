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
                ItemName               = group.Add(new VocabularyKey("name", VocabularyKeyVisiblity.Hidden));
                Path                   = group.Add(new VocabularyKey("path"));
                Extension              = group.Add(new VocabularyKey("extension"));
                Size                   = group.Add(new VocabularyKey("size", VocabularyKeyDataType.Number));
                Revision               = group.Add(new VocabularyKey("revision", VocabularyKeyDataType.Number));
                Rev                    = group.Add(new VocabularyKey("rev", VocabularyKeyDataType.Number, VocabularyKeyVisiblity.Hidden));
                Modifier = group.Add(new VocabularyKey("Modifier", VocabularyKeyVisiblity.Hidden));
                Bytes                  = group.Add(new VocabularyKey("bytes", VocabularyKeyDataType.Number, VocabularyKeyVisiblity.Hidden));
                Root                   = group.Add(new VocabularyKey("root"));
                ClientMTime            = group.Add(new VocabularyKey("clientMTime", VocabularyKeyVisiblity.Hidden));
                ClientMTimeDate        = group.Add(new VocabularyKey("clientMTimeDate", VocabularyKeyVisiblity.Hidden));
                Contents               = group.Add(new VocabularyKey("contents"));
                Hash                   = group.Add(new VocabularyKey("hash", VocabularyKeyVisiblity.Hidden));
                Icon = group.Add(new VocabularyKey("icon", VocabularyKeyVisiblity.Hidden));
                IsDeleted              = group.Add(new VocabularyKey("isDeleted", VocabularyKeyVisiblity.Hidden));
                IsDirectory            = group.Add(new VocabularyKey("isDir", VocabularyKeyVisiblity.Hidden));
                Modified               = group.Add(new VocabularyKey("modified", VocabularyKeyDataType.Boolean, VocabularyKeyVisiblity.Hidden));
                ModifiedDate           = group.Add(new VocabularyKey("modifiedDate", VocabularyKeyDataType.DateTime));
                HasThumbnail           = group.Add(new VocabularyKey("thumbExists", VocabularyKeyVisiblity.Hidden));
                UTCDateClientMTime     = group.Add(new VocabularyKey("utcDateClientMTime", VocabularyKeyVisiblity.Hidden));
                UTCDateModified        = group.Add(new VocabularyKey("utcDateModified", VocabularyKeyVisiblity.Hidden));
                ParentSharedFolderId   = group.Add(new VocabularyKey("ParentSharedFolderId", VocabularyKeyVisiblity.Hidden));
                PhotoInfo              = group.Add(new VocabularyKey("PhotoInfo", VocabularyKeyVisiblity.Hidden));
                ReadOnly               = group.Add(new VocabularyKey("ReadOnly", VocabularyKeyVisiblity.Hidden));
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
