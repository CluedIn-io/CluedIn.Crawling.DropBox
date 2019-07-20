using System;
using System.Globalization;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Crawling.Factories;
using Dropbox.Api.Team;

namespace CluedIn.Crawling.DropBox.ClueProducers
{
    public class MembershipClueProducer : BaseClueProducer<GroupMemberInfo>
    {
        private readonly IClueFactory _factory;

        public MembershipClueProducer([NotNull] IClueFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        protected override Clue MakeClueImpl(GroupMemberInfo input, Guid accountId)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var clue = _factory.Create(EntityType.Account, input.Profile.TeamMemberId.ToString(CultureInfo.InvariantCulture), accountId);

            var data = clue.Data.EntityData;

            if (input.Profile.Name?.DisplayName != null)
            {
                data.Name = input.Profile.Name.DisplayName;
                data.DisplayName = input.Profile.Name.DisplayName;
            }

            return clue;
        }
    }
}
