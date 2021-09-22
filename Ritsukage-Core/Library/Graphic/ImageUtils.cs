using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Threading.Tasks;

namespace Ritsukage.Library.Graphic
{
    public static class ImageUtils
    {
        static IImageFormat Png => SixLabors.ImageSharp.Formats.Png.PngFormat.Instance;
        static IImageEncoder PngEncoder => new SixLabors.ImageSharp.Formats.Png.PngEncoder();

        static IImageFormat Jpeg => SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance;
        static IImageEncoder JpegEncoder => new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();

        static IImageFormat Bmp => SixLabors.ImageSharp.Formats.Bmp.BmpFormat.Instance;
        static IImageEncoder BmpEncoder => new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder();

        static IImageFormat Gif => SixLabors.ImageSharp.Formats.Gif.GifFormat.Instance;
        static IImageEncoder GifEncoder => new SixLabors.ImageSharp.Formats.Gif.GifEncoder();

        public static async void LimitImageScale(string path, int maxWidth, int maxHeight)
        {
            await Task.Run(() =>
            {
                IImageFormat format = Image.DetectFormat(path);
                IImageEncoder encoder = null;
                if (format == Png)
                    encoder = PngEncoder;
                else if (format == Jpeg)
                    encoder = JpegEncoder;
                else if (format == Bmp)
                    encoder = BmpEncoder;
                else if (format == Gif)
                    encoder = GifEncoder;
                else
                    return;

                var image = Image.Load<Rgba32>(path);
                bool flag = false;
                var width = image.Width;
                var height = image.Height;
                if (width > maxWidth)
                {
                    flag = true;
                    var rate = (double)maxWidth / width;
                    width = Convert.ToInt32(Math.Floor(width * rate));
                    height = Convert.ToInt32(Math.Floor(height * rate));
                }
                if (height > maxHeight)
                {
                    flag = true;
                    var rate = (double)maxHeight / height;
                    width = Convert.ToInt32(Math.Floor(width * rate));
                    height = Convert.ToInt32(Math.Floor(height * rate));
                }
                if (flag)
                {
                    image.Mutate(x => x.Resize(width, height, new BoxResampler()));
                    image.Save(path, encoder);
                }
            });
        }
    }
}
