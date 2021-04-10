using HtmlAgilityPack;
using Ritsukage.Tools;
using System;

namespace Ritsukage.Library.Minecraft.Changelog
{
    public class Article
    {
        public string Title { get; init; }

        public string Html { get; init; }

        public string Markdown { get; init; }

        public Article(string url)
        {
            var html = new HtmlDocument();
            html.LoadHtml(Utils.HttpGET(url));
            var container = html.GetElementbyId("article-container");
            var header = container.SelectSingleNode("article/header");
            Title = header?.InnerText;
            var article = container.SelectSingleNode("article/section/div/div[1]");
            Html = header?.InnerHtml + article?.InnerHtml;
            Markdown = new ReverseMarkdown.Converter().Convert(Html);
        }

        public override string ToString() => Utils.RemoveEmptyLine(Markdown)
            .Replace(Environment.NewLine, "  " + Environment.NewLine);
    }
}
