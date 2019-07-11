using System.Collections.Generic;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class Group
    {
        public AccessType2 access_type { get; set; }
        public Group2 group { get; set; }
        public List<object> permissions { get; set; }
        public bool is_inherited { get; set; }
    }
}