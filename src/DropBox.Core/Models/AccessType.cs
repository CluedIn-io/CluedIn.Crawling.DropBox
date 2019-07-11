using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class AccessType
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}