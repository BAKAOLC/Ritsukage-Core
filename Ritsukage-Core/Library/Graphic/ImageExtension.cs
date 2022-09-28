using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using static Ritsukage.Library.Graphic.GraphicDataDefinition;

namespace Ritsukage.Library.Graphic
{
    public static class GraphicDataDefinition
    {
        public static readonly Rgba32 TransparentColor = new(0, 0, 0, 0);

        public static class ImageFormat
        {
            public static IImageFormat Bmp => BmpFormat.Instance;

            public static IImageFormat Gif => GifFormat.Instance;

            public static IImageFormat Jpeg => JpegFormat.Instance;

            public static IImageFormat Png => PngFormat.Instance;

            public static IImageFormat Default => Png;
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

        static readonly ImageFormatManager ImageFormatManager;

        static GraphicDataDefinition()
        {
            ImageFormatManager = new ImageFormatManager();
            ImageFormatManager.AddImageFormat(ImageFormat.Bmp);
            ImageFormatManager.SetEncoder(ImageFormat.Bmp, ImageEncoder.Bmp);
            ImageFormatManager.AddImageFormat(ImageFormat.Gif);
            ImageFormatManager.SetEncoder(ImageFormat.Gif, ImageEncoder.Gif);
            ImageFormatManager.AddImageFormat(ImageFormat.Jpeg);
            ImageFormatManager.SetEncoder(ImageFormat.Jpeg, ImageEncoder.Jpeg);
            ImageFormatManager.AddImageFormat(ImageFormat.Png);
            ImageFormatManager.SetEncoder(ImageFormat.Png, ImageEncoder.Png);
        }

        public static IImageDecoder FindDecoder(IImageFormat format)
            => ImageFormatManager.FindDecoder(format);

        public static IImageEncoder FindEncoder(IImageFormat format)
            => ImageFormatManager.FindEncoder(format);

        public static IImageFormat FindFormatByFileExtension(string extension)
            => ImageFormatManager.FindFormatByFileExtension(extension);

        public static IImageFormat FindFormatByMimeType(string mimeType)
            => ImageFormatManager.FindFormatByMimeType(mimeType);
    }

    public static class ImageExtension
    {
        static readonly ColorSpaceConverter Converter = new();

        #region Byte & Base64
        public static byte[] GetBytes<TPixel>(this Image<TPixel> image, IImageFormat format = null) where TPixel : unmanaged, IPixel<TPixel>
        {
            var ms = new MemoryStream();
            var encoder = FindEncoder(format ?? ImageFormat.Default);
            encoder?.Encode(image, ms);
            return ms.ToArray();
        }

        public static string ToBase64<TPixel>(this Image<TPixel> image, IImageFormat format = null) where TPixel : unmanaged, IPixel<TPixel>
            => Convert.ToBase64String(GetBytes(image, format));

        public static string ToBase64File<TPixel>(this Image<TPixel> image, IImageFormat format = null) where TPixel : unmanaged, IPixel<TPixel>
            => "base64://" + ToBase64(image, format);

        public static string ToBase64Source<TPixel>(this Image<TPixel> image, IImageFormat format = null) where TPixel : unmanaged, IPixel<TPixel>
        {
            format ??= ImageFormat.Default;
            var bytes = GetBytes(image, format);
            return $"data:{format.DefaultMimeType};base64," + Convert.ToBase64String(bytes);
        }
        #endregion

        #region Process
        static Image<TPixel> Worker<TPixel>(Image<TPixel> image, Action<Image<TPixel>> action) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (action != null)
            {
                for (int i = 0; i < image.Frames.Count; i++)
                {
                    var frame = image.Frames.CloneFrame(i);
                    action?.Invoke(frame);
                    image.Frames.InsertFrame(i, frame.Frames.RootFrame);
                    image.Frames.RemoveFrame(i + 1);
                }
            }
            return image;
        }

        #region Color HSV Transform
        public static void ColorHSVTransform<TPixel>(ref this TPixel pixel, float h, float s, float v) where TPixel : unmanaged, IPixel<TPixel>
        {
            Rgba32 originalRGBA32 = new();
            pixel.ToRgba32(ref originalRGBA32);
            var hsv = Converter.ToHsv(originalRGBA32);
            float H = (hsv.H + h) % 360;
            H = H < 0 ? H + 360 : H;
            Rgba32 translateRGBA32 = Converter.ToRgb(new Hsv(H, Math.Clamp(hsv.S + s, 0, 1), Math.Clamp(hsv.V + v, 0, 1)));
            translateRGBA32.A = originalRGBA32.A;
            pixel.FromRgba32(translateRGBA32);
        }

        public static Image<TPixel> ColorHSVTransform<TPixel>(this Image<TPixel> image, float h, float s, float v) where TPixel : unmanaged, IPixel<TPixel>
            => Worker(image, f =>
            {
                for (int y = 0; y < f.Height; y++)
                {
                    for (int x = 0; x < f.Width; x++)
                    {
                        var c = f[x, y];
                        c.ColorHSVTransform(h, s, v);
                        f[x, y] = c;
                    }
                }
            });
        #endregion

        #region Color Graying
        public static void ColorGraying<TPixel>(ref this TPixel pixel) where TPixel : unmanaged, IPixel<TPixel>
        {
            Rgba32 originalRGBA32 = new();
            pixel.ToRgba32(ref originalRGBA32);
            byte rgb = Convert.ToByte(0.299 * originalRGBA32.R + 0.587 * originalRGBA32.G + 0.114 * originalRGBA32.B);
            Rgba32 translateRGBA32 = new(rgb, rgb, rgb, originalRGBA32.A);
            pixel.FromRgba32(translateRGBA32);
        }

        public static Image<TPixel> ColorGraying<TPixel>(this Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
            => Worker(image, f =>
            {
                for (int y = 0; y < f.Height; y++)
                {
                    for (int x = 0; x < f.Width; x++)
                    {
                        var c = f[x, y];
                        c.ColorGraying();
                        f[x, y] = c;
                    }
                }
            });
        #endregion

        #region Reverse Color
        public static void ColorReverse<TPixel>(ref this TPixel pixel) where TPixel : unmanaged, IPixel<TPixel>
        {
            Rgba32 originalRGBA32 = new();
            pixel.ToRgba32(ref originalRGBA32);
            Rgba32 translateRGBA32 = new((255 - originalRGBA32.R) / 255f, (255 - originalRGBA32.G) / 255f, (255 - originalRGBA32.B) / 255f, originalRGBA32.A);
            pixel.FromRgba32(translateRGBA32);
        }

        public static Image<TPixel> ColorReverse<TPixel>(this Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
            => Worker(image, f =>
            {
                for (int y = 0; y < f.Height; y++)
                {
                    for (int x = 0; x < f.Width; x++)
                    {
                        var c = f[x, y];
                        c.ColorReverse();
                        f[x, y] = c;
                    }
                }
            });
        #endregion
        #endregion
    }
}