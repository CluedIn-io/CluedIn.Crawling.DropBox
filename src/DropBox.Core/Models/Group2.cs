namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class Group2
    {
        public string group_name { get; set; }
        public string group_id { get; set; }
        public GroupManagementType group_management_type { get; set; }
        public GroupType group_type { get; set; }
        public bool is_member { get; set; }
        public bool is_owner { get; set; }
        public bool same_team { get; set; }
        public int member_count { get; set; }
    }
}