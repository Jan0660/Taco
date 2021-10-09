using System;
using System.Threading.Tasks;
using Revolt.Commands.Results;

namespace Revolt.Commands.Readers
{
    public class UserTypeReader<T> : TypeReader
        where T : User
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
            IServiceProvider services)
        {
            User result = null;
            string id = null;
            // mention
            if (input.Length == 29 && input.StartsWith("<@") && input.EndsWith(">"))
                id = input[2..^1];
            // id
            else if (input.Length == 26)
                id = input;
            if (id != null)
            {
                result = context.Client.Users.GetCached(id);
                result ??= await context.User.Client.Users.FetchUserAsync(id);
            }

            if (result != null)
                return TypeReaderResult.FromSuccess(result);

            return TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a boolean.");
        }
    }
}