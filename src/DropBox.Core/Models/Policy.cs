namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class Policy
    {
        public AclUpdatePolicy acl_update_policy { get; set; }
        public SharedLinkPolicy shared_link_policy { get; set; }
        public MemberPolicy member_policy { get; set; }
        public ResolvedMemberPolicy resolved_member_policy { get; set; }
    }
}