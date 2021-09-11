using System;
using System.Linq;
using Taco.CommandHandling;

namespace Taco.Util
{
    public static class HelpUtil
    {
        public static string GetModuleHelpContent(string query)
        {
            var module = CommandHandler.Commands.Modules.FirstOrDefault(m =>
                m.Aliases.Any(a => a.Equals(query, StringComparison.InvariantCultureIgnoreCase)) ||
                m.Name.Equals(query, StringComparison.InvariantCultureIgnoreCase));
            if (module == null)
                return null;
            var response = @$"> # {module.Name}
> **No. of commands:** {module.Commands.Count}
> ## Commands:
> > | Command | Description |
> > |:------- |:------:|
";
            foreach (var command in module.Commands)
                response += $"> > | {command.Aliases.First()} | {command.Summary ?? "No summary"} |\n";
            return response;
        }
    }
}