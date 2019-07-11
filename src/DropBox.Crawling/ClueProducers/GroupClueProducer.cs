using System;
using System.Globalization;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Crawling.DropBox.Vocabularies;
using CluedIn.Crawling.Factories;
using Dropbox.Api.Team;

namespace CluedIn.Crawling.DropBox.ClueProducers
{
    public class GroupClueProducer : BaseClueProducer<GroupFullInfo>
    {
        private readonly IClueFactory _factory;

        public GroupClueProducer([NotNull] IClueFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        protected override Clue MakeClueImpl(GroupFullInfo input, Guid accountId)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var clue = _factory.Create(EntityType.Infrastructure.Group, input.GroupId.ToString(CultureInfo.InvariantCulture), accountId);

            var data = clue.Data.EntityData;

            if (input.GroupName != null)
            {
                data.Name = input.GroupName;
                data.DisplayName = input.GroupName;
            }

            if (input.MemberCount != null)
                data.Properties[DropBoxVocabulary.Group.MemberCount] = input.MemberCount.Value.ToString(CultureInfo.InvariantCulture);

            return clue;
        }
    }
}
