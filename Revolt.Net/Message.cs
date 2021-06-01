using Newtonsoft.Json;
using Revolt.Channels;

namespace Revolt
{
    public class Message : RevoltObject
    {
        [JsonProperty("nonce")] public string? Nonce { get; private set; }
        [JsonProperty("channel")] public string ChannelId { get; private set; }
        [JsonIgnore] public Channel Channel => Client.Channels.Get(ChannelId);
        [JsonProperty("author")] public string AuthorId { get; private set; }
        [JsonIgnore] public User Author => Client.Users.Get(AuthorId);
        [JsonProperty("content")] public string Content { get; internal set; }

        [JsonProperty("attachments")] public Attachment[]? Attachments;
        // todo: edited?
    }
}