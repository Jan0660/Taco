using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class DirectMessageChannel : MessageChannel
    {
        [JsonProperty("active")] public bool Active { get; internal set; }

        [JsonIgnore]
        public string OtherUserId => RecipientIds[0] != Client.User._id ? RecipientIds[0] : RecipientIds[1];

        [JsonIgnore] public User OtherUser => Client.Users.Get(OtherUserId);
    }
}