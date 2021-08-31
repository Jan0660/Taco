using System;
using System.Linq;
using System.Threading.Tasks;
using Revolt;
using Revolt.Channels;

namespace Taco
{
    public class CommandContext
    {
        public UserData UserData
        {
            get
            {
                if (_cachedUserData != null)
                    return _cachedUserData;
                _cachedUserData = Mongo.GetOrCreateUserData(Message.AuthorId);
                return _cachedUserData;
            }
        }

        private UserData _cachedUserData;

        public ServerData ServerData
        {
            get
            {
                if (_cachedServerData != null)
                    return _cachedServerData;
                _cachedServerData = Mongo.GetOrCreateServerData(((TextChannel)Message.Channel).ServerId);
                return _cachedServerData;
            }
        }

        private ServerData _cachedServerData;

        public GroupData GroupData
        {
            get
            {
                if (_cachedGroupData != null)
                    return _cachedGroupData;
                if (Message.Channel is not GroupChannel)
                    throw new Exception("Channel is not a GroupChannel.");
                _cachedGroupData = Mongo.GetOrCreateGroupData(Message.ChannelId);
                return _cachedGroupData;
            }
        }

        private GroupData _cachedGroupData;

        public CommunityData CommunityData =>
            _cachedServerData ?? _cachedGroupData ?? Mongo.GetOrCreateCommunityData(Message.ChannelId,
                Message.Channel is GroupChannel ? CommunityType.Group : CommunityType.Server);

        public Message Message { get; }

        public User User => Message.Author;

        public Server Server =>
            Program.Client.ServersCache.FirstOrDefault(s => s._id == (Message?.Channel as TextChannel)?.ServerId);

        public Channel Channel => Message.Channel;

        public CommandContext(Message message)
        {
            Message = message;
        }

        public UserData GetUserData()
        {
            var data = Mongo.GetUserData(Message.AuthorId);
            if (data != null)
                _cachedUserData = data;
            return data;
        }

        public async Task<ServerPermission> GetServerPermissionsAsync()
        {
            var members = (await Program.Client.Servers.GetMembersAsync(Server._id)).Members;
            var member = members.FirstOrDefault(m => m._id.User == User._id);
            ServerPermission serverPerms = (ServerPermission)Server.DefaultPermissionsRaw[0];
            if (Server.Roles != null)
            {
                var roles = Server.Roles.Where(r => member.Roles.Contains(r.Key));
                foreach (var role in roles)
                {
                    serverPerms = serverPerms | role.Value.ServerPermissions;
                }
            }

            return serverPerms;
        }
    }
}