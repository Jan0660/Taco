using System;

namespace RevoltBot.Attributes
{
    // todo: add applicable to
    public class SummaryAttribute : Attribute
    {
        public string Text;

        public SummaryAttribute(string summary)
        {
            this.Text = summary;
        }
    }
}