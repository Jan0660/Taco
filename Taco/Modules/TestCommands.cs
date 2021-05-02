using System.IO;
using System.Net;
using System.Threading.Tasks;
using RevoltBot.Attributes;
using RevoltBot.CommandHandling;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace RevoltBot.Modules
{
    [Summary(":flushed:")]
    public class TestModule : ModuleBase
    {
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
> Mention: <@{user._id}>
> Id: `{user._id}`
> Online: {user.Online}
> [\[Default Avatar\]]({user.DefaultAvatarUrl}) [\[Avatar\]]({user.AvatarUrl})");
        }

        [Command("revolt")]
        [Summary("Information about revolt instance.")]
        public async Task RevoltInfo()
        {
            var info = await Message.Client.GetApiInfo();
            var voso = await Message.Client.GetVosoInfo();
            var autumn = await Message.Client.GetAutumnInfo();
            await ReplyAsync(@$"> # Revolt info (for app.revolt.chat)
> ## Versions
> **Api:** {info.Version}
> **Voso:** {voso.Version}
> **Autumn:** {autumn.Version}
> ## Features
> **Registration:** {StringBooled(info.Features.Registration)}
> **Email:** {StringBooled(info.Features.Email)}
> **Invite-only:** {StringBooled(info.Features.InviteOnly)}
> **Captcha:** {StringBooled(info.Features.Captcha.Enabled)}
> **Autumn:** {StringBooled(info.Features.Autumn.Enabled)}
> **Voso:** {StringBooled(info.Features.Voso.Enabled)}
> **Voso.RTP:** {StringBooled(voso.Features.Rtp)}
> **Autumn.JpegQuality:** {autumn.JpegQuality}
> **Maximum Attachment Size:** {autumn.Tags.Attachments.MaxSize / 1000 / 1000}MB");
        }

        public string StringBooled(bool value)
            => StringBooled(value, value.ToString());

        public string StringBooled(bool value, string str)
        {
            return $@"$\color{{{(value ? "lime" : "red")}}}\textsf{{{str}}}$";
        }

        // [Command("discord")]
        // public async Task Discord()
        // {
        //     await Message.Channel.SendFileAsync("bruh", "discord.jpg", @"C:\Users\Jan\Downloads\image0-53 (1).jpg");
        // }

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
    }
}