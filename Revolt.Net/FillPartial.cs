namespace Revolt
{
    // methods for filling partial objects with data from cached objects
    // under the condition of: if cached.field is not null AND partial.field is null
    public static class FillPartialMethods
    {
        public static void FillPartial(this Member? old, Member partial, string? remove = null)
        {
            if (old != null)
            {
                if (old.Nickname != null && partial!.Nickname == null)
                    partial.Nickname = old.Nickname;
                if (old.Avatar != null && partial!.Avatar == null)
                    partial.Avatar = old.Avatar;
                if (old.Roles != null && partial!.Roles == null)
                    partial.Roles = old.Roles;
            }

            if (remove == "Nickname")
                partial!.Nickname = null;
            if (remove == "Avatar")
                partial!.Avatar = null;
        }
    }
}