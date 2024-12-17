using System;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Ritsukage.Library.Graphic
{
    public static class GraphicEdit
    {
        public static Image<Rgba32> ReserveGifFrames(Image<Rgba32> original)
        {
            var gif = original.CloneEmpty();
            for (var i = original.Frames.Count - 1; i >= 0; i--)
                gif.Frames.AddFrame(original.Frames.CloneFrame(i).Frames.RootFrame);
            gif.Frames.RemoveFrame(0);
            gif.FixGifRepeatCount();
            return gif;
        }

        public static Image<Rgba32> SetGifLeftMotion(Image<Rgba32> original)
        {
            var gif = original.CloneEmpty();
            for (var i = 0; i < original.Frames.Count; i++)
            {
                var image = original.Frames.CloneFrame(i);
                MovePixel(ref image, Lerp(i, original.Frames.Count, original.Height), 0);
                gif.Frames.AddFrame(image.Frames.RootFrame);
            }

            gif.Frames.RemoveFrame(0);
            gif.FixGifRepeatCount();
            return gif;
        }

        public static Image<Rgba32> SetGifRightMotion(Image<Rgba32> original)
        {
            var gif = original.CloneEmpty();
            for (var i = 0; i < original.Frames.Count; i++)
            {
                var image = original.Frames.CloneFrame(i);
                MovePixel(ref image, -Lerp(i, original.Frames.Count, original.Width), 0);
                gif.Frames.AddFrame(image.Frames.RootFrame);
            }

            gif.Frames.RemoveFrame(0);
            gif.FixGifRepeatCount();
            return gif;
        }

        public static Image<Rgba32> SetGifUpMotion(Image<Rgba32> original)
        {
            var gif = original.CloneEmpty();
            for (var i = 0; i < original.Frames.Count; i++)
            {
                var image = original.Frames.CloneFrame(i);
                MovePixel(ref image, 0, Lerp(i, original.Frames.Count, original.Height));
                gif.Frames.AddFrame(image.Frames.RootFrame);
            }

            gif.Frames.RemoveFrame(0);
            gif.FixGifRepeatCount();
            return gif;
        }

        public static Image<Rgba32> SetGifDownMotion(Image<Rgba32> original)
        {
            var gif = original.CloneEmpty();
            for (var i = 0; i < original.Frames.Count; i++)
            {
                var image = original.Frames.CloneFrame(i);
                MovePixel(ref image, 0, -Lerp(i, original.Frames.Count, original.Height));
                gif.Frames.AddFrame(image.Frames.RootFrame);
            }

            gif.Frames.RemoveFrame(0);
            gif.FixGifRepeatCount();
            return gif;
        }

        public static Image<Rgba32> MirrorLeft(Image<Rgba32> image)
        {
            return Worker(image, _MirrorLeft);
        }

        public static Image<Rgba32> MirrorRight(Image<Rgba32> image)
        {
            return Worker(image, _MirrorRight);
        }

        public static Image<Rgba32> MirrorTop(Image<Rgba32> image)
        {
            return Worker(image, _MirrorTop);
        }

        public static Image<Rgba32> MirrorBottom(Image<Rgba32> image)
        {
            return Worker(image, _MirrorBottom);
        }

        public static Image<Rgba32> FillCircleOutRangeColor(Image<Rgba32> image, int size = 0, Rgba32 color = default)
        {
            return Worker(image, x => _FillCircleOutRangeColor(x, size, color));
        }

        public static Image<Rgba32> RotateSymmetry(Image<Rgba32> image, float degrees = 90)
        {
            return Worker(image, x => _RotateSymmetry(x, degrees));
        }

        public static Image<Rgba32> Mosaic(Image<Rgba32> image, int size = 2, int px = 0, int py = 0)
        {
            return Worker(image, x => _Mosaic(x, size, px, py));
        }

        public static Image<Rgba32> Rotate90(Image<Rgba32> image)
        {
            return _Rotate90(image);
        }

        public static Image<Rgba32> Rotate180(Image<Rgba32> image)
        {
            return _Rotate180(image);
        }

        public static Image<Rgba32> Rotate270(Image<Rgba32> image)
        {
            return _Rotate270(image);
        }

        public static Image<Rgba32> Rotate(Image<Rgba32> image, float degrees)
        {
            return _Rotate(image, degrees);
        }

        public static Image<Rgba32> RotateWithOriginalSize(Image<Rgba32> image, float degrees)
        {
            return _RotateWithOriginalSize(image, degrees);
        }

        public static Image<Rgba32> GenerateRotateImage(Image<Rgba32> image, int repeat = 1, int frameDelay = 1)
        {
            var total = image.Frames.Count * repeat;
            var size = new Size(GetRotateMaxBound(image.Size));
            var oimg = image.Clone();
            oimg.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.BoxPad,
                Position = AnchorPositionMode.Center,
                Size = size,
            }));
            ;
            var img = new Image<Rgba32>(oimg.Width, oimg.Height);
            for (var i = 0; i < total; i++)
            {
                var degrees = 360f * i / total;
                var fn = Mod(i, image.Frames.Count);
                var pimg = oimg.Frames.CloneFrame(fn);
                pimg.Mutate(x => x.Rotate(degrees));
                var dx = (pimg.Width - oimg.Width) / 2;
                var dy = (pimg.Height - oimg.Height) / 2;
                pimg.Mutate(x => x.Crop(new(dx, dy, oimg.Width, oimg.Height)));
                img.Frames.AddFrame(pimg.Frames.RootFrame);
            }

            img.Frames.RemoveFrame(0);
            if (image.Frames.Count == 1)
                for (var i = 0; i < total; i++)
                    img.Frames[i].Metadata.GetGifMetadata().FrameDelay = frameDelay;
            img.FixGifRepeatCount();
            return img;
        }

        public static Image<Rgba32> GenerateRotateImageWithOriginalSize(Image<Rgba32> image, int repeat = 1,
            int frameDelay = 1)
        {
            var total = image.Frames.Count * repeat;
            var size = new Size(GetRotateMaxBound(image.Size));
            var oimg = image.Clone();
            oimg.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.BoxPad,
                Position = AnchorPositionMode.Center,
                Size = size,
            }));
            ;
            var img = new Image<Rgba32>(image.Width, image.Height);
            for (var i = 0; i < total; i++)
            {
                var degrees = 360f * i / total;
                var fn = Mod(i, image.Frames.Count);
                var pimg = oimg.Frames.CloneFrame(fn);
                pimg.Mutate(x => x.Rotate(degrees));
                var dx = (pimg.Width - image.Width) / 2;
                var dy = (pimg.Height - image.Height) / 2;
                pimg.Mutate(x => x.Crop(new(dx, dy, image.Width, image.Height)));
                img.Frames.AddFrame(pimg.Frames.RootFrame);
            }

            img.Frames.RemoveFrame(0);
            if (image.Frames.Count == 1)
                for (var i = 0; i < total; i++)
                    img.Frames[i].Metadata.GetGifMetadata().FrameDelay = frameDelay;
            img.FixGifRepeatCount();
            return img;
        }

        public static Image<Rgba32> MergeNinePicture(Image<Rgba32>[] imgs)
        {
            if (imgs == null || imgs.Length != 9) return null;
            var first = imgs.First();
            if (imgs.All(x => x.Width == first.Width && x.Height == first.Height))
            {
                var result = new Image<Rgba32>(first.Width * 3, first.Height * 3);
                for (var y = 0; y < 3; y++)
                for (var x = 0; x < 3; x++)
                {
                    var id = y * 3 + x;
                    var px = first.Width * x;
                    var py = first.Height * y;
                    ClonePixel(imgs[id], result, px, py);
                }

                return result;
            }

            return null;
        }

        public static Image<Rgba32>[] SplitNinePicture(Image<Rgba32> img)
        {
            if (img.Width % 3 == 0 && img.Height % 3 == 0)
            {
                var width = img.Width / 3;
                var height = img.Height / 3;
                var imgs = new Image<Rgba32>[9];
                for (var x = 0; x < 3; x++)
                for (var y = 0; y < 3; y++)
                    imgs[y * 3 + x] = CropImage(img, x * width, y * height, width, height);
                return imgs;
            }

            return null;
        }

        private static Image<Rgba32> FixGifRepeatCount(this Image<Rgba32> image)
        {
            image.SetGifRepeatCount(0);
            return image;
        }

        private static Image<Rgba32> Worker(Image<Rgba32> image, Func<Image<Rgba32>, Image<Rgba32>> func)
        {
            if (image.Frames.Count != 1)
            {
                var img = image.Clone();
                for (var i = 0; i < image.Frames.Count; i++)
                {
                    img.Frames.InsertFrame(i, func.Invoke(image.Frames.CloneFrame(i)).Frames.RootFrame);
                    img.Frames.RemoveFrame(i + 1);
                    img.Frames[i].SetGifFrameDelay(image.Frames[i].GetGifFrameDelay());
                }

                return img;
            }

            return func.Invoke(image);
        }

        private static Image<Rgba32> _MirrorLeft(Image<Rgba32> image)
        {
            var img = image.Clone();
            var max_x = image.Width / 2;
            for (var x = 0; x < max_x; x++)
            for (var y = 0; y < image.Height; y++)
                img[image.Width - x - 1, y] = image[x, y];
            return img;
        }

        private static Image<Rgba32> _MirrorRight(Image<Rgba32> image)
        {
            var img = image.Clone();
            var max_x = image.Width / 2;
            for (var x = 0; x < max_x; x++)
            for (var y = 0; y < image.Height; y++)
                img[x, y] = image[image.Width - x - 1, y];
            return img;
        }

        private static Image<Rgba32> _MirrorTop(Image<Rgba32> image)
        {
            var img = image.Clone();
            var max_y = image.Height / 2;
            for (var y = 0; y < max_y; y++)
            for (var x = 0; x < image.Width; x++)
                img[x, image.Height - y - 1] = image[x, y];
            return img;
        }

        private static Image<Rgba32> _MirrorBottom(Image<Rgba32> image)
        {
            var img = image.Clone();
            var max_y = image.Height / 2;
            for (var y = 0; y < max_y; y++)
            for (var x = 0; x < image.Width; x++)
                img[x, y] = image[x, image.Height - y - 1];
            return img;
        }

        private static Image<Rgba32> _FillCircleOutRangeColor(Image<Rgba32> image, int size = 0, Rgba32 color = default)
        {
            if (size <= 0)
                size = Math.Min(image.Width, image.Height);
            double range = size / 2;
            range *= range;
            var img = image.Clone();
            var cx = (double)image.Width / 2;
            var cy = (double)image.Height / 2;
            for (var y = 0; y < image.Height; y++)
            for (var x = 0; x < image.Width; x++)
            {
                var dx = Math.Ceiling(x - cx);
                var dy = Math.Ceiling(y - cy);
                if (dx * dx + dy * dy > range) img[x, y] = color;
            }

            return img;
        }

        private static Image<Rgba32> _Mosaic(Image<Rgba32> image, int size = 2, int px = 0, int py = 0)
        {
            var img = image.Clone();
            px = Mod(px, size);
            py = Mod(py, size);
            for (var y = 0; y < image.Height; y++)
            for (var x = 0; x < image.Width; x++)
            {
                var tx = Math.Min(Convert.ToInt32(Math.Floor((double)x / size)) * size + px,
                    image.Width - 1);
                var ty = Math.Min(Convert.ToInt32(Math.Floor((double)y / size)) * size + py,
                    image.Height - 1);
                img[x, y] = image[tx, ty];
            }

            return img;
        }

        private static Image<Rgba32> _Rotate90(Image<Rgba32> image)
        {
            var img = image.Clone();
            img.Mutate(x => x.Rotate(RotateMode.Rotate90));
            return img;
        }

        private static Image<Rgba32> _Rotate180(Image<Rgba32> image)
        {
            var img = image.Clone();
            img.Mutate(x => x.Rotate(RotateMode.Rotate180));
            return img;
        }

        private static Image<Rgba32> _Rotate270(Image<Rgba32> image)
        {
            var img = image.Clone();
            img.Mutate(x => x.Rotate(RotateMode.Rotate270));
            return img;
        }

        private static Image<Rgba32> _Rotate(Image<Rgba32> image, float degrees)
        {
            var img = image.Clone();
            img.Mutate(x => x.Rotate(degrees));
            return img;
        }

        private static Image<Rgba32> _RotateSymmetry(Image<Rgba32> image, float degrees)
        {
            var rad = degrees * Math.PI / 180;
            var img = image.Clone();
            var cx = img.Width / 2;
            var cy = img.Height / 2;
            for (var x = 0; x < img.Width; x++)
            for (var y = 0; y < img.Height; y++)
            {
                var dx = x - cx;
                var dy = y - cy;
                var rx = dx * Math.Cos(rad) - dy * Math.Sin(rad);
                var ry = dx * Math.Sin(rad) + dy * Math.Cos(rad);
                if (ry <= 0) continue;
                (rx, ry) = (-rx, -ry);
                (rx, ry) = (rx * Math.Cos(-rad) - ry * Math.Sin(-rad),
                    rx * Math.Sin(-rad) + ry * Math.Cos(-rad));
                (dx, dy) = (Convert.ToInt32(rx) + cx, Convert.ToInt32(ry) + cy);
                if (dx < 0 || dx >= img.Width || dy < 0 || dy >= img.Height) continue;
                img[x, y] = image[dx, dy];
            }

            return img;
        }

        private static Image<Rgba32> _RotateWithOriginalSize(Image<Rgba32> image, float degrees)
        {
            var img = image.Clone();
            img.Mutate(x => x.Rotate(degrees));
            var dx = (img.Width - image.Width) / 2;
            var dy = (img.Height - image.Height) / 2;
            img.Mutate(x => x.Crop(new(dx, dy, image.Width, image.Height)));
            return img;
        }

        private static int GetRotateMaxBound(Size size)
        {
            return Convert.ToInt32(Math.Ceiling(Math.Sqrt(size.Width * size.Width + size.Height * size.Height)));
        }

        private static int Mod(int x, int mod)
        {
            x %= mod;
            return x < 0 ? x + mod : x;
        }

        private static int Lerp(int i, int n, int total)
        {
            return Convert.ToInt32(Math.Ceiling(i * ((double)total / n)));
        }

        private static void MovePixel(ref Image<Rgba32> image, int dx, int dy)
        {
            var original = image.Clone();
            for (var x = 0; x < original.Width; x++)
            for (var y = 0; y < original.Height; y++)
                image[x, y] = original[Mod(x + dx, original.Width), Mod(y + dy, original.Height)];
            original.Dispose();
        }

        private static void ClonePixel(Image<Rgba32> source, Image<Rgba32> to, int dx = 0, int dy = 0)
        {
            for (var x = 0; x < source.Width; x++)
            for (var y = 0; y < source.Height; y++)
            {
                var px = x + dx;
                var py = y + dy;
                if (px >= 0 && px < to.Width && py >= 0 && py < to.Height)
                    to[px, py] = source[x, y];
            }
        }

        private static Image<Rgba32> CropImage(Image<Rgba32> source, int x, int y, int width, int height)
        {
            var img = new Image<Rgba32>(width, height);
            for (var px = 0; px < width; px++)
            for (var py = 0; py < height; py++)
            {
                var sx = x + px;
                var sy = y + py;
                if (sx >= 0 && sx < img.Width && sy >= 0 && sy < img.Height) img[px, py] = source[sx, sy];
            }

            return img;
        }
    }
}