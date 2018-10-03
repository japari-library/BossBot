using Newtonsoft.Json;

namespace NadekoBot.Core.Common.Pokemon
{
    public class FriendsNameId
    {
		[JsonProperty("name")]
        public string Name { get; set; }
		
		[JsonProperty("triviaid")]
        public string Triviaid { get; set; }
    }
}