using System.Linq;
using Revolt.Channels;

namespace Revolt.Commands
{
    /// <summary> The context of a command which may contain the client, user, guild, channel, and message. </summary>
    public class RevoltCommandContext : ICommandContext
    {
        public string Arguments { get; set; }
        public Message Message { get; }
        public User User { get; }
        public Channel Channel { get; }
        public RevoltClient Client { get; }
        public Server Server { get; }
        public RevoltCommandContext(Message message)
        {
            Arguments = message.Content;
            Message = message;
            User = message.Author;
            Channel = message.Channel;
            Client = message.Client;
            if(Channel is TextChannel textChannel)
                Server = Client.ServersCache.FirstOrDefault(s => s._id == textChannel.ServerId);
        }
    }
}