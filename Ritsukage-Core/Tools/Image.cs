using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Net;

namespace Ritsukage.Tools
{
    public class BaseImage
    {
        public Bitmap Source { get; set; }

        public Size Size { get => Source.Size; }

        public int Width { get => Source.Width; }

        public int Height { get => Source.Height; }

        public PixelFormat PixelFormat { get => Source.PixelFormat; }

        public ImageFormat ImageFormat = ImageFormat.Png;

        public string ImageFormatString
        {
            get => ImageExtensions.ImageFormatToString(ImageFormat);
            set => ImageFormat = ImageExtensions.StringToImageFormat(value);
        }

        public string DataUriScheme { get => MimeMapping.GetMimeMapping(ImageFormatString); }

        public string ToBase64()
        {
            using MemoryStream ms = new MemoryStream();
            Source.Save(ms, ImageFormat);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();
            return Convert.ToBase64String(arr);
        }

        public string ToBase64File() => "base64://" + ToBase64();

        public string ToBase64Source() => $"data:{DataUriScheme};base64," + ToBase64();

        public BaseImage DrawText(float x, float y, string text, string type = "黑体", float size = 9, int r = 0, int g = 0, int b = 0, float angle = 0)
        {
            using Graphics pic = GetGraphics();
            using Font font = new Font(type, size);
            Color myColor = Color.FromArgb(r, g, b);
            PointF pos = new PointF(x, y);
            if (angle != 0)
            {
                Matrix matrix = pic.Transform;
                matrix.RotateAt(angle, pos);
                pic.Transform = matrix;
            }
            using SolidBrush myBrush = new SolidBrush(myColor);
            pic.DrawString(text, font, myBrush, pos);
            return this;
        }

        public BaseImage DrawRectangle(float x, float y, float width, float height, int r = 0, int g = 0, int b = 0)
        {
            using Graphics pic = GetGraphics();
            Color myColor = Color.FromArgb(r, g, b);
            using SolidBrush myBrush = new SolidBrush(myColor);
            pic.FillRectangle(myBrush, new RectangleF(x, y, width, height));
            return this;
        }

        public BaseImage DrawEllipse(float x, float y, int width, int height, int r = 0, int g = 0, int b = 0)
        {
            using Graphics pic = GetGraphics();
            Color myColor = Color.FromArgb(r, g, b);
            using Pen myBrush = new Pen(myColor);
            pic.DrawEllipse(myBrush, new RectangleF(x, y, width, height));
            return this;
        }

        public BaseImage DrawImage(string path, float x, float y, float width = 0, float height = 0)
        {
            if (!File.Exists(path))
                return this;
            using Bitmap b = new Bitmap(path);
            using Graphics pic = GetGraphics();
            if (width != 0 && height != 0)
                pic.DrawImage(b, x, y, width, height);
            else if (width == 0 && height == 0)
                pic.DrawImage(b, x, y);
            return this;
        }

        public BaseImage SetImageSize(int width, int height)
        {
            int basex = width < 0 ? -width : 0;
            int basey = height < 0 ? -height : 0;
            Bitmap img = new Bitmap(Math.Abs(width), Math.Abs(height));
            Graphics pic = GetGraphics(img);
            pic.DrawImage(Source, basex, basey, width, height);
            Source = img;
            return this;
        }

