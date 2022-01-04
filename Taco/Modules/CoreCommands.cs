using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Revolt.Channels;
using Revolt.Commands.Attributes;
using Taco.CommandHandling;
using Taco.Util;
using Console = Log73.Console;

namespace Taco.Modules
{
    [Name("Core")]
    [Summary("Core commands like `info` and `help`.")]
    public class CoreCommands : TacoModuleBase
    {
        [Command("info")]
        [Summary("General information about the bot.")]
        public Task BotInformation()
        {
            var uptime = DateTime.Now - Program.StartTime;
            return ReplyAsync($@"> ## Taco
> **Developed by:** [Jan0660](</@01EX40TVKYNV114H8Q8VWEGBWQ>) (<https://github.com/Jan0660>)
> **Repository:** <https://github.com/Jan0660/Taco>
> **Uptime:** {(uptime.Days == 0 ? "" : uptime.Days + " Days")} {uptime.Hours} Hours {uptime.Minutes} Minutes
> **Latest update at:** <t:{new DateTimeOffset(new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToUniversalTime()).ToUnixTimeSeconds()}:D>
> **Groups count:** {Message.Client.ChannelsCache.OfType<GroupChannel>().Count()}
> **Servers count:** {Message.Client.ServersCache.Count}
> [invite link](<https://app.revolt.chat/bot/{Context.Client.User._id}>)"
#if DEBUG
                              + "\nDEBUG BUILD"
#endif
            );
        }

        [Command("help")]
        [Summary("Get general/category/commands help.")]
        public async Task Help([Remainder] string query = null)
        {
            if (query == null)
            {
                // main help
                var description =
                    $@"Use `{Program.Prefix}help <name of module>` to get the list of commands in a module.
| Module | Description | Command count |
|:------- |:------:|:-----:|
";
                foreach (var module in CommandHandler.Commands.Modules)
                {
                    if (module.IsHidden() || module.Commands.Count == 0)
                        continue;
                    description += $"| {module.Name} | {module.Summary ?? "No summary"} | {module.Commands.Count} |\n";
                }

                await ReplyAsync(description);
                return;
            }
            else
            {
                // ManPages
                {
                    var page = ManPages.Get(query);
                    if (page != null)
                    {
                        await ReplyAsync(page.Content);
                        return;
                    }
                }
                // Module
                {
                    var response = HelpUtil.GetModuleHelpContent(query);
                    if (response != null)
                    {
                        await ReplyAsync(response);
                        return;
                    }
                }
                // Command
                {
                    var command = CommandHandler.Commands.Search(query).Commands.FirstOrDefault().Command;
                    if (command == null)
                        goto after_command;
                    var preconditions = "";
                    foreach (var precondition in command.Preconditions)
                        preconditions +=
                            $"$\\color{{{((await precondition.CheckPermissionsAsync(Context, command, null)).IsSuccess ? "lime" : "red")}}}\\textsf{{{precondition.GetType().Name.Replace("Attribute", "")}}}$, ";
                    if (preconditions != "")
                        preconditions = preconditions.Remove(preconditions.Length - 2);
                    await ReplyAsync($@"> ## {command.Aliases.First()}
> {command.Summary}" + (preconditions != "" ? "\n> **Preconditions:** " + preconditions : "")
                     + (command.Aliases.Count != 1
                         ? $"\n> **Aliases:** {String.Join(", ", command.Aliases.ToArray()[1..])}"
                         : "")
                     + (command.Module != null ? $"\n> **Module:** {command.Module.Name}" : ""));
                    return;
                }
                after_command: ;
            }

            await ReplyAsync("Help not found!");
        }

        [Command("me")]
        [Summary("Shows your current information with this bot.")]
        public Task Me()
        {
            return ReplyAsync($@"> ## {Context.User.Username}
> **Permission Level:** {Context.UserData.PermissionLevel} - {((sbyte)Context.UserData.PermissionLevel)}");
        }

        [Command("ping")]
        [Summary("Ping!")]
        public Task Ping()
        {
            var web = new WebClient();
            var stopwatch = Stopwatch.StartNew();
            web.DownloadString(Message.Client.ApiUrl);
            var restPing = stopwatch.ElapsedMilliseconds;
            return ReplyAsync(@$"**REST API Ping:** `{restPing}`ms
**MongoDB:** `{Mongo.Ping().Result}`ms
**Websocket Ping:** `{Context.Client.WebSocketPing}`ms");
        }

        [Command("test")]
        [Alias("test-alias")]
        [Summary("Test command.")]
        public Task TestCommand()
            => ReplyAsync("test");

        [Command("prefix")]
        [Summary("Get prefix.")]
        public Task GetPrefix()
            => ReplyAsync($"My prefix here is `{Context.CommunityData.CustomPrefix ?? Program.Prefix}`.");

        [Command("invite")]
        public Task GetInvite()
            => ReplyAsync($"https://app.revolt.chat/bot/{Context.Client.User._id}");
    }
}