using HtmlAgilityPack;
using Ritsukage.Tools;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ritsukage.Library.Minecraft.Changelog
{
    public class ArticleList
    {
        const string Host = "https://feedback.minecraft.net";
        const string MC_Beta = "https://feedback.minecraft.net/hc/en-us/sections/360001185332-Beta-Information-and-Changelogs";
        const string MC_Release = "https://feedback.minecraft.net/hc/en-us/sections/360001186971-Release-Changelogs";
        const string MC_Snapshot = "https://feedback.minecraft.net/hc/en-us/sections/360002267532-Snapshot-Information-and-Changelogs";

        public string Title { get; init; }
        public Dictionary<string, string> Articles { get; init; } = new();

        public ArticleList(string type)
        {
            string url = string.Empty;
            switch (type.ToLower())
            {
                case "beta": url = MC_Beta; break;
                case "release": url = MC_Release; break;
                case "snapshot": url = MC_Snapshot; break;
            }
            if (string.IsNullOrEmpty(url))
                return;
            var html = Utils.HttpGET(url);
            var index = html.IndexOf("<header class=\"page-header\">");
            var titleMatch = Regex.Match(html[index..], "<h1>(?<title>[^<]+)</h1>");
            Title = titleMatch.Groups["title"].Value.Trim();
            var articlesMatch = Regex.Matches(html, "<a href=\"(?<url>[^\"]+)\" class=\"article-list-link\">(?<name>[^<]+)</a>");
            foreach (Match article in articlesMatch)
                if (!Articles.ContainsKey(article.Groups["name"].Value))
                    Articles.Add(article.Groups["name"].Value, Host + article.Groups["url"].Value);
        }
    }
}
