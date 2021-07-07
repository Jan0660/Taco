using System.Linq;
using System.Threading.Tasks;
using Revolt;

namespace Taco.CommandHandling
{
    public class ModuleBase
    {
        public Message Message;
        public string Args;
        public CommandContext Context;

        internal void SetContext(CommandContext context)
        {
            Context = context;
        }

        public Task<SelfMessage> ReplyAsync(string content)
            => Message.Channel.SendMessageAsync(content);

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