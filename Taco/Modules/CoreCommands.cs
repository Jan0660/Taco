using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Revolt;
using Revolt.Channels;
using Taco.Attributes;
using Taco.CommandHandling;

namespace Taco.Modules
{
    [ModuleName("Core", "base", "basic")]
    [Summary("Core commands like `info` and `help`.")]
    public class CoreCommands : ModuleBase
    {
        [Command("info")]
        [Summary("General information about the bot.")]
        public Task BotInformation()
        {
            var uptime = DateTime.Now - Program.StartTime;
            return ReplyAsync($@"> ## Taco
> **Developed by:** `owouwuvu` <@01EX40TVKYNV114H8Q8VWEGBWQ>
> **Uptime:** {(uptime.Days == 0 ? "" : uptime.Days + " Days")} {uptime.Hours} Hours {uptime.Minutes} Minutes
> **Latest update at:** {new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("dd/MM/yyyy")}
> **Groups count:** {Message.Client.ChannelsCache.OfType<GroupChannel>().Count()}
> **Friends count:** {Message.Client.UsersCache.Where(user => user.Relationship == RelationshipStatus.Friend).Count()}"
#if DEBUG
                              + "\nDEBUG BUILD"
#endif
            );
        }

        [Command("help")]
        [Summary("HELP ME HELP ME PLEASE SEND HELP")]
        public async Task Help()
        {
            if (Args == "")
            {
                // main help
                var description =
                    $@"Use `{Program.Prefix}help <name of module>` to get the list of commands in a module.
| Module | Description | Command count |
|:------- |:------:|:-----:|
";
                foreach (var module in CommandHandler.ModuleInfos)
                {
                    if (module.IsHidden())
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
                    var page = ManPages.Get(Args);
                    if (page != null)
                    {
                        await ReplyAsync(page.Content);
                        return;
                    }
                }
                // Module
                {
                    var module =
                        CommandHandler.ModuleInfos.FirstOrDefault(m =>
                            m.Names?.AllNames.Any(a => a.ToLower() == Args.ToLower()) ??
                            m.Name.ToLower() == Args.ToLower());
                    if (module == null)
                        goto after_module;
                    var response = @$"> # {module.Name}
> **No. of commands:** {module.Commands.Count}
> ## Commands:
> > | Command | Description |
> > |:------- |:------:|
";
                    foreach (var command in module.Commands)
                        response += $"> > | {command.Aliases.First()} | {command.Summary ?? "No summary"} |\n";
                    await ReplyAsync(response);
                    return;
                }
                after_module: ;
                // Command
                {
                    var command =
                        CommandHandler.Commands.FirstOrDefault(c => c.Aliases.Any(a => a.ToLower() == Args.ToLower()));
                    if (command == null)
                        goto after_command;
                    var preconditions = "";
                    foreach (var precondition in command.Preconditions)
                        preconditions +=
                            $"$\\color{{{((await precondition.Evaluate(Context)).IsSuccess ? "lime" : "red")}}}\\textsf{{{precondition.GetType().Name.Replace("Attribute", "")}}}$, ";
                    if (preconditions != "")
                        preconditions = preconditions.Remove(preconditions.Length - 2);
                    await ReplyAsync($@"> ## {command.Aliases.First()}
> {command.Summary}" + (preconditions != "" ? "\n> **Preconditions:** " + preconditions : "")
                     + (command.Aliases.Length != 1
                         ? $"\n> **Aliases:** {String.Join(", ", command.Aliases[1..])}"
                         : "")
                     + (command.Module != null ? $"\n> **Module:** {command.Module.Name}" : ""));
                    return;
                }
                after_command: ;
            }

            await ReplyAsync("noone can help you, not even god");
        }

        [Command("me")]
        [Summary("Shows your current information with this bot.")]
        public Task Me()
        {
            return ReplyAsync($@"> ## {Context.User.Username}
> **Permission Level:** {Context.UserData.PermissionLevel} - {((sbyte) Context.UserData.PermissionLevel)}");
        }

        [Command("donate")]
        public Task Donate()
            => ReplyAsync($@"> ## Donate
> **ETC:** `0xDd2c32F8c25Ae6e7aFC590593f5Dfd34639e4F14`
> **DONATE TO INSERT TOO h:** https://ko-fi.com/insertish");

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
**Websocket Ping:** doesnt exist");
        }

        [Command("test", "test-alias")]
        [Summary("Test command.")]
        public async Task TestCommand()
        {
            await ReplyAsync("test");
        }
    }
}