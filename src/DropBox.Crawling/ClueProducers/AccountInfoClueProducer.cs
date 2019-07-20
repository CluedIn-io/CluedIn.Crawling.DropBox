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

        protected override Clue MakeClueImpl(FullAccount input, Guid accountId)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var clue = _factory.Create(EntityType.Infrastructure.User, input.AccountId, accountId);

            // TODO: Populate clue data
            var data = clue.Data.EntityData;

            if (input.Name != null)
            {
                if (input.Name.DisplayName != null)
                {
                    data.Name = input.Name.DisplayName;
                    data.DisplayName = input.Name.DisplayName;
                }

                data.Properties[DropBoxVocabulary.AccountInfo.FamiliarName] = input.Name.FamiliarName.PrintIfAvailable();
                data.Properties[DropBoxVocabulary.AccountInfo.GivenName] = input.Name.GivenName.PrintIfAvailable();
                data.Properties[DropBoxVocabulary.AccountInfo.Surname] = input.Name.Surname.PrintIfAvailable();
            }

            data.Properties[DropBoxVocabulary.AccountInfo.IsPaired] = input.IsPaired.PrintIfAvailable();
            
            data.Properties[DropBoxVocabulary.AccountInfo.Locale] = input.Locale.PrintIfAvailable();
            data.Properties[DropBoxVocabulary.AccountInfo.Country] = input.Country.PrintIfAvailable();
            data.Properties[DropBoxVocabulary.AccountInfo.Email] = input.Email.PrintIfAvailable();
            data.Properties[DropBoxVocabulary.AccountInfo.ReferredLink] = input.ReferralLink.PrintIfAvailable();


            if (! string.IsNullOrEmpty(input.Team?.Name))
            {
                data.Properties[DropBoxVocabulary.AccountInfo.TeamName] = input.Team.Name;
            }

            return clue;
        }
    }
}
