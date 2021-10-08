using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Anargy.Attributes;
using Anargy.Revolt.Preconditions;
using Revolt;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Taco.Attributes;
using Taco.CommandHandling;
using Console = Log73.Console;

namespace Taco.Modules
{
    [Name("Test")]
    [Summary(":flushed:")]
    public class TestModule : TacoModuleBase
    {
        // todo: mv
        [Command("whois")]
        [Summary("Retrieve information about a user.")]
        public async Task WhoIs()
        {
            var user = GetMention(Args);
            if (user == null)
            {
                await ReplyAsync(":x: Specify a user by mentioning them, their name or id.");
                return;
            }

            await ReplyAsync($@"> ## {user.Username}
> Mention: [{user.Username}](/@{user._id})
> Id: `{user._id}`
> Online: {user.Online}
> Badges: {user.Badges} ({user.BadgesRaw})
> [\[Default Avatar\]]({user.DefaultAvatarUrl}) [\[Avatar\]]({user.AvatarUrl})");
        }

        [Command("revolt")]
        [Summary("Information about revolt instance.")]
        public async Task RevoltInfo()
        {
            var info = await Message.Client.GetApiInfoAsync();
            VortexInformation vortex = null;
            AutumnInformation autumn = null;
            await Task.WhenAll(Task.Run(async () => autumn = await Message.Client.GetAutumnInfoAsync()),
                Task.Run(async () =>
                    vortex = info.Features.Vortex.Enabled ? await Message.Client.GetVortexInfoAsync() : null));
            var str = new StringBuilder();
            str.AppendLine($"> **Delta v{info.Version}** [\\[URL\\]]({Message.Client.ApiUrl})");
            str.AppendLine(">");
            if ((info.Features.Vortex?.Enabled ?? false) && vortex != null)
            {
                str.AppendLine($"> **Vortex v{vortex.Version}** [\\[URL\\]]({info.Features.Vortex.Url})");
                str.AppendLine(">");
            }

            var autumnTable = new Dictionary<string, AutumnInfoTag>()
            {
                ["Attachments"] = autumn.Tags.Attachments,
                ["Avatars"] = autumn.Tags.Avatars,
                ["Icons"] = autumn.Tags.Icons,
                ["Profile Backgrounds"] = autumn.Tags.Backgrounds,
                ["Server Banners"] = autumn.Tags.Banners
            };
            str.AppendLine($"> **Autumn v{autumn.Version}** [\\[URL\\]]({Message.Client.AutumnUrl})");
            str.AppendLine("> | Tag | Max. Size |");
            str.AppendLine("> |:--- | ----:|");
            foreach (var tag in autumnTable)
            {
                if (!tag.Value.Enabled)
                    continue;
                str.AppendLine($"> | {tag.Key} | {tag.Value.MaxSize / 1000 / 1000}MB |");
            }

            await ReplyAsync(str.ToString());
        }

        public string StringBooled(bool value)
            => StringBooled(value, value.ToString());

        public string StringBooled(bool value, string str)
        {
            return $@"$\color{{{(value ? "lime" : "red")}}}\textsf{{{str}}}$";
        }

        [Command("fuck")]
        [Summary("Fuck someone.")]
        public async Task Fuck()
        {
            var mention = GetMention(Args);
            if (mention == null)
            {
                await ReplyAsync("mention someone h");
                return;
            }

            var web = new WebClient();
            var authorPfp =
                await Image.LoadAsync(new MemoryStream(await web.DownloadDataTaskAsync(Message.Author.AvatarUrl)));
            var mentionPfp =
                await Image.LoadAsync(new MemoryStream(await web.DownloadDataTaskAsync(mention.AvatarUrl)));
            authorPfp.Mutate(c => c.Resize(new Size(166, 166)));
            mentionPfp.Mutate(c => c.Resize(new Size(110, 110)));
            var image = await Image.LoadAsync(@"./Resources/Fuck.png");
            image.Mutate(c =>
            {
                c.DrawImage(authorPfp, new Point(117, 65), 1.0f);
                c.DrawImage(mentionPfp, new Point(140, 330), 1.0f);
            });
            await Message.Channel.SendPngAsync(image, "get fucked nerd");
        }

        // [Command("jan")]
        // public async Task Jan()
        // {
        //     var web = new WebClient();
        //     await Message.Channel.SendFileAsync("jan", "jan.png",
        //         await web.DownloadDataTaskAsync(
        //             "https://cdn.discordapp.com/attachments/803693023661522966/816394428176138250/motivate.png"));
        // }
        //
        // [Command("unfriend")]
        // public async Task UnfriendMePls()
        // {
        //     if (Message.AuthorId != "01EX40TVKYNV114H8Q8VWEGBWQ")
        //     {
        //         await ReplyAsync("cock");
        //         return;
        //     }
        //
        //     Console.WriteLine("h1");
        //     await Message.Client.Users.RemoveFriendAsync(Message.AuthorId);
        //
        //     Console.WriteLine("h2");
        //     await ReplyAsync("get kek'd");
        // }

        [Command("leave")]
        [Summary("Leaves the group.")]
        [GroupOnly]
        [RequireGroupOwner]
        public async Task LeaveChannel()
        {
            await Message.Client.Channels.LeaveAsync(Message.ChannelId);
        }

        // [Command("hspace")]
        // public async Task HSpace()
        // {
        //     var text = "";
        //     for (int i = 0; i < 30; i++)
        //     {
        //         text += $"$\\hspace{{{i}cm}}$ >\n";
        //     }
        //
        //     await ReplyAsync(text);
        // }
        [Command("roadmap")]
        public Task Roadmap()
            => ReplyAsync("Revolt's roadmap is available here: https://revolt.chat/roadmap");

        [Command("weblate")]
        public Task Weblate()
            => ReplyAsync("https://weblate.insrt.uk/engage/revolt/?utm_source=taco");

        [Command("session")]
        public Task Session()
            => ReplyAsync("https://rvf.geist.ga/posts/Getting-Session-Data");

        [Command("gitlab")]
        public Task GitLab()
            => ReplyAsync("https://gitlab.insrt.uk/revolt");

        [Command("coc")]
        public Task FullCodeOfConduct() => ReplyAsync("> " + string.Join("\n> ", Program.Config.CodeOfConduct));

        [Command("permissions")]
        [Alias("perms")]
        public async Task PermissionTest()
        {
            var perms = Context.Server.GetPermissionsFor(Context.User._id);

            StringBuilder res = new();
            res.Append("> ## Server Permissions\n");
            foreach (var enumVal in Enum.GetValues<ServerPermission>())
            {
                res.AppendLine("> " + (perms.Server.HasFlag(enumVal) ? ":white_check_mark:" : ":x:") +
                               $" {enumVal.ToString()}");
            }

            res.Append("> ## Channel Permissions\n");
            foreach (var enumVal in Enum.GetValues<ChannelPermission>())
            {
                res.AppendLine("> " + (perms.Channel.HasFlag(enumVal) ? ":white_check_mark:" : ":x:") +
                               $" {enumVal.ToString()}");
            }
            // todo: this channel permissions

            await ReplyAsync(res.ToString());
        }

        [Command("prectest")]
        [RequireServerPermissions(ServerPermission.BanMembers | ServerPermission.ManageServer)]
        public Task PreconditionTest()
            => InlineReplyAsync("test maybe worked?");

        [Command("htest")]
        public Task HTest()
        {
            return ReplyAsync(Context.Message.Replies[0]);
        }
    }
}