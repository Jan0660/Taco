using System.Threading.Tasks;
using RevoltApi;

namespace RevoltBot.Attributes
{
    public class RequireBotOwnerAttribute : BarePreconditionAttribute
    {
        public override async Task<bool> Evaluate(Message message)
        {
            if (message.AuthorId == Program.BotOwnerId)
                return true;
            await message.Channel.SendMessageAsync(
                $"Sorry, but this command can only be executed by the developer of this bot, <@{Program.BotOwnerId}>.");
            return false;
        }
    }
}