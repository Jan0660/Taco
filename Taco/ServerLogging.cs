using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revolt;
using Revolt.Channels;

namespace Taco
{
    public static class ServerLogging
    {
        public static Dictionary<string, List<Message>> MessageCache = new();

        public static void RegisterEvents()
        {
            Program.Client.MessageReceived += MessageReceived;
            Program.Client.MessageUpdated += MessageUpdated;
            Program.Client.MessageDeleted += MessageDeleted;
            Program.Client.ServerRoleUpdated += ServerRoleUpdated;
        }

        private static Task ServerRoleUpdated(Server server, string roleId, Role old, Role now)
        {
            var serverData = Mongo.GetServerData(server._id);
            if (serverData?.LogChannelId == null)
                return Task.CompletedTask;
            if (old == null)
                // dont replace
                return serverData.LogChannel.SendMessageAsync($@"> ## Role Created
> Id: `{roleId}`
> {Util.Util.TextEmbed(now).Replace("\n", "\n> ")}");
            return serverData.LogChannel.SendMessageAsync($@"> ## Role Updated
> Id: `{roleId}`
> {Util.Util.TextEmbed(old, now).Replace("\n", "\n> ")}");
        }

        private static async Task MessageReceived(Message message)
        {
            if (message.Channel is TextChannel textChannel)
            {
                if (!MessageCache.ContainsKey(textChannel.ServerId))
                    MessageCache.Add(textChannel.ServerId, new());
                MessageCache[textChannel.ServerId].LimitedAdd(message, 5000);
            }
        }

        private static async Task MessageUpdated(string id, MessageEditData editData)
        {
            var server = MessageCache.FirstOrDefault(server => server.Value.Any(msg => msg._id == id));
            if (server.Key == null)
                return;
            var serverData = Mongo.GetServerData(server.Key);
            if (serverData == null | serverData?.LogChannelId == null)
                return;
            var message = server.Value.First(msg => msg._id == id);
            await serverData.LogChannel.SendMessageAsync($@"> ## Message Edited
> by @{message.Author.Username} [{message.AuthorId}]
> Old:
> ```
> {message.Content.Shorten(900).Replace("`", "\u200b`")}
> ```
> New:
> ```
> {editData.Content.Shorten(900).Replace("`", "\u200b`")}
> ```");
        }

        private static async Task MessageDeleted(string id)
        {
            var server = MessageCache.FirstOrDefault(server => server.Value.Any(msg => msg._id == id));
            if (server.Key == null)
                return;
            var serverData = Mongo.GetServerData(server.Key);
            if (serverData == null | serverData?.LogChannelId == null)
                return;
            var message = server.Value.First(msg => msg._id == id);
            await serverData.LogChannel.SendMessageAsync($@"> ## Message Deleted
> by @{message.Author.Username} [{message.AuthorId}]
> Content:
> {message.Content.Shorten(1900).Replace("`", "\u200b`")}");
        }
    }
}