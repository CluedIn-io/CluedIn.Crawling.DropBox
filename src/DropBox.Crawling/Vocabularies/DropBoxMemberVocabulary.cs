// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropBoxMemberVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the DropBoxMemberVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.Crawling.DropBox.Vocabularies
{
    /// <summary>The drop box member vocabulary.</summary>
    public class DropBoxMemberVocabulary : SimpleVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DropBoxMemberVocabulary"/> class.
        /// </summary>
        public DropBoxMemberVocabulary()
        {
            VocabularyName = "DropBox Member";
            KeyPrefix      = "dropbox.member";
            KeySeparator   = ".";
            Grouping       = EntityType.Account;

            AddGroup("Dropbox Member Details", group =>
            {
                Active = group.Add(new VocabularyKey("active", VocabularyKeyDataType.Boolean));
                UId    = group.Add(new VocabularyKey("uid", VocabularyKeyVisiblity.Hidden));
            });
        }

        public VocabularyKey UId { get; set; }

        public VocabularyKey Active { get; set; }

    }
}
