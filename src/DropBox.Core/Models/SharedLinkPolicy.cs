using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class SharedLinkPolicy
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}