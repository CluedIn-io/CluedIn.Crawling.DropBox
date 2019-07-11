using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class CurrentAudience
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}