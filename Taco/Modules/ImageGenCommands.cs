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

        public async Task<Message> TemplateSend(string template, string userId, Point location, bool rounded = true)
        {
            var httpClient = new HttpClient();
            var pfp = new Bitmap(await httpClient.GetStreamAsync($"{Message.Client.ApiUrl}/users/{userId}/avatar"));
            if (rounded)
                pfp = RoundCorners(pfp, (int)(BaseRounding * ((double)pfp.Height / (double)BaseResolution)));
            var img = ImageGen.Template(pfp, template, location);
            var stream = new MemoryStream();
            img.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            return await Message.Channel.SendFileAsync("HH", "bruowh.png", stream.GetBuffer());
        }
        
        public static Bitmap RoundCorners(Image image, int cornerRadius)
        {
            cornerRadius *= 2;
            Bitmap roundedImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(-1, -1, cornerRadius, cornerRadius, 180, 90);
            gp.AddArc(0 + roundedImage.Width - cornerRadius, -1, cornerRadius, cornerRadius, 270, 90);
            gp.AddArc(0 + roundedImage.Width - cornerRadius, 0 + roundedImage.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            gp.AddArc(-1, 0 + roundedImage.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            using (Graphics g = Graphics.FromImage(roundedImage))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.SetClip(gp);
                g.DrawImage(image, Point.Empty);
            }
            return roundedImage;
        }
    }
}