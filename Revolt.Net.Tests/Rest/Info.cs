namespace Revolt.Net.Tests.Rest;

[TestClass]
public class Info
{
    [TestMethod]
    public async Task Delta()
    {
        var delta = await Static.Bot.GetApiInfoAsync();
        Assert.IsNotNull(delta);
        Assert.IsNotNull(delta.Features);
    }
    [TestMethod]
    public async Task Autumn()
    {
        var autumn = await Static.Bot.GetAutumnInfoAsync();
        Assert.IsNotNull(autumn);
        Assert.IsNotNull(autumn.Tags);
    }
    [TestMethod]
    public async Task Vortex()
    {
        var vortex = await Static.Bot.GetVortexInfoAsync();
        Assert.IsNotNull(vortex);
        Assert.IsNotNull(vortex.Features);
    }
}