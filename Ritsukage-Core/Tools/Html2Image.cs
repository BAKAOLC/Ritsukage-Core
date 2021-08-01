using NetCoreHTMLToImage;
using System.IO;

namespace Ritsukage.Tools
{
    public static class Html2Image
    {
        public static BaseImage FromHtmlString(string html)
        {
            var bytes = new HtmlConverter().FromHtmlString(html);
            return new MemoryImage(new MemoryStream(bytes));
        }

        public static BaseImage FromUrl(string url)
            => FromHtmlString(Utils.HttpGET(url));
    }
}
