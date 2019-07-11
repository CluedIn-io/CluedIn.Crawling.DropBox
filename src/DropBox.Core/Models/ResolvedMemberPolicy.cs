using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class ResolvedMemberPolicy
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}