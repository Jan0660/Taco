namespace Revolt.Net.Tests.Rest;

[TestClass]
public class Invites
{
    [TestMethod]
    public async Task FetchInvite()
    {
        var invite = await Static.Bot.Invites.FetchInviteAsync("Testers");
        Assert.AreEqual(invite.Type, "Server");
    }
}