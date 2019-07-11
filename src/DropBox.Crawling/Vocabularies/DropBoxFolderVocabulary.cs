using CluedIn.Core.Data;

namespace CluedIn.Crawling.DropBox.Vocabularies
{
    /// <summary>The drop box folder vocabulary.</summary>
    public class DropBoxFolderVocabulary : DropBoxMetaDataVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DropBoxFolderVocabulary"/> class.
        /// </summary>
        public DropBoxFolderVocabulary()
        {
            VocabularyName = "DropBox Folders";
            KeyPrefix      = "dropbox.folder";
            Grouping       = EntityType.Files.Directory;
        }
    }
}
