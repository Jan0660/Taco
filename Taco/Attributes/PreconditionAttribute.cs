using System;
using System.Threading.Tasks;
using Revolt;
using Taco.CommandHandling;

namespace Taco.Attributes
{
    public abstract class PreconditionAttribute : Attribute
    {
        public abstract Task<PreconditionResult> Evaluate(Message message);
    }
}