using Newtonsoft.Json;
using Revolt.Channels;

namespace Revolt
{
    public class Message : RevoltObject
    {
        [JsonProperty("nonce")] public string? Nonce;
        [JsonProperty("channel")] public string ChannelId;
        [JsonIgnore] public Channel Channel => Client.Channels.Get(ChannelId);
        [JsonProperty("author")] public string AuthorId;
        [JsonIgnore] public User Author => Client.Users.Get(AuthorId);
        [JsonProperty("content")] public string Content;
        [JsonProperty("attachments")] public Attachment[]? Attachments;
        // todo: edited?
    }
}