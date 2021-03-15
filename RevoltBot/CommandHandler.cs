using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RevoltApi;
using RevoltBot.Attributes;

namespace RevoltBot
{
    public static class CommandHandler
    {
        public static List<CommandInfo> Commands = new();
        public static List<ModuleInfo> ModuleInfos = new();

        public static void LoadCommands()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                if (type.IsInterface)
                    continue;
                // https://stackoverflow.com/questions/4963160/how-to-determine-if-a-type-implements-an-interface-with-c-sharp-reflection
                if (typeof(ModuleBase).IsAssignableFrom(type))
                {
                    if(type == typeof(ModuleBase))
                        continue;
                    var module = new ModuleInfo()
                    {
                        Type = type,
                        Summary = type.GetCustomAttribute<SummaryAttribute>()?.Text,
                        Names = type.GetCustomAttribute<ModuleNameAttribute>()
                    };
                    ModuleInfos.Add(module);
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var att = method.GetCustomAttribute<CommandAttribute>();
                        if (att == null)
                            continue;
                        var aliases = att.Aliases;

                        var command = new CommandInfo
                        {
                            Aliases = aliases, Method = method,
                            Preconditions = method.GetCustomAttributes<PreconditionAttribute>(true).ToArray(),
                            Summary = method.GetCustomAttribute<SummaryAttribute>()?.Text,
                            Module = module
                        };
                        Commands.Add(command);
                        module.Commands.Add(command);
                    }
                }
            }
        }

        public static async Task ExecuteCommandAsync(Message message, int prefixLength)
        {
            var relevant = message.Content.Remove(0, prefixLength);
            // get command
            var commands = Commands.Where(c => c.Aliases.Any(a => relevant.ToLower() == a.ToLower()))
                .Concat(Commands.Where(c => c.Aliases.Any(a => relevant.ToLower().StartsWith(a.ToLower()))));
            CommandInfo command = null;
            int longest = 0;
            foreach (var cmd in commands)
            {
                foreach (var cmdAlias in cmd.Aliases.Where(a => relevant.ToLower().StartsWith(a.ToLower())))
                {
                    if (longest < cmdAlias.Length)
                    {
                        longest = cmdAlias.Length;
                        command = cmd;
                    }
                }
            }

            if (command == null)
                throw new Exception("COMMAND_NOT_FOUND");
            var alias = command.Aliases.First(a => relevant.ToLower().StartsWith(a.ToLower()));
            var args = relevant.Remove(0, alias.Length + (alias.Length == relevant.Length ? 0 : 1));
            foreach (var precondition in command.Preconditions)
            {
                var result = await precondition.Evaluate(message);
                if (!result.IsSuccess)
                {
                    await message.Channel.SendMessageAsync(result.Message);
                    return;
                }
            }

            await command.ExecuteAsync(message, args);
        }
    }
}