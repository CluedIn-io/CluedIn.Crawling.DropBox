using System;
using CluedIn.Core.Data;
using CluedIn.Crawling.Factories;
using CluedIn.Crawling.Helpers;

using CluedIn.Crawling.DropBox.Vocabularies;
using Dropbox.Api.Users;

namespace CluedIn.Crawling.DropBox.ClueProducers
{
    public class AccountInfoClueProducer : BaseClueProducer<FullAccount>
    {
        private readonly IClueFactory _factory;

        public AccountInfoClueProducer(IClueFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        protected override Clue MakeClueImpl(FullAccount value, Guid accountId)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var clue = _factory.Create(EntityType.Infrastructure.User, value.AccountId, accountId);

            // TODO: Populate clue data
            var data = clue.Data.EntityData;

            if (value.Name != null)
            {
                if (value.Name.DisplayName != null)
                {
                    data.Name = value.Name.DisplayName;
                    data.DisplayName = value.Name.DisplayName;
                }

                data.Properties[DropBoxVocabulary.AccountInfo.FamiliarName] = value.Name.FamiliarName.PrintIfAvailable();
                data.Properties[DropBoxVocabulary.AccountInfo.GivenName] = value.Name.GivenName.PrintIfAvailable();
                data.Properties[DropBoxVocabulary.AccountInfo.Surname] = value.Name.Surname.PrintIfAvailable();
            }

            data.Properties[DropBoxVocabulary.AccountInfo.IsPaired] = value.IsPaired.PrintIfAvailable();
            
            data.Properties[DropBoxVocabulary.AccountInfo.Locale] = value.Locale.PrintIfAvailable();
            data.Properties[DropBoxVocabulary.AccountInfo.Country] = value.Country.PrintIfAvailable();
            data.Properties[DropBoxVocabulary.AccountInfo.Email] = value.Email.PrintIfAvailable();
            data.Properties[DropBoxVocabulary.AccountInfo.ReferredLink] = value.ReferralLink.PrintIfAvailable();


            if (! string.IsNullOrEmpty(value.Team?.Name))
                data.Properties[DropBoxVocabulary.AccountInfo.TeamName] = value.Team.Name;

            return clue;
        }
    }
}
