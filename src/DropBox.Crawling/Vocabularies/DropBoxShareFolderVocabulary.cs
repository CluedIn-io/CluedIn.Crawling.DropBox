// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropBoxShareFolderVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the DropBoxShareFolderVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.Crawling.DropBox.Vocabularies
{
    /// <summary>
    /// The drop box share folder vocabulary.
    /// </summary>
    public class DropBoxShareFolderVocabulary : SimpleVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DropBoxShareFolderVocabulary"/> class.
        /// </summary>
        public DropBoxShareFolderVocabulary()
        {
            VocabularyName = "DropBox Shared Folder";
            KeyPrefix      = "dropbox.sharedFolder";
            KeySeparator   = ".";
            Grouping       = EntityType.Files.Directory;
            
            AddGroup("Dropbox Shared Folder Details", group =>
            {
                FolderPath       = group.Add(new VocabularyKey("folderPath"));
                Owner            = group.Add(new VocabularyKey("owner", VocabularyKeyVisibility.Hidden));
                AccessType       = group.Add(new VocabularyKey("accessType"));
                SharedLinkPolicy = group.Add(new VocabularyKey("sharedLinkPolicy"));
            });
        }

        public VocabularyKey SharedLinkPolicy { get; set; }

        public VocabularyKey Owner { get; set; }

        public VocabularyKey AccessType { get; set; }

        public VocabularyKey FolderPath { get; set; }
    }
}
