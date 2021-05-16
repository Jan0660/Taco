using System;

namespace Taco.Attributes
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