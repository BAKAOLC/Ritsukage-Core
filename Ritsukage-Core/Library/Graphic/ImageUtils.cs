using Ritsukage.Tools.Console;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Threading.Tasks;

namespace Ritsukage.Library.Graphic
{
    public static class ImageUtils
    {
        public static async void LimitImageScale(string path, int maxWidth, int maxHeight)
        {
            await Task.Run(() =>
            {
                try
                {
                    var image = ImageEdit.LoadImage(path, out IImageFormat format);
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
                        ImageEdit.SaveImage(image, format, path);
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLog.Error("Image Utils", ex.GetFormatString());
                }
            });
        }
    }
}
