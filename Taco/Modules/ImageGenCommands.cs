using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Revolt;
using Revolt.Commands.Attributes;
using Taco.Attributes;
using Taco.CommandHandling;
using Taco.Util;

namespace Taco.Modules
{
    [Name("ImageGen")]
    public class ImageGenCommands : TacoModuleBase
    {
        public static int BaseRounding = 8; // 32
        public const ushort BaseResolution = 16; // 64

        [Command("jesusCarry")]
        public Task JesusCarry(User user = null)
            => TemplateSend("JesusCarry.png", user ?? Context.User, new Point(162, 182));

        [Command("suicide")]
        public Task Suicide(User user = null)
            => TemplateSend("Suicide.png", user ?? Context.User, new Point(36, 103), size: 64);

        [Command("02ily")]
        public Task ZeroTwoILoveThis(User user = null)
            => TemplateSend("02ily.png", user ?? Context.User, new Point(131, 569), rounded: false, size: 256);
        
        [Command("killList")]
        public Task KillList(User user = null)
            => TemplateSend("KillList.png", user ?? Context.User, new Point(69, 53), rounded: false, size: 256);
        
        [Command("lovedList")]
        public Task LovedList(User user = null)
            => TemplateSend("LovedList.png", user ?? Context.User, new Point(69, 53), rounded: false, size: 256);

        public async Task<Message> TemplateSend(string template, User user, Point location, bool rounded = true,
            int size = 128)
        {
            var httpClient = new HttpClient();
            var pfp = new Bitmap(
                await httpClient.GetStreamAsync($"{user.AvatarUrl}?size={size}"));
            if (pfp.Height != size)
            {
                pfp = pfp.Resize(new Size(size, size), ImageFormat.Png);
            }

            if (rounded)
                pfp = ImageGen.RoundCorners(pfp,
                    (int) (BaseRounding * ((double) pfp.Height / (double) BaseResolution)));
            var img = ImageGen.Template(pfp, template, location);
            var stream = new MemoryStream();
            img.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            return await Message.Channel.SendFileAsync("HH", "bruowh.png", stream.GetBuffer());
        }
    }
}