using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Revolt;
using Revolt.Commands.Attributes;
using Revolt.Commands.Attributes.Preconditions;
using Taco.CommandHandling;

namespace Taco.Modules
{
    [Name("Moderation")]
    [Summary("Commands for community moderation/administration and settings.")]
    public class ModerationCommands : TacoModuleBase
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

        [Command("tag")]
        [Summary("Get a tag.")]
        [Alias("tags", "t")]
        public Task GetTag([Name("tag")] string? tagName)
        {
            if (Context.CommunityData.Tags == null)
                return InlineReplyAsync("This server doesn't have any tags defined!");
            var tag = Context.CommunityData.Tags.FirstOrDefault(t =>
                t.Key.Equals(tagName, StringComparison.InvariantCultureIgnoreCase));
            if (tag.Value != null)
                return InlineReplyAsync(tag.Value);
            if (tagName?.ToLower() == "list" || tagName is null or "")
                return ListTags();
            return InlineReplyAsync("Tag not found!", true);
        }

        [Command("list tag")]
        [Summary("List tags.")]
        public Task ListTags()
        {
            var res = new StringBuilder();
            foreach (var tag in Context.CommunityData.Tags)
            {
                res.Append($"> `{tag.Value}`: ");
                if (tag.Value.Contains('\n'))
                    res.AppendLine(tag.Value.Replace("\n", "\n> > "));
                else
                    res.AppendLine(tag.Value);
            }

            return ReplyAsync(res.ToString());
        }

        [Command("add tag")]
        [Summary("Add a tag.")]
        [RequireServerPermissions(ServerPermission.ManageServer)]
        public async Task AddTag(string name, [Remainder] string content)
        {
            if (Context.CommunityData.Tags.Any(t =>
                t.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                await ReplyAsync("This tag already exists!");
                return;
            }

            Context.CommunityData.Tags ??= new();
            Context.CommunityData.Tags.Add(name, content);
            await Context.UpdateCommunityDataAsync();
            await ReplyAsync($"Added tag by the name of `{name}`.");
        }

        [Command("setprefix")]
        [Summary("Set the prefix in this server.")]
        public async Task SetPrefix(string newPrefix)
        {
            Context.CommunityData.CustomPrefix = newPrefix;
            await Context.UpdateCommunityDataAsync();
            await InlineReplyAsync($"Prefix changed to `{newPrefix}`!");
        }

        [Command("ban")]
        [Summary("Ban a user.")]
        [TextChannelOnly]
        [RequireServerPermissions(ServerPermission.BanMembers)]
        [RequireBotServerPermission(ServerPermission.BanMembers)]
        public async Task Ban(User user, string reason = null)
        {
            await Context.Client.Servers.BanMemberAsync(Context.Server._id, user._id, reason);
            await ReplyAsync("User banned.");
        }

        [Command("ban")]
        [Summary("Ban a user by id.")]
        [TextChannelOnly]
        [RequireServerPermissions(ServerPermission.BanMembers)]
        [RequireBotServerPermission(ServerPermission.BanMembers)]
        public async Task Ban(string userId, string reason = null)
        {
            await Context.Client.Servers.BanMemberAsync(Context.Server._id, userId, reason);
            await ReplyAsync("User banned.");
        }

        [Command("unban")]
        [Summary("Unban a user by id.")]
        [TextChannelOnly]
        [RequireServerPermissions(ServerPermission.BanMembers)]
        [RequireBotServerPermission(ServerPermission.BanMembers)]
        public async Task Unban(string userId)
        {
            var res = await Context.Client.Servers.UnbanMemberAsync(Context.Server._id, userId);
            if (res.StatusCode == HttpStatusCode.NotFound)
            {
                await ReplyAsync("User is not banned.");
                return;
            }
            await ReplyAsync("User unbanned.");
        }
    }
}