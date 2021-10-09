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
    }
}