using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RevoltApi
{
    public class User : RevoltObject
    {
        [JsonProperty("username")] public string Username;
        [JsonProperty("relations")] public Relation[] Relations;
        [JsonProperty("status")] public Status Status;
        [JsonProperty("badges")] public int Badges;
        [JsonProperty("relationship")] public RelationshipStatus Relationship;
        [JsonProperty("online")] public bool Online;
        [JsonProperty("avatar")] public Attachment Avatar;
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