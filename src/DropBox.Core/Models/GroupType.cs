using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class GroupType
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}