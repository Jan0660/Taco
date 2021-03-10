using System;
using System.Reflection;
using System.Threading.Tasks;
using RevoltApi;

namespace RevoltBot
{
    public class CommandInfo
    {
        public string[] Aliases;
        public Type AttributeType;
        public MethodInfo Method;

        public async Task ExecuteAsync(Message message, string args)
        {
            var module = Activator.CreateInstance(Method.DeclaringType!) as ModuleBase;
            module.Message = message;
            module.Args = args;
            var val = Method.Invoke(module, null);
            if (val is Task task)
                await task;
        }
    }
}