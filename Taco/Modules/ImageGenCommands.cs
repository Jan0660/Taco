using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
            => TemplateSend("JesusCarry.png", _getContextUserId(), new Point(162, 182));

        [Command("suicide")]
        public Task Suicide()
            => TemplateSend("Suicide.png", _getContextUserId(), new Point(36, 103), size: 64);

        [Command("retard", "retardFound")]
        public Task Retard()
            => TemplateSend("RetardFound.png", _getContextUserId(), new Point(486, 18), rounded: false, size: 256);
        
        [Command("02ily")]
        public Task ZeroTwoILoveThis()
            => TemplateSend("02ily.png", _getContextUserId(), new Point(131, 569), rounded: false, size: 256);
        
        [Command("killList")]
        public Task KillList()
            => TemplateSend("KillList.png", _getContextUserId(), new Point(69, 53), rounded: false, size: 256);
        
        [Command("lovedList")]
        public Task LovedList()
            => TemplateSend("LovedList.png", _getContextUserId(), new Point(69, 53), rounded: false, size: 256);

        private string _getContextUserId()
        {
            if (Args == "")
                return Message.AuthorId;
            if (Args.Length == 26)
                return Args;
            if (Args.Length == 29)
                return Args.Replace("<@", "").Replace(">", "");
            return Context.Message.Client.UsersCache.FirstOrDefault(u => u.Username.ToLower() == Args.ToLower())?._id ??
                   "amogus";
        }

        public async Task<Message> TemplateSend(string template, string userId, Point location, bool rounded = true,
            int size = 128)
        {
            var httpClient = new HttpClient();
            var pfp = new Bitmap(
                await httpClient.GetStreamAsync($"{Message.Client.ApiUrl}/users/{userId}/avatar?size={size}"));
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