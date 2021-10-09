using System.Linq;
using System.Threading.Tasks;
using Revolt;
using Revolt.Commands;

namespace Taco.CommandHandling
{
    public class TacoModuleBase : ModuleBase<TacoCommandContext>
    {
        public Message Message => Context.Message;
        public string Args;

        public Task<SelfMessage> ReplyAsync(string content)
            => Context.Client.Channels.SendMessageAsync(Context.Message.ChannelId, content);

        public Task<SelfMessage> InlineReplyAsync(string content, bool mention = false)
            => Message.Channel.SendMessageAsync(content,
                replies: new[] { new MessageReply { Id = Message._id, Mention = mention } });

        public User GetMention(string mention)
        {
            return Message.Client.UsersCache.FirstOrDefault(u => u._id == mention) ??
                   Message.Client.UsersCache.FirstOrDefault(u => u.Username.ToLower() == mention.ToLower()) ??
                   Message.Client.UsersCache.FirstOrDefault(u => u._id == mention.Replace("<@", "").Replace(">", "")) ??
                   Message.Client.Users.Get(mention);
        }
    }
}