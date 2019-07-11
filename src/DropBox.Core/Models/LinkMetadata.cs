using System.Collections.Generic;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class LinkMetadata
    {
        public List<AudienceOption> audience_options { get; set; }
        public CurrentAudience current_audience { get; set; }
        public List<LinkPermission> link_permissions { get; set; }
        public bool password_protected { get; set; }
        public string url { get; set; }
    }
}