using System;

namespace Revolt.Commands.Attributes
{
    // Extension of the Cosmetic Summary, for Groups, Commands, and Parameters
    /// <summary>
    ///     Attaches remarks to your commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RemarksAttribute : Attribute
    {
        public string Text { get; }

        public RemarksAttribute(string text)
        {
            Text = text;
        }
    }
}
