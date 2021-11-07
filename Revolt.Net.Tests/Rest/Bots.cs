namespace Revolt.Net.Tests.Rest;

[TestClass]
public class Bots
{
    [TestMethod]
    public async Task FetchOwnedBots()
    {
        var owned = await Static.User.Bots.FetchOwnedBotsAsync();
        Assert.IsNotNull(owned);
        Assert.IsNotNull(owned.Bots);
        Assert.IsNotNull(owned.Users);
    }
    [TestMethod]
    public async Task FetchPublicBot()
    {
        var bot = await Static.User.Bots.FetchPublicBotAsync("01FEEHTQX9CTRFW8P4EEGGSEY3");
        Assert.AreEqual("Taco", bot.Username);
    }
}