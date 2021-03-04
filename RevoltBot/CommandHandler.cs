using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RevoltApi;

namespace RevoltBot
{
    public static class CommandHandler
    {
        public static List<CommandInfo> Commands = new();
        public static void LoadCommands()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                if(type.IsInterface)
                    continue;
                // https://stackoverflow.com/questions/4963160/how-to-determine-if-a-type-implements-an-interface-with-c-sharp-reflection
                if (typeof(ModuleBase).IsAssignableFrom(type))
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var att = method.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "CommandAttribute");
                        if(att == null)
                            continue;
                        var aliases = new List<string>();
                        foreach (var arg in att.ConstructorArguments)
                        {
                            var val = arg.Value as IReadOnlyCollection<CustomAttributeTypedArgument>;
                            foreach (var h in val)
                            {
                                aliases.Add((string)h.Value);
                            }
                        }
                        var command = new CommandInfo
                        {
                            Aliases = aliases.ToArray(), AttributeType = att.AttributeType, Method = method
                        };
                        Commands.Add(command);
                    }
                }
            }
        }

        public static void ExecuteCommand(Message message, int prefixLength)
        {
            var relevant = message.Content.Remove(0, prefixLength);
            // get command
            var command = Commands.FirstOrDefault(c => c.Aliases.Any(alias => relevant.ToLower().StartsWith(alias.ToLower())));
            if (command == null)
                throw new Exception("COMMAND_NOT_FOUND");
            var alias = command.Aliases.First(alias => relevant.ToLower().StartsWith(alias.ToLower()));
            var args = relevant.Remove(0, alias.Length + (alias.Length == relevant.Length ? 0 : 1));
            command.Execute(message, args);
        }
    }
}