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

        public Task<Message> ReplyAsync(string content)
            => Message.Channel.SendMessageAsync(content);
    }
}