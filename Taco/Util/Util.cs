using System.Text;
using System.Text.RegularExpressions;
using Revolt;

namespace Taco.Util;

public static class Util
{
    private static readonly Regex EscapeMarkdownRegex =
        new("(\\$)|(^#)", RegexOptions.Compiled | RegexOptions.Multiline);

    public static string EscapeMarkdown(string str)
        => EscapeMarkdownRegex.Replace(str, match => '\\' + match.Value);

    public static string StringBooled(bool value)
        => StringBooled(value, value.ToString().ToLowerInvariant());

    public static string StringBooled(bool value, string str)
        => $@"$\color{{{(value ? "lime" : "red")}}}\textsf{{{str}}}$";

    public static string TextEmbed(Role role)
    {
        var str = new StringBuilder();
        str.AppendLine($"Name: {role.Name}");
        str.AppendLine($"Rank: `{role.Rank}`");
        if (role.Color is not null or "")
            str.AppendLine($"Color: `{role.Color}`");
        str.AppendLine($"Hoisted: {StringBooled(role.Hoist ?? false)}");
        // str.AppendLine($"Server Permissions: {role.ServerPermissions} ({(int)role.ServerPermissions})");
        // str.AppendLine($"Channel Permissions: {role.ChannelPermissions} ({(int)role.ChannelPermissions})");
        return str.ToString();
    }

    /// <summary>
    /// Generates a text embed for the difference of two roles.
    /// </summary>
    /// <param name="old"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    public static string TextEmbed(Role old, Role now)
    {
        var str = new StringBuilder();
        if (old.Name != now.Name)
            str.AppendLine($"Name: {old.Name} **=>** {now.Name}");
        if (old.Name != now.Name)
            str.AppendLine($"Rank: `{old.Rank}` **=>** `{now.Rank}`");
        if (old.Color != now.Color)
            str.AppendLine($"Color: `{old.Color}` **=>** `{now.Color}`");
        if (old.Hoist != now.Hoist)
            str.AppendLine($"Hoisted: {StringBooled(now.Hoist ?? false)} **=>** {StringBooled(now.Hoist ?? false)}");
        // if(old.ServerPermissions != now.ServerPermissions)
        //     str.AppendLine($"Server Permissions: {old.ServerPermissions} **=>** {now.ServerPermissions}");
        // if(old.ChannelPermissions != now.ChannelPermissions)
        //     str.AppendLine($"Channel Permissions: {old.ChannelPermissions} **=>** {now.ChannelPermissions}");

        return str.ToString();
    }
}