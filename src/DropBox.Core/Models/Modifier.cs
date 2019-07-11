using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class Modifier
    {
        public bool email_verified { get; set; }
        public bool same_team { get; set; }
        public string display_name { get; set; }
        public int uid { get; set; }
        public string email { get; set; }
    }
}
