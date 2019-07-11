using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class AudienceOption
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}