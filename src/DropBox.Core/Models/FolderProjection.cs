
using System.Collections.Generic;
using CluedIn.Providers.Webhooks.Models;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class FolderProjection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }
        public bool Sensitive { get; set; }
        public List<CluedInPermission> Permissions { get; set; }
        public bool Active { get; set; }
    }
}
