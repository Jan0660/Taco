using System;
using System.Collections.Generic;
using System.Linq;

namespace Taco.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]  
    public class ModuleNameAttribute : Attribute
    {
        public string Text { get; }
        public string[] Aliases { get; }
        public IEnumerable<String> AllNames => new[] {Text}.Concat(Aliases);

        public ModuleNameAttribute(string name)
        {
            Aliases = new string[0];
            this.Text = name;
        }
        
        public ModuleNameAttribute(string name, params string[] aliases)
        {
            this.Text = name;
            Aliases = aliases;
        }
    }
}