using System.Threading.Tasks;
using Revolt;
using Taco.CommandHandling;

namespace Taco.Attributes
{
    public class RequireDeveloperAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> Evaluate(Message message)
        {
            var data = Mongo.GetUserData(message.AuthorId);
            if (data is {PermissionLevel: (PermissionLevel)100})
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("You suck."));
        }
    }
}