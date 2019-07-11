// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropBoxAccountInfoVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the DropBoxAccountInfoVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.Crawling.DropBox.Vocabularies
{
    /// <summary>The drop box account info vocabulary.</summary>
    /// <seealso cref="CluedIn.Core.Data.Vocabularies.SimpleVocabulary" />
    public class DropBoxAccountInfoVocabulary : SimpleVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DropBoxAccountInfoVocabulary"/> class.
        /// </summary>
        public DropBoxAccountInfoVocabulary()
        {
            VocabularyName = "DropBox Account";
            KeyPrefix      = "dropbox.account";
            KeySeparator   = ".";
            Grouping       = EntityType.Infrastructure.User;

            AddGroup("Dropbox Account Details", group =>
            {
                TeamName        = group.Add(new VocabularyKey("teamName"));
                FamiliarName    = group.Add(new VocabularyKey("familiar_name"));
                ReferredLink    = group.Add(new VocabularyKey("referredLink", VocabularyKeyVisiblity.Hidden));
                QuotaInfoNormal = group.Add(new VocabularyKey("quotaInfoNormal", VocabularyKeyVisiblity.Hidden));
                QuotaInfoQuota  = group.Add(new VocabularyKey("quotaInfoQuota", VocabularyKeyVisiblity.Hidden));
                QuotaInfoShared = group.Add(new VocabularyKey("quotaInfoShared", VocabularyKeyVisiblity.Hidden));
                Locale          = group.Add(new VocabularyKey("locale", VocabularyKeyVisiblity.Hidden));
                IsPaired        = group.Add(new VocabularyKey("isPaired", VocabularyKeyVisiblity.Hidden));
            });

            GivenName = Add(new VocabularyKey("givenName"));
            Surname   = Add(new VocabularyKey("SurName"));
            Country   = Add(new VocabularyKey("Country"));
            Email     = Add(new VocabularyKey("Email"));

            AddMapping(GivenName,     CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.FirstName);
            AddMapping(Surname,       CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.LastName);
            AddMapping(Email,         CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.Email);
            AddMapping(Country,       CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.HomeAddressCountryCode);
        }

        public VocabularyKey FamiliarName { get; set; }

        public VocabularyKey IsPaired { get; set; }

        public VocabularyKey ReferredLink { get; set; }

        public VocabularyKey TeamName { get; set; }

        public VocabularyKey QuotaInfoNormal { get; set; }
        
        public VocabularyKey QuotaInfoQuota { get; set; }
        
        public VocabularyKey QuotaInfoShared { get; set; }

        public VocabularyKey Locale { get; set; }

        public VocabularyKey GivenName { get; set; }

        public VocabularyKey Surname { get; set; }

        public VocabularyKey Country { get; set; }

        public VocabularyKey Email { get; set; }
    }
}
