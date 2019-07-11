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

        protected override Clue MakeClueImpl(GroupMemberInfo value, Guid accountId)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var clue = _factory.Create(EntityType.Account, value.Profile.TeamMemberId.ToString(CultureInfo.InvariantCulture), accountId);

            var data = clue.Data.EntityData;

            if (value.Profile.Name?.DisplayName != null)
            {
                data.Name = value.Profile.Name.DisplayName;
                data.DisplayName = value.Profile.Name.DisplayName;
            }

            return clue;
        }
    }
}
