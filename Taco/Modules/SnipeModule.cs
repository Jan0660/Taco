using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revolt;
using Taco.Attributes;
using Taco.CommandHandling;

namespace Taco.Modules
{
    [Summary("Snipe command")]
    [ModuleName("Snipe")]
    public class SnipeModule : ModuleBase
    {
        private static List<Message> _messages = new();
        private static List<Message> _snipedMessages = new();

        [Command("snipe")]
        [Summary("Retrieves deleted messages in current group.")]
        public async Task Snipe()
        {
            var sniped = _snipedMessages.Where(m => m.ChannelId == Message.ChannelId).ToList();
            if (sniped.Count == 0)
            {
                await ReplyAsync("Nothing to snipe here.");
                return;
            }

            sniped = sniped.Count > 5 ? sniped.TakeLast(5).ToList() : sniped;
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