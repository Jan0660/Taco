using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class LastMessage : RevoltObject
    {
        [JsonIgnore] public User Author => Client.Users.Get(AuthorId);
        [JsonProperty("author")] public string AuthorId { get; internal set; }
        [JsonProperty("short")] public string Short { get; internal set; }
    }
}