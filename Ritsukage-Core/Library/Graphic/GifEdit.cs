using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace Ritsukage.Library.Graphic
{
    public static class GifEdit
    {
        public static Image<Rgba32> ReadGif(Stream stream)
        {
            var decoder = new GifDecoder
            {
                DecodingMode = FrameDecodingMode.All
            };
            var gif = decoder.Decode<Rgba32>(new Configuration(), stream);
            stream.Dispose();
            return gif;
        }
        public static Image<Rgba32> ReadGif(string path)
            => ReadGif(File.OpenRead(path));

        public static void SaveGif(Image<Rgba32> gif, string path)
        {
            var stream = File.OpenWrite(path);
            var encoder = new GifEncoder
            {
                ColorTableMode = GifColorTableMode.Local
            };
            encoder.Encode(gif, stream);
            stream.Dispose();
        }

        public static Image<Rgba32> CreateReverse(Image<Rgba32> original)
        {
            var gif = new Image<Rgba32>(new Configuration(new GifConfigurationModule()),
                original.Width, original.Height);
            gif.Metadata.GetGifMetadata().RepeatCount = 0;
            foreach (var frame in original.Frames)
                gif.Frames.InsertFrame(0, frame);
            gif.Frames.RemoveFrame(gif.Frames.Count - 1);
            return gif;
        }

        public static Image<Rgba32> CreateMoveLeft(Image<Rgba32> original)
        {
            var gif = new Image<Rgba32>(new Configuration(new GifConfigurationModule()),
                original.Width, original.Height);
            gif.Metadata.GetGifMetadata().RepeatCount = 0;
            for (var i = 0; i < original.Frames.Count; i++)
            {
                var image = original.Frames.CloneFrame(i);
                MovePixel(ref image, Lerp(i, original.Frames.Count, original.Height), 0);
                gif.Frames.AddFrame(image.Frames[0]);
            }
            gif.Frames.RemoveFrame(0);
            return gif;
        }

        public static Image<Rgba32> CreateMoveRight(Image<Rgba32> original)
        {
            var gif = new Image<Rgba32>(new Configuration(new GifConfigurationModule()),
                original.Width, original.Height);
            gif.Metadata.GetGifMetadata().RepeatCount = 0;
            for (var i = 0; i < original.Frames.Count; i++)
            {
                var image = original.Frames.CloneFrame(i);
                MovePixel(ref image, -Lerp(i, original.Frames.Count, original.Width), 0);
                gif.Frames.AddFrame(image.Frames[0]);
            }
            gif.Frames.RemoveFrame(0);
            return gif;
        }

        public static Image<Rgba32> CreateMoveUp(Image<Rgba32> original)
        {
            var gif = new Image<Rgba32>(new Configuration(new GifConfigurationModule()),
                original.Width, original.Height);
            gif.Metadata.GetGifMetadata().RepeatCount = 0;
            for (var i = 0; i < original.Frames.Count; i++)
            {
                var image = original.Frames.CloneFrame(i);
                MovePixel(ref image, 0, Lerp(i, original.Frames.Count, original.Height));
                gif.Frames.AddFrame(image.Frames[0]);
            }
            gif.Frames.RemoveFrame(0);
            return gif;
        }

        public static Image<Rgba32> CreateMoveDown(Image<Rgba32> original)
        {
            var gif = new Image<Rgba32>(new Configuration(new GifConfigurationModule()),
                original.Width, original.Height);
            gif.Metadata.GetGifMetadata().RepeatCount = 0;
            for (var i = 0; i < original.Frames.Count; i++)
            {
                var image = original.Frames.CloneFrame(i);
                MovePixel(ref image, 0, -Lerp(i, original.Frames.Count, original.Height));
                gif.Frames.AddFrame(image.Frames[0]);
            }
            gif.Frames.RemoveFrame(0);
            return gif;
        }

        static void MovePixel(ref Image<Rgba32> image, int dx, int dy)
        {
            var original = image.Clone();
            for (int x = 0; x < original.Width; x++)
                for (int y = 0; y < original.Height; y++)
                    image[x, y] = original[Mod(x + dx, original.Width), Mod(y + dy, original.Height)];
            original.Dispose();
        }

        static int Lerp(int i, int n, int total)
            => Convert.ToInt32(Math.Ceiling(i * ((double)total / n)));

        static int Mod(int x, int mod)
        {
            x %= mod;
            return x < 0 ? x + mod : x;
        }
    }
}
