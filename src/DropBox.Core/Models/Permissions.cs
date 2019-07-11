using System.Collections.Generic;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class Permissions
    {
        public List<User> users { get; set; }
        public List<Group> groups { get; set; }
        public List<Invitee> invitees { get; set; }
        public string cursor { get; set; }
    }
}