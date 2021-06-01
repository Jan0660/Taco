using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class SavedMessagesChannel : Channel
    {
        [JsonProperty("user")] public string UserId { get; internal set; }
    }
}