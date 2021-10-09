using Revolt.Channels;

namespace Revolt.Commands
{
    /// <summary>
    ///     Represents a context of a command. This may include the client, guild, channel, user, and message.
    /// </summary>
    public interface ICommandContext
    {
        public string Arguments { get; set; }
        public Message Message { get; }
        public User User { get; }
        public Channel Channel { get; }
        public RevoltClient Client { get; }
        public Server Server { get; }
    }
}