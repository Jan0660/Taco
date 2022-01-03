using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Revolt;
using Revolt.Channels;
using Revolt.Commands;
using Revolt.Commands.Extensions;
using Revolt.Commands.Info;
using Revolt.Commands.Results;
using Taco.Attributes;
using Taco.Util;
using Console = Log73.Console;

namespace Taco.CommandHandling
{
    public static class CommandHandler
    {
        public static CommandService Commands { get; private set; }
        private static string _mentionPrefix;

        public static async Task InitializeAsync()
        {
            var commands = Commands = new CommandService(new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async
            });
            // todo: where tf is ServiceCollection
            await commands.AddModulesAsync(typeof(Program).Assembly, null);
            commands.CommandExecuted += CommandExecuted;
            _mentionPrefix = $"<@{Program.Client.User._id}>";
            Program.Client.MessageReceived += MessageReceived;
            Console.Log("Initialized command handler.");
        }

        private static async Task CommandExecuted(Optional<CommandInfo> arg1, ICommandContext arg2, IResult result)
        {
            var context = (TacoCommandContext)arg2;
            if (!result.IsSuccess)
            {
                if (result.Error is CommandError.Exception && result is ExecuteResult executeResult)
                {
                    var exception = executeResult.Exception;
                    await context.Channel.SendMessageAsync($@"> ## An internal exception occurred
> 
> ```csharp
> ({exception.GetType().FullName}) {exception.Message.Replace("\n", "\n> ")}
> ```");
                    Console.Exception(exception);
                }
                else if (result.Error == CommandError.UnknownCommand)
                {
                    var content = context.Message.Content;
                    int argPos = 0;
                    if (context.Message.Content.HasPrefix(context.CommunityData.CustomPrefix, ref argPos))
                    {
                        content = content.Substring(argPos);
                    }

                    var helpIndex = content.IndexOf(" help", StringComparison.InvariantCultureIgnoreCase);
                    if (helpIndex != -1)
                        content = content.Remove(helpIndex);
                    var response = HelpUtil.GetModuleHelpContent(content);
                    if (response != null)
                        await context.Channel.SendMessageAsync(response);
                    else
                    {
                        var (_, tagValue) = context.ServerData.Tags.FirstOrDefault(t =>
                            t.Key.Equals(content, StringComparison.InvariantCultureIgnoreCase));
                        if (tagValue != null)
                            await context.Channel.SendMessageAsync(tagValue,
                                replies: new MessageReply[] { new(context.Message._id) });
                    }
// #if DEBUG
//                     else
//                     {
//                         await context.Channel.SendMessageAsync("[DEBUG] Command not found.",
//                             replies: new[] { new MessageReply(context.Message._id) });
//                     }
// #endif
                }
                else if (result.Error == CommandError.UnmetPrecondition)
                {
                    await context.Channel.SendMessageAsync(result.ToString()!,
                        replies: new[] { new MessageReply(context.Message._id) });
                }
            }
        }

        private static bool HasPrefix(this string args, string? customPrefix, ref int argPos)
            => args.HasStringPrefix(customPrefix ?? Program.Prefix, ref argPos) ||
               args.HasStringPrefix(_mentionPrefix + " ", ref argPos) ||
               args.HasStringPrefix(_mentionPrefix, ref argPos);

        public static async Task MessageReceived(Message message)
        {
            try
            {
                int argPos = 0;
                var context = new TacoCommandContext(message);
                if (
                    context.User.Bot != null ||
                    context.Message.AuthorId == context.Client.User._id ||
                    !message.Content.HasPrefix(context.CommunityData.CustomPrefix, ref argPos)
                )
                    return;
                var userData = context.GetUserData();
                if (userData != null)
                {
                    if (context.UserData.PermissionLevel == PermissionLevel.Blacklist)
                    {
                        await message.Channel.SendMessageAsync(context.UserData.BlacklistedMessage == null
                            ? $"<@{message.AuthorId}> blacklisted"
                            : String.Format(context.UserData.BlacklistedMessage, context.UserData.UserId));
                        return;
                    }
                    else if (context.UserData.PermissionLevel == PermissionLevel.BlacklistSilent)
                        return;
                }

                await Commands.ExecuteAsync(context, message.Content.Substring(argPos), null, MultiMatchHandling.Best);
            }
            catch (Exception exc)
            {
                Console.Exception(exc);
            }
        }
    }
}