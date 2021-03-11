using System;
using System.Threading.Tasks;
using RevoltApi;

namespace RevoltBot.Attributes
{
    public abstract class BarePreconditionAttribute : Attribute
    {
        public abstract Task<bool> Evaluate(Message message);
    }
}