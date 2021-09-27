using System.Collections.Generic;
using System.Linq;

namespace Taco
{
    public static class ManPages
    {
        public readonly static ManPage[] Pages = {
            new()
            {
                Name = "tags",
                Content = @"- To get a tag use `tag <name>`.
- To create a tag use `add tag <name> <content>`.
- To list tags use `list tag`."
            }
        };

        public static ManPage Get(string name)
            => Pages.FirstOrDefault(p => p.Names.Any(a => a.ToLower() == name.ToLower()));
    }

    public class ManPage
    {
        public string Name { get; init; }
        public IEnumerable<string> Names => new[] {Name}.Concat(Aliases);
        public string[] Aliases { get; init; } = new string[0];
        public string Content { get; init; }
    }
}