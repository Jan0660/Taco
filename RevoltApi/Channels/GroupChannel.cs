using Newtonsoft.Json;

namespace RevoltApi.Channels
{
    public class GroupChannel : MessageChannel
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("owner")] public string OwnerId;
        [JsonProperty("description")] public string Description;
    }
}