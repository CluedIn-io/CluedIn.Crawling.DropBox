using CluedIn.Core.Data;

namespace CluedIn.Crawling.DropBox.Vocabularies
{
    /// <summary>
    /// The drop box file vocabulary.
    /// </summary>
    public class DropBoxFileVocabulary : DropBoxMetaDataVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DropBoxFileVocabulary"/> class.
        /// </summary>
        public DropBoxFileVocabulary()
        {
            VocabularyName = "DropBox Files";
            KeyPrefix      = "dropbox.file";
            Grouping       = EntityType.Files.File;
        }
    }
}
