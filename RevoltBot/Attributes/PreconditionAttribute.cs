using System;
using System.Threading.Tasks;
using RevoltApi;

namespace RevoltBot.Attributes
{
    public abstract class PreconditionAttribute : Attribute
    {
        public abstract Task<PreconditionResult> Evaluate(Message message);
    }
}