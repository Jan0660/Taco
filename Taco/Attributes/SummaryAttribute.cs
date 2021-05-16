using System;

namespace Taco.Attributes
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