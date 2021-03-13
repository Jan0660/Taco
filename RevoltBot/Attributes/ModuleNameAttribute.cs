using System;

namespace RevoltBot.Attributes
{
    // todo: add applicable to
    public class ModuleNameAttribute : Attribute
    {
        public string Text;

        public ModuleNameAttribute(string name)
        {
            this.Text = name;
        }
    }
}