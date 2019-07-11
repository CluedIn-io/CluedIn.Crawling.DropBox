using System.Collections.Generic;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class User
    {
        public AccessType access_type { get; set; }
        public User2 user { get; set; }
        public List<object> permissions { get; set; }
        public bool is_inherited { get; set; }
    }
}