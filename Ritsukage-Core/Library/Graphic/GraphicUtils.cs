using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading;

namespace Ritsukage.Library.Graphic
{
    public static class GraphicUtils
    {
        public static readonly Rgba32 TransparentColor = new(0, 0, 0, 0);

        static readonly ImageFormatManager ImageFormatManager;

        public static class ImageFormat
        {
            public static IImageFormat Bmp => BmpFormat.Instance;

            public static IImageFormat Gif => GifFormat.Instance;

            public static IImageFormat Jpeg => JpegFormat.Instance;

            public static IImageFormat Png => PngFormat.Instance;

            public static IImageFormat Default => Png;
        }

        public static class ImageDecoder
        {
            public static IImageDecoder Bmp => BmpDecoder.Instance;

            public static IImageDecoder Gif => GifDecoder.Instance;

            public static IImageDecoder Jpeg => JpegDecoder.Instance;

            public static IImageDecoder Png => PngDecoder.Instance;
        }

        public static class ImageEncoder
        {
            public static IImageEncoder Bmp => new BmpEncoder
            {
                SupportTransparency = true
            };

            public static IImageEncoder Gif => new GifEncoder
            {
                ColorTableMode = GifColorTableMode.Local
            };

            public static IImageEncoder Jpeg => new JpegEncoder();

            public static IImageEncoder Png => new PngEncoder();
        }

        static GraphicUtils()
        {
            ImageFormatManager = new ImageFormatManager();
            ImageFormatManager.AddImageFormat(ImageFormat.Bmp);
            ImageFormatManager.SetDecoder(ImageFormat.Bmp, ImageDecoder.Bmp);
            ImageFormatManager.SetEncoder(ImageFormat.Bmp, ImageEncoder.Bmp);
            ImageFormatManager.AddImageFormat(ImageFormat.Gif);
            ImageFormatManager.SetDecoder(ImageFormat.Gif, ImageDecoder.Gif);
            ImageFormatManager.SetEncoder(ImageFormat.Gif, ImageEncoder.Gif);
            ImageFormatManager.AddImageFormat(ImageFormat.Jpeg);
            ImageFormatManager.SetDecoder(ImageFormat.Jpeg, ImageDecoder.Jpeg);
            ImageFormatManager.SetEncoder(ImageFormat.Jpeg, ImageEncoder.Jpeg);
            ImageFormatManager.AddImageFormat(ImageFormat.Png);
            ImageFormatManager.SetDecoder(ImageFormat.Png, ImageDecoder.Png);
            ImageFormatManager.SetEncoder(ImageFormat.Png, ImageEncoder.Png);
        }

        public static IImageDecoder FindDecoder(IImageFormat format)
            => ImageFormatManager.GetDecoder(format);

        public static IImageEncoder FindEncoder(IImageFormat format)
            => ImageFormatManager.GetEncoder(format);

        public static bool FindFormatByFileExtension(string extension, out IImageFormat format)
            => ImageFormatManager.TryFindFormatByFileExtension(extension, out format);

        public static bool FindFormatByMimeType(string mimeType, out IImageFormat format)
            => ImageFormatManager.TryFindFormatByMimeType(mimeType, out format);

        public static Image<Rgba32> LoadImage(byte[] bytes, out IImageFormat format)
        {
            format = Image.DetectFormat(bytes);
            return Image.Load<Rgba32>(bytes);
        }

        public static Image<Rgba32> LoadImage(byte[] bytes, IImageDecoder decoder)
            => LoadImage(new MemoryStream(bytes), decoder);

        public static Image<Rgba32> LoadImage(byte[] bytes)
            => LoadImage(bytes, out _);

        public static Image<Rgba32> LoadImage(Stream stream, out IImageFormat format)
        {
            format = Image.DetectFormat(stream);
            return Image.Load<Rgba32>(stream);
        }

        public static Image<Rgba32> LoadImage(Stream stream, IImageDecoder decoder)
            => decoder.Decode<Rgba32>(new(), stream);

        public static Image<Rgba32> LoadImage(Stream stream)
            => LoadImage(stream, out _);

        public static Image<Rgba32> LoadImage(string path, out IImageFormat format)
        {
            format = Image.DetectFormat(path);
            return Image.Load<Rgba32>(path);
        }

        public static Image<Rgba32> LoadImage(string path, IImageDecoder decoder)
            => LoadImage(File.ReadAllBytes(path), decoder);

        public static Image<Rgba32> LoadImage(string path)
            => LoadImage(path, out _);

        public static async void SaveImage(Image<Rgba32> image, IImageFormat format, string path)
        {
            var encoder = FindEncoder(format) ?? FindEncoder(ImageFormat.Default);
            var stream = GetFileStream(path);
            if (stream != null)
            {
                await image.SaveAsync(stream, encoder);
                stream?.Dispose();
                if (format == ImageFormat.Gif)
                    await GIFsicle.Compress(path);
            }
        }

        public static bool LimitGraphicScale(string path, int maxWidth, int maxHeight)
        {
            try
            {
                var image = LoadImage(path, out IImageFormat format);
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
                    image.Mutate(x => x.Resize(width, height));
                    SaveImage(image, format, path);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool LimitGraphicScale(string path, int maxScale)
            => LimitGraphicScale(path, maxScale, maxScale);

        static FileStream GetFileStream(string path)
        {
            FileStream stream = null;
            bool flag = false;
            SpinWait.SpinUntil(() =>
            {
                try
                {
                    stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                }
                catch (IOException)
                {
                }
                catch (Exception)
                {
                    flag = true;
                }
                return flag || stream != null;
            });
            return stream;
        }
    }
}
