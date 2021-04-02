﻿using System.Threading.Tasks;
using RevoltApi;
using RevoltBot.CommandHandling;

namespace RevoltBot.Attributes
{
    public class RequireBotOwnerAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> Evaluate(Message message)
        {
            if (message.AuthorId == Program.BotOwnerId)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError($"Sorry, but this command can only be executed by the developer of this bot, <@{Program.BotOwnerId}>."));
        }
    }
}