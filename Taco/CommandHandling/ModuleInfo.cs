using System;
using System.Collections.Generic;
using Taco.Attributes;

namespace Taco.CommandHandling
{
    public class ModuleInfo
    {
        public List<CommandInfo> Commands = new ();
        public Type Type;

        public string Name => Names?.Text ?? Type.Name;
        public ModuleNameAttribute Names;
        public string Summary;
        public Attribute[] Attributes;
    }
}