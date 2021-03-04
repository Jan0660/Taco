using System.IO;
using System.Threading.Tasks;
using RevoltApi;
using RevoltApi.Channels;
using SixLabors.ImageSharp;

namespace RevoltBot
{
    public static class ExtensionMethods
    {
        public static async Task<Message> SendPngAsync(this Channel channel, Image image, string content, string filename = "h.png")
        {
            var memory = new MemoryStream();
            await image.SaveAsPngAsync(memory);
            return await channel.SendFileAsync(content, filename, memory.GetBuffer());
        }
    }
}