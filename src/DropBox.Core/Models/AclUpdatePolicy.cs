using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class AclUpdatePolicy
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}