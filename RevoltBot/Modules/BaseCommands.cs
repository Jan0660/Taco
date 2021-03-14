using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RevoltApi.Channels;
using RevoltBot.Attributes;

namespace RevoltBot.Modules
{
    [ModuleName("Basic", "base", "core")]
    [Summary("Basic commands like `info` and `help`.")]
    public class BaseCommands : ModuleBase
    {
        [Command("info")]
        [Summary("General information about the bot.")]
        public Task BotInformation()
        {
            var uptime = Program.StartTime - DateTime.Now;
            return ReplyAsync($@"> ## Taco
> **Developed by:** `owouwuvu` <@01EX40TVKYNV114H8Q8VWEGBWQ>
> **Uptime:** {(uptime.Days == 0 ? "" : uptime.Days + " Days")} {uptime.Hours} Hours {uptime.Minutes} Minutes
> **Latest update at:** {new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("dd/MM/yyyy")}
> **Groups count:** {Message.Client.ChannelsCache.OfType<GroupChannel>().Count()}");
        }

        [Command("help")]
        [Summary("HELP ME")]
        public async Task Help()
        {
            if (Args == "")
            {
                // main help
                var description = $@"Use `{Program.Prefix}help <name of module>` to get the list of commands in a module.
| Module | Description | Command count |
|:------- |:------:|:-----:|
";
                foreach (var module in CommandHandler.ModuleInfos)
                {
                    description += $"| {module.Name} | {module.Summary ?? "No summary"} | {module.Commands.Count} |\n";
                }

                await ReplyAsync(description);
                return;
            }
            else
            {
                {
                    var module =
                        CommandHandler.ModuleInfos.FirstOrDefault(m =>
                            m.Names?.AllNames.Any(a => a.ToLower() == Args.ToLower()) ?? m.Name.ToLower() == Args.ToLower());
                    if (module == null)
                        goto after_module;
                    var response = @$"> # {module.Name}
> **No. of commands:** {module.Commands.Count}
> ## Commands:
";
                    foreach (var command in module.Commands)
                        response += $"> > {command.Aliases.First()} - {command.Summary ?? "No summary"}\n";
                    await ReplyAsync(response);
                    return;
                }
                after_module: ;
                {
                    var command =
                        CommandHandler.Commands.FirstOrDefault(c => c.Aliases.Any(a => a.ToLower() == Args.ToLower()));
                    if (command == null)
                        goto after_command;
                    var preconditions = "";
                    foreach (var precondition in command.BarePreconditions)
                        preconditions += 
$"$\\color{{{(await precondition.EvaluateBool(Message) ? "lime" : "red")}}}\\text{{{precondition.GetType().Name.Replace("Attribute", "")}}}$, ";
                    preconditions = preconditions.Remove(preconditions.Length - 2);
                    await ReplyAsync($@"> ## {command.Aliases.First()}
> {command.Summary}" + (preconditions != "" ? "\n> **Preconditions:** " + preconditions : ""));
                    return;
                }
                after_command: ;
                // todo: special help pages
            }

            await ReplyAsync("no");
        }
    }
}