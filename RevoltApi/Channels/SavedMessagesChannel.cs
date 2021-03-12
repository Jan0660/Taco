using Newtonsoft.Json;

namespace RevoltApi.Channels
{
    public class SavedMessagesChannel : Channel
    {
        [JsonProperty("user")] public string UserId;
    }
}