using System.Collections.Generic;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class Invitee
    {
        public AccessType3 access_type { get; set; }
        public Invitee2 invitee { get; set; }
        public List<object> permissions { get; set; }
        public bool is_inherited { get; set; }
    }
}