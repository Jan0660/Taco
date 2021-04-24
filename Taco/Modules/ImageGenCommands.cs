using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using RevoltApi;
using RevoltBot.Attributes;
using RevoltBot.CommandHandling;
using RevoltBot.Util;

namespace RevoltBot.Modules
{
    [ModuleName("ImageGen")]
    public class ImageGenCommands : ModuleBase
    {
        public static int BaseRounding = 8; // 32
        public const ushort BaseResolution = 16; // 64
        [Command("jesusCarry")]
        public Task JesusCarry()
            => TemplateSend("JesusCarry.png", Message.AuthorId, new Point(162, 182));
        [Command("suicide")]
        public Task Suicide()
            => TemplateSend("Suicide.png", Message.AuthorId, new Point(36, 103), size: 64);

        public async Task<Message> TemplateSend(string template, string userId, Point location, bool rounded = true, int size = 128)
        {
            var httpClient = new HttpClient();
            var pfp = new Bitmap(await httpClient.GetStreamAsync($"{Message.Client.ApiUrl}/users/{userId}/avatar?size={size}"));
            if (pfp.Height != size)
            {
                pfp = pfp.Resize(new Size(size, size), ImageFormat.Png);
            }
            if (rounded)
                pfp = ImageGen.RoundCorners(pfp, (int)(BaseRounding * ((double)pfp.Height / (double)BaseResolution)));
            var img = ImageGen.Template(pfp, template, location);
            var stream = new MemoryStream();
            img.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            return await Message.Channel.SendFileAsync("HH", "bruowh.png", stream.GetBuffer());
        }
    }
}