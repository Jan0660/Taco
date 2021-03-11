using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RevoltApi;
using RevoltBot.Attributes;

namespace RevoltBot.Modules
{
    public class SnipeModule : ModuleBase
    {
        private static List<Message> _messages = new();
        private static List<Message> _snipedMessages = new();

        [Command("snipe")]
        public async Task Snipe()
        {
            var sniped = _snipedMessages.Where(m => m.ChannelId == Message.ChannelId);
            var enumerable = sniped.ToList();
            if (enumerable.Count == 0)
            {
                await ReplyAsync("Nothing to snipe here.");
                return;
            }

            sniped = enumerable.Count() > 5 ? enumerable.TakeLast(5) : enumerable;
            int i = 0;
            var msg = "";
            foreach (var snipe in sniped)
            {
                msg += $@"> ### <@{snipe.AuthorId}>
> ```
> {snipe.Content.Replace("\n", "\n> ").Replace("```", "\u200b`\u200b`\u200b`")}
> ```
> {"\u200b"}
";
                i++;
                if (i == 5)
                    break;
            }

            await ReplyAsync(msg);
        }

        public static void Init(RevoltClient client)
        {
            client.MessageDeleted += ClientOnMessageDeleted;
            client.MessageReceived += ClientOnMessageReceived;
        }

        private static Task ClientOnMessageDeleted(string id)
        {
            var message = _messages.FirstOrDefault(m => m._id == id);
            if (message == null)
                return Task.CompletedTask;
            _snipedMessages.Add(message);
            if (_snipedMessages.Count == 100)
                _snipedMessages.Remove(_snipedMessages.Last());
            return Task.CompletedTask;
        }

        private static Task ClientOnMessageReceived(Message message)
        {
            _messages.Add(message);
            if (_messages.Count == 100)
                _messages.Remove(_messages.Last());
            return Task.CompletedTask;
        }
    }
}