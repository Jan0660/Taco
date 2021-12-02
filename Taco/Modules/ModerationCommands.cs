using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Revolt;
using Revolt.Commands.Attributes;
using Revolt.Commands.Attributes.Preconditions;
using Taco.Attributes;
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
        [RequireServerModerator]
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
        [RequireServerPermissions(ServerPermission.ManageServer)]
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

        // todo: add a way to remove a deleted role from the mod role list
        [Command("mod add role")]
        [TextChannelOnly]
        [Summary("Add a role to the mod list.")]
        [RequireServerPermissions(ServerPermission.ManageServer)]
        public async Task AddModRole(Role role)
        {
            // todo: add role id to Role object in Revolt.Net
            Context.ServerData.ModRoles.Add(Context.Server.Roles.First(r => r.Value == role).Key);
            await Context.UpdateCommunityDataAsync();
            await ReplyAsync($"Added role `{role.Name}` to the mod list.");
        }

        [Command("mod rm role")]
        [TextChannelOnly]
        [Summary("Remove a role from the mod list.")]
        [RequireServerPermissions(ServerPermission.ManageServer)]
        public async Task RemoveModRole(Role role)
        {
            Context.ServerData.ModRoles.Remove(Context.Server.Roles.First(r => r.Value == role).Key);
            await Context.UpdateCommunityDataAsync();
            await ReplyAsync($"Removed role `{role.Name}` from the mod list.");
        }

        [Command("mod list")]
        [TextChannelOnly]
        [Summary("View the mod list.")]
        [RequireServerPermissions(ServerPermission.ManageServer)]
        public async Task ModList()
        {
            var str = new StringBuilder();
            str.AppendLine("## Mod List");
            if (Context.ServerData.ModRoles.Count != 0)
            {
                str.AppendLine("### Roles");
                foreach (var r in Context.Server.Roles)
                    if (Context.ServerData.ModRoles.Contains(r.Key))
                        str.AppendLine($"* {r.Value.Name} [`{r.Key}`]");
            }

            await ReplyAsync(str.ToString());
        }
    }
}