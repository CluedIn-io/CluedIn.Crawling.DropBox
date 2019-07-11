using System;
using System.Collections.Generic;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class FolderList
    {
        public List<Entry> entries { get; set; }
        public string cursor { get; set; }
    }
}
