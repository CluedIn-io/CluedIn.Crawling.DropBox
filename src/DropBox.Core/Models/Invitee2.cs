using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class Invitee2
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
        public string email { get; set; }
    }
}