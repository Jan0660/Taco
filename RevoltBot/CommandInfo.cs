using System;
using System.Reflection;
using RevoltApi;

namespace RevoltBot
{
    public class CommandInfo
    {
        public string[] Aliases;
        public Type AttributeType;
        public MethodInfo Method;

        public void Execute(Message message, string args)
        {
            var module = Activator.CreateInstance(Method.DeclaringType!) as ModuleBase;
            module.Message = message;
            module.Args = args;
            Method.Invoke(module, null);
        }
    }
}