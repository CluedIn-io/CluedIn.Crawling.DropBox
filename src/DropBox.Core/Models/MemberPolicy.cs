using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class MemberPolicy
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}