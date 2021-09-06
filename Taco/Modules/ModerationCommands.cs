using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Revolt;
using Taco.Attributes;
using Taco.CommandHandling;

namespace Taco.Modules
{
    [Summary("Commands for community moderation/administration and settings.")]
    public class ModerationCommands : ModuleBase
    {
        [Command("logchannel")]
        [TextChannelOnly]
        [RequireServerPermissions(ServerPermission.ManageServer)]
        public async Task SetLogChannel()
        {
            Context.ServerData.LogChannelId = Message.ChannelId;
            await Context.ServerData.UpdateAsync();
            await ReplyAsync("log channel set");
        }

        [Command("tag", "t")]
        public Task GetTag()
        {
            if (Context.CommunityData.Tags == null)
                return InlineReplyAsync("This server doesn't have any tags defined!");
            var tag = Context.CommunityData.Tags.FirstOrDefault(t =>
                t.Key.Equals(Args, StringComparison.InvariantCultureIgnoreCase));
            if (tag.Value != null)
                return InlineReplyAsync(tag.Value);
            return InlineReplyAsync("Tag not found!", true);
        }

        [Command("add tag")]
        [RequireServerPermissions(ServerPermission.ManageServer)]
        public async Task AddTag()
        {
            var split = Args.Split(' ');
            if (Context.CommunityData.Tags.Any(t =>
                t.Key.Equals(split[0], StringComparison.InvariantCultureIgnoreCase)))
            {
                await ReplyAsync("This tag already exists!");
                return;
            }
            Context.CommunityData.Tags ??= new();
            Context.CommunityData.Tags.Add(split[0], string.Join(' ', split[1..]));
            await Context.UpdateCommunityDataAsync();
            await ReplyAsync($"Added tag by the name of `{split[0]}`.");
        }
    }
}