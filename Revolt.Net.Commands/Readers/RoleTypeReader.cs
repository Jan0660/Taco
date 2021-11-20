using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Revolt.Commands.Results;

namespace Revolt.Commands.Readers;

public class RoleTypeReader<T> : TypeReader
    where T : Role
{
    public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
        IServiceProvider services)
    {
        var results = new Dictionary<string, TypeReaderValue>();
        string id = null;
        // By id (1.0)
        {
            var role = context.Server.Roles.FirstOrDefault(role =>
                input == role.Key);
            AddResult(results, role.Key, role.Value as T, 1.0f);
        }
        // By case-sensitive name (0.9)
        {
            var role = context.Server.Roles.FirstOrDefault(role =>
                input == role.Value.Name);
            AddResult(results, role.Key, role.Value as T, 0.9f);
        }
        // By case-insensitive name (0.8)
        {
            var role = context.Server.Roles.FirstOrDefault(role =>
                string.Equals(input, role.Value.Name, StringComparison.OrdinalIgnoreCase));
            AddResult(results, role.Key, role.Value as T, 0.8f);
        }

        if (results.Count != 0)
            return TypeReaderResult.FromSuccess(results.Values.ToImmutableArray());

        return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Role not found.");
    }

    private void AddResult(Dictionary<string, TypeReaderValue> results, string roleId, T role, float score)
    {
        if (role != null && !results.ContainsKey(roleId))
            results.Add(roleId, new TypeReaderValue(role, score));
    }
}