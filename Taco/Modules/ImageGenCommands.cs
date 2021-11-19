using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Revolt;
using Revolt.Commands.Attributes;
using SkiaSharp;
using Taco.CommandHandling;
using Taco.Util;

namespace Taco.Modules
{
    [Name("ImageGen")]
    public class ImageGenCommands : TacoModuleBase
    {
        [Command("jesusCarry")]
        public Task JesusCarry(User user = null)
            => TemplateSend("JesusCarry.png", user ?? Context.User, new SKPoint(162, 182));

        [Command("02ily")]
        public Task ZeroTwoILoveThis(User user = null)
            => TemplateSend("02ily.png", user ?? Context.User, new SKPoint(131, 569), rounded: false, size: 256);

        [Command("killList")]
        public Task KillList(User user = null)
            => TemplateSend("KillList.png", user ?? Context.User, new SKPoint(69, 53), rounded: false, size: 256);

        [Command("lovedList")]
        public Task LovedList(User user = null)
            => TemplateSend("LovedList.png", user ?? Context.User, new SKPoint(69, 53), rounded: false, size: 256);

        public async Task<Message> TemplateSend(string template, User user, SKPoint location, bool rounded = true,
            int size = 128)
        {
            var httpClient = new HttpClient();
            var pfp = SKBitmap.Decode(await httpClient.GetByteArrayAsync($"{user.AvatarUrl}?size={size}"));
            if (pfp.Height != size)
                pfp = pfp.Resize(new SKSizeI(size, size), SKFilterQuality.Medium);
            if (rounded)
                pfp = ImageGen.RoundCorners(pfp);
            var img = ImageGen.Template(pfp, template, location);
            var stream = new MemoryStream();
            img.Encode(stream, SKEncodedImageFormat.Png, 90);
            stream.Position = 0;
            return await Message.Channel.SendFileAsync("", "jan.png", stream.GetBuffer());
        }
    }
}