using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Revolt
{
    public class User : RevoltObject
    {
        [JsonProperty("username")] public string Username { get; internal set; }
        [JsonProperty("relations")] public Relation[] Relations { get; internal set; }
        [JsonProperty("status")] public Status Status { get; internal set; }
        [JsonProperty("badges")] public int Badges { get; internal set; }
        [JsonProperty("relationship")] public RelationshipStatus Relationship { get; internal set; }
        [JsonProperty("online")] public bool Online { get; internal set; }
        [JsonProperty("avatar")] public Attachment Avatar { get; internal set; }
        [JsonIgnore] public string DefaultAvatarUrl => $"{Client.ApiUrl}/users/{_id}/default_avatar";
        [JsonIgnore] public string AvatarUrl => $"{Client.AutumnUrl}/{Avatar.Tag}/{Avatar._id}";

        public Task<RelationshipStatus> AddFriendAsync()
            => Client.Users.AddFriendAsync(Username);
        
        public Task<RelationshipStatus> RemoveFriendAsync()
            => Client.Users.RemoveFriendAsync(_id);
        
        public Task<RelationshipStatus> BlockAsync()
            => Client.Users.BlockAsync(_id);
        
        public Task<RelationshipStatus> UnblockAsync()
            => Client.Users.UnblockAsync(_id);

        public override string ToString()
            => Username;
    }
}