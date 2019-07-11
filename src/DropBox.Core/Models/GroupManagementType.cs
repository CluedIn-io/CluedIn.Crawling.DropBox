using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class GroupManagementType
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}