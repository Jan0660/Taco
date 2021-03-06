using System;

namespace RevoltBot
{
    public class CommandAttribute : Attribute
    {
        public readonly string[] Aliases;

        public CommandAttribute(params string[] aliases)
        {
            this.Aliases = aliases;
        }
    }
}