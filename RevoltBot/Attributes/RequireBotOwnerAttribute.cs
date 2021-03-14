using System.Threading.Tasks;
using RevoltApi;

namespace RevoltBot.Attributes
{
    public class RequireBotOwnerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> Evaluate(Message message)
        {
            if (message.AuthorId == Program.BotOwnerId)
                return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError($"Sorry, but this command can only be executed by the developer of this bot, <@{Program.BotOwnerId}>.");
        }
    }
}