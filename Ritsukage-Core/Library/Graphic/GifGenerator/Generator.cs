using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp.Processing;

namespace Ritsukage.Library.Graphic.GifGenerator
{
    public static class Generator
    {
        private static readonly Rgba32 DefaultBackgroundColor = new(255, 255, 255, 255);

        private const string TemplateFolder = "GifTemplate";

        public static string[] GetTemplateList()
        {
            return !Directory.Exists(TemplateFolder) ? Array.Empty<string>() : Directory.GetDirectories(TemplateFolder);
        }

        public static Image<Rgba32> Generate(Image<Rgba32> image, string template)
        {
            var templatePath = $"{TemplateFolder}/{template}";
            if (!Directory.Exists(templatePath)) throw new DirectoryNotFoundException("目标类型模板不存在");

            if (image.Width != image.Height)
            {
                var size = Math.Max(image.Width, image.Height);
                var temp = new Image<Rgba32>(size, size);
                var x = (size - image.Width) / 2;
                var y = (size - image.Height) / 2;
                temp.Mutate(ctx => ctx.DrawImage(image, new Point(x, y), 1));
                image = temp;
            }

            var i = 0;
            var imgPath = $"{templatePath}/{i}.png";
            var jsonPath = $"{templatePath}/{i}.json";
            Image<Rgba32> result = null;
            while (File.Exists(imgPath) && File.Exists(jsonPath))
            {
                using var temp = Image.Load<Rgba32>(imgPath);
                var json = JObject.Parse(File.ReadAllText(jsonPath));
                var delay = json["delay"]!.Value<int>();
                temp.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = delay;
                var transparent = json["transparent"]?.Value<bool>() ?? false;
                if (json.TryGetValue("rect", out var rect))
                {
                    var p1 = new Point(rect[0]!["x"]!.Value<int>(), rect[0]!["y"]!.Value<int>());
                    var p2 = new Point(rect[1]!["x"]!.Value<int>(), rect[1]!["y"]!.Value<int>());
                    var p3 = new Point(rect[2]!["x"]!.Value<int>(), rect[2]!["y"]!.Value<int>());
                    var p4 = new Point(rect[3]!["x"]!.Value<int>(), rect[3]!["y"]!.Value<int>());
                    DrawImageToTargetByVertex(temp, image, p1, p2, p3, p4,
                        new Rgba32(255, 255, 255, transparent ? 0 : 255));
                }

                if (result is null)
                {
                    result = temp.Clone();
                    result.SetGifRepeatCount(0);
                }
                else
                {
                    result.Frames.AddFrame(temp.Frames.RootFrame);
                }

                i++;
                imgPath = $"{templatePath}/{i}.png";
                jsonPath = $"{templatePath}/{i}.json";
            }

            return result;
        }

        private static Vector2? Intersection(Vector2 begin1, Vector2 end1, Vector2 begin2, Vector2 end2)
        {
            var a1 = end1.Y - begin1.Y;
            var b1 = begin1.X - end1.X;
            var c1 = a1 * begin1.X + b1 * begin1.Y;
            var a2 = end2.Y - begin2.Y;
            var b2 = begin2.X - end2.X;
            var c2 = a2 * begin2.X + b2 * begin2.Y;
            var delta = a1 * b2 - a2 * b1;
            if (delta == 0) return null;
            return new Vector2
            {
                X = (b2 * c1 - b1 * c2) / delta,
                Y = (a1 * c2 - a2 * c1) / delta,
            };
        }

        private static Image<Rgba32> DrawImageToTargetByVertex(Image<Rgba32> target, Image<Rgba32> source, Vector2 p1,
            Vector2 p2, Vector2 p3, Vector2 p4, Rgba32? backgroundColor = null)
        {
            backgroundColor ??= DefaultBackgroundColor;
            var topWidth = p2.X - p1.X;
            var bottomWidth = p3.X - p4.X;
            var leftHeight = p4.Y - p1.Y;
            var rightHeight = p3.Y - p2.Y;
            for (var y = 0; y < target.Height; y++)
            for (var x = 0; x < target.Width; x++)
            {
                var pixel = target[x, y];
                if (pixel.A == 255) continue;
                var u1 = (x - p1.X) / topWidth;
                var u2 = (x - p4.X) / bottomWidth;
                var v1 = (y - p1.Y) / leftHeight;
                var v2 = (y - p2.Y) / rightHeight;
                var intersection = Intersection(new(u1, 0),
                    new(u2, 1),
                    new(0, v1),
                    new(1, v2));
                if (intersection is null) continue;
                if (intersection.Value.X is < 0 or > 1) continue;
                if (intersection.Value.Y is < 0 or > 1) continue;
                var px = (int)((source.Width - 1) * intersection.Value.X);
                var py = (int)((source.Height - 1) * intersection.Value.Y);
                if (px < 0 || px >= source.Width) continue;
                if (py < 0 || py >= source.Height) continue;
                var result = source[px, py];
                if (result == default) result = backgroundColor.Value;
                if (pixel.A != 0)
                {
                    result.R = (byte)((pixel.R * pixel.A + result.R * (255 - pixel.A)) / 255);
                    result.G = (byte)((pixel.G * pixel.A + result.G * (255 - pixel.A)) / 255);
                    result.B = (byte)((pixel.B * pixel.A + result.B * (255 - pixel.A)) / 255);
                }

                target[x, y] = result;
            }

            return target;
        }
    }
}