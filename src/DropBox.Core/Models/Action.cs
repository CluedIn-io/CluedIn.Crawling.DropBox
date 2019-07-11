using Newtonsoft.Json;

namespace CluedIn.Crawling.DropBox.Core.Models
{
    public class Action
    {
        [JsonProperty(".tag")]
        public string tag { get; set; }
    }
}