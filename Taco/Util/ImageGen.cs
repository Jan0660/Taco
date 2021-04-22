using System.Drawing;
using System.Drawing.Imaging;

namespace RevoltBot.Util
{
    public static class ImageGen
    {
        static public Image Template(Image pfp, string template, Point point)
        {
            var src = new Bitmap("./Resources/" + template, true);
            var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppPArgb);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.Transparent);
            gr.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height));
            gr.DrawImage(pfp, point);
            return bmp;
        }
    }
}