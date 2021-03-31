using System.Threading.Tasks;
using RevoltApi;
using RevoltBot.CommandHandling;

namespace RevoltBot.Attributes
{
    public class RequireDeveloperAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> Evaluate(Message message)
        {
            if (Mongo.GetOrCreateUserData(message.AuthorId).PermissionLevel == (PermissionLevel)100)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("You suck."));
        }
    }
}