using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;
using RevoltApi;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Console = Log73.Console;

namespace RevoltBot.Modules
{
    public class TestModule : ModuleBase
    {
        [Command("test", "test-alias")]
        public async Task TestCommand()
        {
            await ReplyAsync("test");
        }

        [Command("help", "help-alias")]
        public async Task HelpCommand()
        {
            await ReplyAsync("lol fuck you");
        }

        [Command("whois")]
        public async Task WhoIs()
        {
            var user = GetMention(Args);
            if (user == null)
            {
                await ReplyAsync(":x: Specify a user by mentioning them, their name or id.");
                return;
            }

            await ReplyAsync($@"> ## {"\u200b"} {user.Username}
> Mention: <@{user._id}>
> Id: `{user._id}`
> Online: {user.Online}
> [\[Default Avatar\]]({user.DefaultAvatarUrl}) [\[Avatar\]]({user.AvatarUrl})");
        }

        [Command("revolt")]
        public async Task RevoltInfo()
        {
            var info = await Message.Client.GetApiInfo();
            await ReplyAsync(@$"> ## Revolt info (for app.revolt.chat)
> ### Versions
> **Api:** {info.Version}
> ### Features
> **Email:** {info.Features.Email}
> **Invite-only:** {info.Features.InviteOnly}
> **Captcha:** {info.Features.Captcha.Enabled}");
        }

        [Command("discord")]
        public async Task Discord()
        {
            await Message.Channel.SendFileAsync("bruh", "discord.jpg", @"C:\Users\Jan\Downloads\image0-53 (1).jpg");
        }

        [Command("fuck")]
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
            // todo: resource folder or some dumb shit
            var image = await Image.LoadAsync(@"C:\Users\Jan\source\repos\ElseIfBot\Resources\Fuck.png");
            image.Mutate(c =>
            {
                c.DrawImage(authorPfp, new Point(117, 65), 1.0f);
                c.DrawImage(mentionPfp, new Point(140, 330), 1.0f);
            });
            await Message.Channel.SendPngAsync(image, "get fucked nerd");
        }

        [Command("jan")]
        public async Task Jan()
        {
            var web = new WebClient();
            await Message.Channel.SendFileAsync("jan", "jan.png",
                await web.DownloadDataTaskAsync(
                    "https://cdn.discordapp.com/attachments/803693023661522966/816394428176138250/motivate.png"));
        }

        public User GetMention(string mention)
        {
            return Message.Client.UsersCache.FirstOrDefault(u => u._id == mention) ??
                   Message.Client.UsersCache.FirstOrDefault(u => u.Username.ToLower() == mention.ToLower()) ??
                   Message.Client.UsersCache.FirstOrDefault(u => u._id == mention.Replace("<@", "").Replace(">", "")) ??
                   Message.Client.Users.Get(mention);
        }

        [Command("retard")]
        public async Task Retard()
        {
            await Message.Channel.BeginTypingAsync();
            await Task.Delay(7000);
            await ReplyAsync("<@01EXAG0ZFX02W7PNQE7W5MT339> retard");
            await Message.Channel.EndTypingAsync();
        }

        [Command("druh")]
        public async Task Druh()
        {
            await ReplyAsync("cock");
            return;
            var delay = 50;
            while (true)
            {
                await Message.Channel.BeginTypingAsync();
                await Task.Delay(delay);
                await Message.Channel.EndTypingAsync();
                await Task.Delay(delay);
            }
        }
    }
}