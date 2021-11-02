using SkiaSharp;

namespace Taco.Util
{
    public static class ImageGen
    {
        static public SKBitmap Template(SKBitmap pfp, string templateFile, SKPoint point)
        {
            var template = SKBitmap.Decode("./Resources/" + templateFile);
            using var src = new SKCanvas(template);
            src.DrawBitmap(pfp, point);
            src.Flush();
            return template;
        }

        public static SKBitmap RoundCorners(SKBitmap bitmap)
        {
            var roundedImage = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(roundedImage);
            using var path = new SKPath();
            using var paint = new SKPaint();
            path.AddCircle(bitmap.Width / 2f, bitmap.Height / 2f, bitmap.Height / 2f);
            canvas.ClipPath(path, SKClipOperation.Intersect, true);
            canvas.ResetMatrix();
            canvas.DrawBitmap(bitmap, 0f, 0f);
            return roundedImage;
        }
    }
}