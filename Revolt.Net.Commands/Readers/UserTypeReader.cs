using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;
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
            var results = new Dictionary<string, TypeReaderValue>();
            string id = null;
            // By mention and id (1.0)
            if (input.Length == 29 && input.StartsWith("<@") && input.EndsWith(">"))
                id = input[2..^1];
            else if (input.Length == 26)
                id = input;
            if (id != null)
            {
                var res = context.Client.Users.GetCached(id);
                res ??= await context.User.Client.Users.FetchUserAsync(id);
                AddResult(results, res as T, 1.0f);
            }

            // By Username (0.9)
            AddResult(results, context.Client.UsersCache.FirstOrDefault(user =>
                string.Equals(input, user.Username, StringComparison.OrdinalIgnoreCase)) as T, 0.9f);
            // todo: By Nickname

            if (results.Count != 0)
                return TypeReaderResult.FromSuccess(results.Values.ToImmutableArray());

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "User not found.");
        }

        private void AddResult(Dictionary<string, TypeReaderValue> results, T user, float score)
        {
            if (user != null && !results.ContainsKey(user._id))
                results.Add(user._id, new TypeReaderValue(user, score));
        }
    }
}