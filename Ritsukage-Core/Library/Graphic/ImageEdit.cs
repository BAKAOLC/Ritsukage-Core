using Ritsukage.Tools.Console;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace Ritsukage.Library.Graphic
{
    public static class ImageEdit
    {
        static readonly ImageFormatManager ImageFormatManager;

        static ImageEdit()
        {
            ImageFormatManager = new ImageFormatManager();
            ImageFormatManager.AddImageFormat(PngFormat.Instance);
            #region Bmp
            {
                ImageFormatManager.AddImageFormat(BmpFormat.Instance);
                var encoder = new BmpEncoder
                {
                    SupportTransparency = true
                };
                ImageFormatManager.SetEncoder(BmpFormat.Instance, encoder);
            }
            #endregion
            #region Gif
            {
                ImageFormatManager.AddImageFormat(GifFormat.Instance);
                var encoder = new GifEncoder
                {
                    ColorTableMode = GifColorTableMode.Local
                };
                ImageFormatManager.SetEncoder(GifFormat.Instance, encoder);
            }
            #endregion
            #region Jpeg
            {
                ImageFormatManager.AddImageFormat(JpegFormat.Instance);
                var encoder = new JpegEncoder();
                ImageFormatManager.SetEncoder(JpegFormat.Instance, encoder);
            }
            #endregion
            #region Png
            {
                ImageFormatManager.AddImageFormat(PngFormat.Instance);
                var encoder = new PngEncoder();
                ImageFormatManager.SetEncoder(PngFormat.Instance, encoder);
            }
            #endregion
        }

        public static Image<Rgba32> LoadImage(Stream stream, out IImageFormat format)
        {
            var img = Image.Load<Rgba32>(stream, out format);
            stream.Dispose();
            return img;
        }

        public static Image<Rgba32> LoadImage(Stream stream)
        {
            var img = Image.Load<Rgba32>(stream);
            stream.Dispose();
            return img;
        }

        public static Image<Rgba32> LoadImage(string path, out IImageFormat format)
            => LoadImage(File.OpenRead(path), out format);

        public static Image<Rgba32> LoadImage(string path)
            => LoadImage(File.OpenRead(path));

        public static async void SaveImage(Image<Rgba32> image, IImageFormat format, string path)
        {
            var encoder = ImageFormatManager.FindEncoder(format);
            if (encoder == null)
            {
                ConsoleLog.Error("Image Edit", "未能找到对应图像格式的编码器定义，将使用PNG格式编码\n"
                    + new System.Diagnostics.StackFrame(true).ToString());
                encoder = ImageFormatManager.FindEncoder(PngFormat.Instance);
            }
            await image.SaveAsync(path, encoder);
            if (format == GifFormat.Instance)
            {
                await GIFsicle.Compress(path);
            }
        }

        public static Image<Rgba32> MirrorLeft(Image<Rgba32> image)
        {
            var img = image.Clone();
            for (int i = 0; i < image.Frames.Count; i++)
            {
                img.Frames.InsertFrame(i, _MirrorLeft(image.Frames.CloneFrame(i)).Frames.RootFrame);
                img.Frames.RemoveFrame(i + 1);
            }
            return img;
        }

        public static Image<Rgba32> MirrorRight(Image<Rgba32> image)
        {
            var img = image.Clone();
            for (int i = 0; i < image.Frames.Count; i++)
            {
                img.Frames.InsertFrame(i, _MirrorRight(image.Frames.CloneFrame(i)).Frames.RootFrame);
                img.Frames.RemoveFrame(i + 1);
            }
            return img;
        }

        public static Image<Rgba32> MirrorTop(Image<Rgba32> image)
        {
            var img = image.Clone();
            for (int i = 0; i < image.Frames.Count; i++)
            {
                img.Frames.InsertFrame(i, _MirrorTop(image.Frames.CloneFrame(i)).Frames.RootFrame);
                img.Frames.RemoveFrame(i + 1);
            }
            return img;
        }

        public static Image<Rgba32> MirrorBottom(Image<Rgba32> image)
        {
            var img = image.Clone();
            for (int i = 0; i < image.Frames.Count; i++)
            {
                img.Frames.InsertFrame(i, _MirrorBottom(image.Frames.CloneFrame(i)).Frames.RootFrame);
                img.Frames.RemoveFrame(i + 1);
            }
            return img;
        }

        public static Image<Rgba32> Mosaic(Image<Rgba32> image, int size = 2, int px = 0, int py = 0)
        {
            var img = image.Clone();
            for (int i = 0; i < image.Frames.Count; i++)
            {
                img.Frames.InsertFrame(i, _Mosaic(image.Frames.CloneFrame(i), size, px, py).Frames.RootFrame);
                img.Frames.RemoveFrame(i + 1);
            }
            return img;
        }

        static Image<Rgba32> _MirrorLeft(Image<Rgba32> image)
        {
            var img = image.Clone();
            int max_x = image.Width / 2;
            for (int x = 0; x < max_x; x++)
                for (int y = 0; y < image.Height; y++)
                    img[image.Width - x - 1, y] = image[x, y];
            return img;
        }

        static Image<Rgba32> _MirrorRight(Image<Rgba32> image)
        {
            var img = image.Clone();
            int max_x = image.Width / 2;
            for (int x = 0; x < max_x; x++)
                for (int y = 0; y < image.Height; y++)
                    img[x, y] = image[image.Width - x - 1, y];
            return img;
        }

        static Image<Rgba32> _MirrorTop(Image<Rgba32> image)
        {
            var img = image.Clone();
            int max_y = image.Height / 2;
            for (int y = 0; y < max_y; y++)
                for (int x = 0; x < image.Width; x++)
                    img[x, image.Height - y - 1] = image[x, y];
            return img;
        }

        static Image<Rgba32> _MirrorBottom(Image<Rgba32> image)
        {
            var img = image.Clone();
            int max_y = image.Height / 2;
            for (int y = 0; y < max_y; y++)
                for (int x = 0; x < image.Width; x++)
                    img[x, y] = image[x, image.Height - y - 1];
            return img;
        }

        static Image<Rgba32> _Mosaic(Image<Rgba32> image, int size = 2, int px = 0, int py = 0)
        {
            var img = image.Clone();
            px = Mod(px, size);
            py = Mod(py, size);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int tx = Math.Min(Convert.ToInt32(Math.Floor((double)x / size)) * size + px,
                        image.Width - 1);
                    int ty = Math.Min(Convert.ToInt32(Math.Floor((double)y / size)) * size + py,
                        image.Height - 1);
                    img[x, y] = image[tx, ty];
                }
            }
            return img;
        }

        static int Mod(int x, int mod)
        {
            x %= mod;
            return x < 0 ? x + mod : x;
        }
    }
}
