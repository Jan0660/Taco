using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Owoify;
using RevoltApi.Channels;
using RevoltBot.Attributes;
using RevoltBot.CommandHandling;

namespace RevoltBot.Modules
{
    [ModuleName("Fun")]
    [Summary("r")]
    public class FunCommands : ModuleBase
    {
        [Command("owo")]
        [Summary("OwOify input.")]
        public Task OwO()
            => ReplyAsync(Owoifier.Owoify(Args));


        [Command("uwu")]
        [Summary("UwUify input.")]
        public Task UwU()
            => ReplyAsync(Owoifier.Owoify(Args, Owoifier.OwoifyLevel.Uwu));


        [Command("uvu")]
        [Summary("UvUify input.")]
        public Task UvU()
            => ReplyAsync(Owoifier.Owoify(Args, Owoifier.OwoifyLevel.Uvu));

        [Command("bing")]
        [Summary("Gets current bing versions.")]
        public async Task BingInfo()
        {
            var content = await new WebClient().DownloadStringTaskAsync("https://www.bing.com/version");
            var matches = new Regex("(?<=<td>build</td><td>)(.+?)(?=</td>)").Matches(content);
            await ReplyAsync($@"> **SNRCode:** {matches[0]}
> **CoreCLR:** {matches[1]}
> **CoreFX:** {matches[2]}");
        }

        [Command("bing-toggle")]
        [Summary("Toggles Bing notifications in this channel.")]
        [GroupOnly]
        public async Task BingReminderToggle()
        {
            if (Message.AuthorId == "01EX40TVKYNV114H8Q8VWEGBWQ"
                | Message.AuthorId == ((GroupChannel) Message.Channel).OwnerId)
            {
                if (Program.Config.BingReminderChannels.Contains(Message.ChannelId))
                {
                    Program.Config.BingReminderChannels.Remove(Message.ChannelId);
                    await Program.SaveConfig();
                    await ReplyAsync("Removed from bing reminders.");
                }
                else
                {
                    Program.Config.BingReminderChannels.Add(Message.ChannelId);
                    await Program.SaveConfig();
                    await ReplyAsync("Added to bing reminders.");
                }
            }
            else
            {
                await ReplyAsync("no");
            }
        }

        [Command("uber-fruit", "uber", "uberfruit")]
        [Summary("Sends some nice uber fruit.")]
        public Task UberFruit()
            => Message.Channel.SendFileAsync("", "uber.png", "./Resources/UberFruit.png");

        [Command("flooshed", "floosh")]
        [Summary(":flushed:")]
        public Task Flooshed()
            => Message.Channel.SendFileAsync("", "flooshed.png", "./Resources/flooshed.png");

        [Command("flush", "flushed")]
        [Summary(":flushed:")]
        public Task Flushed()
            => ReplyAsync("# $\\huge\\text{😳}$");

        [Command("retard")]
        public async Task Retard()
        {
            await Message.Channel.BeginTypingAsync();
            await Task.Delay(7000);
            await ReplyAsync("<@01EXAG0ZFX02W7PNQE7W5MT339> retard");
            await Message.Channel.EndTypingAsync();
        }

        [Command("gaytext", "gay")]
        [Summary("Converts h*terosexual text to the gay.")]
        public Task GayText()
        {
            var cycle = new[]
            {
                "F66", "FC6", "CF6", "6F6", "6FC", "6CF", "66F", "C6F"
            };
            //var h = @"$\textsf{\color{#F66}g\color{#FC6}a\color{#CF6}y\color{#6F6} \color{#6FC}t\color{#6CF}e\color{#66F}x\color{#C6F}t}$";
            var res = @"$\textsf{";
            int i = 0;
            foreach (char ch in Args)
            {
                res += $"\\color{{#{cycle[i]}}}{ch}";
                i++;
                if (i == cycle.Length)
                    i = 0;
            }
            res += "}$";
            return ReplyAsync(res);
        }
    }
}