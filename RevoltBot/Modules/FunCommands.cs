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
    }
}