        public BaseImage CutImage(int x, int y, int width, int height)
        {
            int basex = width < 0 ? x - width : x;
            int basey = height < 0 ? y - height : y;
            int dx = width < 0 ? -1 : 1;
            int dy = height < 0 ? -1 : 1;
            Bitmap img = new Bitmap(Math.Abs(width), Math.Abs(height));
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    if (basex + dx * i >= 0 && basex + dx * i < Source.Width && basey + dy * j > 0 && basey + dy * j < Source.Height)
                        img.SetPixel(i, j, Source.GetPixel(basex + dx * i, basey + dy * j));
                }
            }
            Source = img;
            return this;
        }

        public BaseImage LeftRotateImage()
        {
            Bitmap img = new Bitmap(Height, Width);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    img.SetPixel(y, Width - x - 1, Source.GetPixel(x, y));
                }
            }
            Source = img;
            return this;
        }

        public BaseImage RightRotateImage()
        {
            Bitmap img = new Bitmap(Height, Width);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    img.SetPixel(Height - y - 1, x, Source.GetPixel(x, y));
                }
            }
            Source = img;
            return this;
        }

        public BaseImage TranslatePixel(int dx, int dy)
        {
            Bitmap img = new Bitmap(Width, Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    img.SetPixel(x, y, Source.GetPixel((Width + x - dx) % Width, (Height + y - dy) % Height));
                }
            }
            Source = img;
            return this;
        }

        public BaseImage TranslateHSV(float h, float s, float v)
        {
            ImageExtensions.HSVColor hsv;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    hsv = ImageExtensions.HSVColor.FromRGB(Source.GetPixel(x, y));
                    hsv.hue += h;
                    hsv.hue %= 360;
                    hsv.hue = hsv.hue < 0 ? 360 + hsv.hue : hsv.hue;
                    hsv.saturation += s;
                    hsv.saturation = Math.Min(Math.Max(hsv.saturation, 0), 100);
                    hsv.value += v;
                    hsv.value = Math.Min(Math.Max(hsv.value, 0), 100);
                    Source.SetPixel(x, y, hsv.ToRGB(Source.GetPixel(x, y).A));
                }
            }
            return this;
        }

        public BaseImage GetGrayImage()
        {
            int rgb;
            Color c;
            BaseImage img = Clone();
            Bitmap bmp = img.Source;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    c = bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
                    bmp.SetPixel(x, y, Color.FromArgb(c.A, rgb, rgb, rgb));
                }
            }
            return img;
        }

        public BaseImage GetAntiColorImage()
        {
            Color c;
            BaseImage img = Clone();
            Bitmap bmp = img.Source;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    c = bmp.GetPixel(x, y); ;
                    bmp.SetPixel(x, y, Color.FromArgb(c.A, 255 - c.R, 255 - c.G, 255 - c.B));
                }
            }
            return img;
        }

        private Graphics GetGraphics(Bitmap img = null)
        {
            img ??= Source;
            Graphics pic = Graphics.FromImage(img);
            pic.SmoothingMode = SmoothingMode.HighQuality;
            pic.CompositingQuality = CompositingQuality.HighQuality;
            pic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            pic.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            return pic;
        }

        public string Save(string name, string path = "")
        {
            if (string.IsNullOrWhiteSpace(path))
                path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            if (!name.Contains("."))
                name += ImageExtensions.ImageFormatToString(ImageFormat);

            path = Path.Combine(path, name);
            Source.Save(path, ImageFormat);
            return path;
        }

        public string SaveAndDispose(string name)
        {
            string result = Save(name);
            Source.Dispose();
            return result;
        }

        public string SaveTempFile() => Save(Path.GetTempFileName());

        public BaseImage Clone()
        {
            BaseImage img = new BaseImage
            {
                Source = (Bitmap)Source.Clone(),
                ImageFormat = ImageFormat
            };
            return img;
        }
    }

    public class EmptyImage : BaseImage
    {
        public EmptyImage(int Width, int Height)
        {
            Source = new Bitmap(Width, Height);
        }
    }

    public class MemoryImage : BaseImage
    {
        public MemoryImage(Image image)
        {
            Source = (Bitmap)image;
        }

        public MemoryImage(Stream stream)
        {
            Source = (Bitmap)Image.FromStream(stream);
        }
    }

    public class FileImage : BaseImage
    {
        public FileImage(string path)
        {
            Source = new Bitmap(path);
        }
    }

    public class NetworkImage : BaseImage
    {
        public NetworkImage(string url)
        {
            Source = (Bitmap)url.GetImageFromNet().Clone();
        }
    }

    public class ColorTools
    {
        private int a = 255;
        private int r = 0;
        private int g = 0;
        private int b = 0;

        public ColorTools(Color c)
        {
            A = c.A;
            R = c.R;
            G = c.G;
            B = c.B;
        }

        public ColorTools(int R, int G, int B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public ColorTools(int A, int R, int G, int B)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public int A
        {
            get => a;
            set
            {
                a = value % 256;
                a = a < 0 ? 256 + a : a;
            }
        }

        public int R
        {
            get => r;
            set
            {
                r = value % 256;
                r = r < 0 ? 256 + r : r;
            }
        }

        public int G
        {
            get => g;
            set
            {
                g = value % 256;
                g = g < 0 ? 256 + g : g;
            }
        }

        public int B
        {
            get => b;
            set
            {
                b = value % 256;
                b = b < 0 ? 256 + b : b;
            }
        }

        public void TranslateHSV(int h = 0, int s = 0, int v = 0)
        {
            var hsv = ImageExtensions.HSVColor.FromRGB(GetColor());
            hsv.hue += h;
            hsv.hue %= 360;
            hsv.hue = hsv.hue < 0 ? 360 + hsv.hue : hsv.hue;
            hsv.saturation += s;
            hsv.saturation = Math.Min(Math.Max(hsv.saturation, 0), 100);
            hsv.value += v;
            hsv.value = Math.Min(Math.Max(hsv.value, 0), 100);
            var rgb = hsv.ToRGB();
            R = rgb.R;
            G = rgb.G;
            B = rgb.B;
        }

        public void ToGray()
        {
            int rgb = (int)Math.Round(0.299 * R + 0.587 * G + 0.114 * B);
            R = rgb;
            G = rgb;
            B = rgb;
        }

        public void ToAntiColor()
        {
            R = 255 - R;
            G = 255 - G;
            B = 255 - B;
        }

        public Color GetColor() => Color.FromArgb(a, r, g, b);
    }

    public static class ImageExtensions
    {
        public static Image GetImageFromNet(this string url, Action<WebRequest> requestAction = null, Func<WebResponse, Image> responseFunc = null)
        {
            return new Uri(url).GetImageFromNet(requestAction, responseFunc);
        }

        public static Image GetImageFromNet(this Uri url, Action<WebRequest> requestAction = null, Func<WebResponse, Image> responseFunc = null)
        {
            Image img;
            try
            {
                WebRequest request = WebRequest.Create(url);
                requestAction?.Invoke(request);
                using WebResponse response = request.GetResponse();
                if (responseFunc != null)
                {
                    img = responseFunc(response);
                }
                else
                {
                    img = Image.FromStream(response.GetResponseStream());
                }
            }
            catch
            {
                img = null;
            }
            return img;
        }

        public static string ImageFormatToString(ImageFormat format)
        {
            if (format.Equals(ImageFormat.Jpeg))
                return ".jpg";
            else if (format.Equals(ImageFormat.Png))
                return ".png";
            else if (format.Equals(ImageFormat.Gif))
                return ".gif";
            else if (format.Equals(ImageFormat.Bmp))
                return ".bmp";
            else if (format.Equals(ImageFormat.Icon))
                return ".ico";
            else
                return string.Empty;
        }

        public static ImageFormat StringToImageFormat(string format)
        {
            return format switch
            {
                ".jpg" => ImageFormat.Jpeg,
                ".jpeg" => ImageFormat.Jpeg,
                ".png" => ImageFormat.Png,
                ".gif" => ImageFormat.Gif,
                ".bmp" => ImageFormat.Bmp,
                ".ico" => ImageFormat.Icon,
                _ => null,
            };
        }

        public static SizeF MeasureString(string font, float size, string text) => new Font(font, size).MeasureString(text);

        public static SizeF MeasureString(this Font font, string text)
        {
            using Bitmap img = new Bitmap(1, 1);
            return Graphics.FromImage(img).MeasureString(text, font);
        }

        public static SizeF Rotate(this SizeF sizeF, float angle)
        {
            float width = (float)(sizeF.Width * Math.Cos(angle) + sizeF.Height * Math.Sin(angle));
            float height = (float)(sizeF.Height * Math.Cos(angle) + sizeF.Width * Math.Sin(angle));
            sizeF.Width = width;
            sizeF.Height = height;
            return sizeF;
        }

        public struct HSVColor
        {
            public float hue;
            public float saturation;
            public float value;

            public HSVColor(float h, float s, float v)
            {
                hue = h;
                saturation = s;
                value = v;
            }

            public Color ToRGB(int alpha = 255)
            {
                hue -= Convert.ToSingle(Math.Floor(hue / 360) * 360);
                saturation /= 100;
                value /= 100;
                byte v = Convert.ToByte(value * 255);
                if (saturation == 0)
                {
                    return Color.FromArgb(255, v, v, v);
                }
                int h = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
                float f = hue / 60 - h;
                byte a = Convert.ToByte(v * (1 - saturation));
                byte b = Convert.ToByte(v * (1 - saturation * f));
                byte c = Convert.ToByte(v * (1 - saturation * (1 - f)));
                switch (h)
                {
                    case 0:
                        return Color.FromArgb(alpha, v, c, a);
                    case 1:
                        return Color.FromArgb(alpha, b, v, a);
                    case 2:
                        return Color.FromArgb(alpha, a, v, c);
                    case 3:
                        return Color.FromArgb(alpha, a, b, v);
                    case 4:
                        return Color.FromArgb(alpha, c, a, v);
                    case 5:
                        return Color.FromArgb(alpha, v, a, b);
                    default:
                        throw new NotImplementedException();
                }
            }

            public static HSVColor FromRGB(Color RGB)
            {
                HSVColor hsv = new HSVColor();
                byte max = Math.Max(RGB.R, RGB.G);
                max = Math.Max(max, RGB.B);
                byte min = Math.Min(RGB.R, RGB.G);
                min = Math.Min(min, RGB.B);
                hsv.value = ((float)max) / 255;
                int mm = max - min;
                if (max == 0)
                {
                    hsv.saturation = 0;
                }
                else
                {
                    hsv.saturation = ((float)mm) / max;
                }
                if (mm == 0)
                {
                    hsv.hue = 0;
                }
                else if (RGB.R == max)
                {
                    hsv.hue = ((float)(RGB.G - RGB.B)) / mm * 60;
                }
                else if (RGB.G == max)
                {
                    hsv.hue = 120 + ((float)(RGB.B - RGB.R)) / mm * 60;
                }
                else if (RGB.B == max)
                {
                    hsv.hue = 240 + ((float)(RGB.R - RGB.G)) / mm * 60;
                }
                if (hsv.hue < 0) hsv.hue += 360;
                hsv.saturation *= 100;
                hsv.value *= 100;
                return hsv;
            }
        }
    }
}
