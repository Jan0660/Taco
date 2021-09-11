using System;
using System.Linq;
using System.Threading.Tasks;
using Revolt;
using Revolt.Channels;

namespace Anargy.Revolt
{
    public class RevoltCommandContext : CommandContext
    {
        public Message Message { get; }
        public User User { get; }
        public Channel Channel { get; }
        public RevoltClient Client { get; }
        public Server Server { get; }
        public RevoltCommandContext(Message message) : base(message.Content)
        {
            Message = message;
            User = message.Author;
            Channel = message.Channel;
            Client = message.Client;
            if(Channel is TextChannel textChannel)
                Server = Client.ServersCache.FirstOrDefault(s => s._id == textChannel.ServerId);
        }

        public async Task<ServerPermission> GetServerPermissionsAsync()
        {
            var members = (await Client.Servers.GetMembersAsync(Server._id)).Members;
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