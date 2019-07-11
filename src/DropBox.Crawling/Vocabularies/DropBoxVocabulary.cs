// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropBoxVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the DropBoxVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CluedIn.Crawling.DropBox.Vocabularies
{
    /// <summary>
    /// The drop box vocabulary.
    /// </summary>
    public static class DropBoxVocabulary
    {
        /// <summary>
        /// Initializes static members of the <see cref="DropBoxVocabulary"/> class.
        /// </summary>
        static DropBoxVocabulary()
        {
            Folder       = new DropBoxFolderVocabulary();
            File         = new DropBoxFileVocabulary();
            SharedFolder = new DropBoxShareFolderVocabulary();
            Group        = new DropBoxGroupVocabulary();
            Member       = new DropBoxMemberVocabulary();
            AccountInfo  = new DropBoxAccountInfoVocabulary();
        }

        public static DropBoxFolderVocabulary Folder { get; private set; }
        public static DropBoxFileVocabulary File { get; private set; }
        public static DropBoxShareFolderVocabulary SharedFolder { get; private set; }
        public static DropBoxGroupVocabulary Group { get; private set; }
        public static DropBoxMemberVocabulary Member { get; private set; }
        public static DropBoxAccountInfoVocabulary AccountInfo { get; private set; }
    }
}