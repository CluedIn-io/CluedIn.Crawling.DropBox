// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropBoxGroupVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the DropBoxGroupVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.Crawling.DropBox.Vocabularies
{
    /// <summary>The drop box group vocabulary.</summary>
    public class DropBoxGroupVocabulary : SimpleVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DropBoxGroupVocabulary"/> class.
        /// </summary>
        public DropBoxGroupVocabulary()
        {
            VocabularyName = "DropBox Group";
            KeyPrefix      = "dropbox.group";
            KeySeparator   = ".";
            Grouping       = EntityType.Infrastructure.Group;
           
            AddGroup("Dropbox Group Details", group =>
            {
                MemberCount = group.Add(new VocabularyKey("memberCount", VocabularyKeyDataType.Number));
                SameTeam    = group.Add(new VocabularyKey("sameTeam", VocabularyKeyDataType.Boolean, VocabularyKeyVisibility.Hidden));
            });
        }

        public VocabularyKey MemberCount { get; set; }

        public VocabularyKey SameTeam { get; set; }

    }
}
