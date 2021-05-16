using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class GroupChannel : MessageChannel
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("owner")] public string OwnerId;
        [JsonProperty("description")] public string Description;

        public Task AddMemberAsync(string userId)
            => Client.Channels.AddGroupMemberAsync(_id, userId);

        public Task RemoveMemberAsync(string userId)
            => Client.Channels.RemoveGroupMemberAsync(_id, userId);

        public Task LeaveAsync()
            => Client.Channels.LeaveAsync(_id);

        public override string ToString()
            => Name;
    }
}