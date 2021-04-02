using System.Linq;
using System.Threading.Tasks;
using RevoltApi;

namespace RevoltBot.CommandHandling
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

        public User GetMention(string mention)
        {
            return Message.Client.UsersCache.FirstOrDefault(u => u._id == mention) ??
                   Message.Client.UsersCache.FirstOrDefault(u => u.Username.ToLower() == mention.ToLower()) ??
                   Message.Client.UsersCache.FirstOrDefault(u => u._id == mention.Replace("<@", "").Replace(">", "")) ??
                   Message.Client.Users.Get(mention);
        }
    }
}