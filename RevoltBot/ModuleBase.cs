using System.Threading.Tasks;
using RevoltApi;

namespace RevoltBot
{
    public class ModuleBase
    {
        public Message Message;
        public string Args;

        public Task<Message> ReplyAsync(string content)
            => Message.Channel.SendMessageAsync(content);
    }
}