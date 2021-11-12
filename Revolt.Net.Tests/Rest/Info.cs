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
        Assert.IsNotNull(delta.Features.Autumn);
        Assert.IsNotNull(delta.Features.Captcha);
        Assert.IsNotNull(delta.Features.Vortex);
    }
    [TestMethod]
    public async Task Autumn()
    {
        var autumn = await Static.Bot.GetAutumnInfoAsync();
        Assert.IsNotNull(autumn);
        Assert.IsNotNull(autumn.Tags);
        Assert.IsNotNull(autumn.Tags.Attachments);
        Assert.IsNotNull(autumn.Tags.Avatars);
        Assert.IsNotNull(autumn.Tags.Backgrounds);
        Assert.IsNotNull(autumn.Tags.Banners);
        Assert.IsNotNull(autumn.Tags.Icons);
    }
    [TestMethod]
    public async Task Vortex()
    {
        var vortex = await Static.Bot.GetVortexInfoAsync();
        Assert.IsNotNull(vortex);
        Assert.IsNotNull(vortex.Features);
    }
}