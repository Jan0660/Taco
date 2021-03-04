using System;
using System.ComponentModel.Design;
using Newtonsoft.Json;
using RevoltApi.Channels;

namespace RevoltApi
{
    public class Message : RevoltObject
    {
        [JsonProperty("nonce")] public string? Nonce;
        [JsonProperty("channel")] public string ChannelId;
        [JsonIgnore] public Channel Channel => Client.GetChannel(ChannelId);
        [JsonProperty("author")] public string AuthorId;
        [JsonIgnore] public User Author => Client.GetUser(AuthorId);
        [JsonProperty("content")] public string Content;
        [JsonProperty("attachment")] public Attachment? Attachment;
        // todo: edited?
    }
}