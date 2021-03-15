using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RevoltApi;
using RevoltBot.Attributes;

namespace RevoltBot
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CommandInfo
    {
        public ModuleInfo Module;
        public string[] Aliases;
        public MethodInfo Method;
        public PreconditionAttribute[] Preconditions;
        public string Summary;
        private string DebuggerDisplay
        {
            get
            {
                return string.Format("Command: {0}", Aliases.First());
            }
        }

        public async Task ExecuteAsync(Message message, string args)
        {
            var module = Activator.CreateInstance(Method.DeclaringType!) as ModuleBase;
            module!.Message = message;
            module.Args = args;
            var val = Method.Invoke(module, null);
            if (val is Task task)
                await task;
        }
    }
}