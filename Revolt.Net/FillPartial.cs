namespace Revolt
{
    // methods for filling partial objects with data from cached objects
    // under the condition of: if cached.field is not null AND partial.field is null
    public static class FillPartialMethods
    {
        public static void FillPartial(this Member? old, Member partial, string? clear = null)
        {
            if (old != null)
            {
                partial._id = old._id;
                if (old.Nickname != null && partial!.Nickname == null)
                    partial.Nickname = old.Nickname;
                if (old.Avatar != null && partial!.Avatar == null)
                    partial.Avatar = old.Avatar;
                if (old.Roles != null && partial!.Roles == null)
                    partial.Roles = old.Roles;
            }

            if (clear == "Nickname")
                partial!.Nickname = null;
            if (clear == "Avatar")
                partial!.Avatar = null;
        }

        public static void FillPartial(this Role old, Role partial, string? clear = null)
        {
            Role now = old;
            if (partial != null)
            {
                partial.Name ??= old.Name;
                partial.Permissions ??= old.Permissions;
                partial.Color ??= old.Color;
                partial.Hoist ??= old.Hoist;
                now = partial;
            }

            if (clear == "Colour")
                now.Color = null;
        }
    }
}