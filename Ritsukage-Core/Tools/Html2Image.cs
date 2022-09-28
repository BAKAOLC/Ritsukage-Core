using NetCoreHTMLToImage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace Ritsukage.Tools
{
    public static class Html2Image
    {
        public static Image<Rgba32> FromHtmlString(string html)
        {
            var bytes = new HtmlConverter().FromHtmlString(html);
            return Image.Load<Rgba32>(new MemoryStream(bytes));
        }

        public static Image<Rgba32> FromUrl(string url)
            => FromHtmlString(Utils.HttpGET(url));
    }
}
