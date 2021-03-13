using System;
using System.Collections;
using System.Collections.Generic;
using RevoltBot.Attributes;

namespace RevoltBot
{
    public class ModuleInfo
    {
        public List<CommandInfo> Commands = new ();
        public Type Type;

        public string Name => Names?.Text ?? Type.Name;
        public ModuleNameAttribute Names;
        public string Summary;
    }
}