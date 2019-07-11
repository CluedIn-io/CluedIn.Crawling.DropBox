using System.Collections.Generic;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class Entry
    {
        public AccessType access_type { get; set; }
        public bool is_inside_team_folder { get; set; }
        public bool is_team_folder { get; set; }
        public string name { get; set; }
        public Policy policy { get; set; }
        public string preview_url { get; set; }
        public string shared_folder_id { get; set; }
        public string time_invited { get; set; }
        public string path_lower { get; set; }
        public LinkMetadata link_metadata { get; set; }
        public List<object> permissions { get; set; }
    }
}