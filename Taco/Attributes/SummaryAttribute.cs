using System;

namespace RevoltBot.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SummaryAttribute : Attribute
    {
        public string Text;

        public SummaryAttribute(string summary)
        {
            this.Text = summary;
        }
    }
}