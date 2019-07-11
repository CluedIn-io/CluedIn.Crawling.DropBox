using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class AccessType3
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}