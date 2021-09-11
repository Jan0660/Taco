using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Anargy.Attributes;
using Anargy.Revolt.Preconditions;
using Owoify;
using Revolt.Channels;
using Taco.Attributes;
using Taco.CommandHandling;

namespace Taco.Modules
{
    [Name("Fun")]
    [Summary("Fun")]
    public class FunCommands : TacoModuleBase
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
            if (matches.Count == 1)
                await ReplyAsync($@"> **SNRCode:** {matches[0]}");
            else
                await ReplyAsync($@"> **SNRCode:** {matches[0]}
> **CoreCLR:** {matches[1]}
> **CoreFX:** {matches[2]}");
        }

        [Command("uber-fruit")]
        [Alias("uber", "uberfruit")]
        [Summary("Sends some nice uber fruit.")]
        public Task UberFruit()
            => (new Random().Next(10) > 7)
                ? Message.Channel.SendFileAsync("", "epic-uber.gif", "./Resources/EpicUberFruit.gif")
                : Message.Channel.SendFileAsync("", "uber.png", "./Resources/UberFruit.png");

        [Command("flooshed")]
        [Alias("floosh")]
        [Summary(":flushed:")]
        public Task Flooshed()
            => Message.Channel.SendFileAsync("", "flooshed.png", "./Resources/flooshed.png");

        [Command("flush")]
        [Alias("flushed")]
        [Summary(":flushed:")]
        public Task Flushed()
            => ReplyAsync("# $\\huge\\text{😳}$");

        [Command("gaytext")]
        [Alias("gay")]
        [Summary("Converts h*terosexual text to the gay.")]
        public Task GayText()
        {
            var cycle = new[]
            {
                "F66", "FC6", "CF6", "6F6", "6FC", "6CF", "66F", "C6F"
            };
            var res = @"$\textsf{";
            int i = 0;
            foreach (char ch in Args)
            {
                string str = ch switch
                {
                    '{' => "\\{",
                    '}' => "\\}",
                    '\\' => "\\\\",
                    _ => ch.ToString()
                };
                res += $"\\color{{#{cycle[i]}}}{str}";
                i++;
                if (i == cycle.Length)
                    i = 0;
            }

            res += "}$";
            return ReplyAsync(res);
        }
    }
}