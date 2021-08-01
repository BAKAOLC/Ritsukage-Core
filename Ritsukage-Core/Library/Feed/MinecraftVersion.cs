using Microsoft.Toolkit.Parsers.Rss;
using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ritsukage.Library.Feed
{
    public class MinecraftVersion : FeedReader
    {
        public MinecraftVersion()
            : base("https://rsshub.app/minecraft/version",
                  "https://rsshub-indol-omega.vercel.app/minecraft/version")
        { }

        const string MojangMeta = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public override Task<IEnumerable<RssSchema>> Read()
        {
            IEnumerable<RssSchema> rss = null;
            try
            {
                var jdata = JObject.Parse(Utils.HttpGET(MojangMeta));
                var sb = new StringBuilder();
                sb.AppendLine("<rss xmlns:atom=\"http://www.w3.org/2005/Atom\" version=\"2.0\">");
                sb.AppendLine("<channel>");
                sb.AppendLine("<title><![CDATA[ Minecraft Java版游戏更新 ]]></title>");
                sb.AppendLine("<link>https://www.minecraft.net/</link>");
                sb.AppendLine("<description><![CDATA[ Minecraft Java版游戏更新 - Made with love by Ritsukage-Core]]></description>");
                sb.AppendLine("<generator>Ritsukage-Core</generator>");
                sb.AppendLine("<language>zh-cn</language>");
                sb.AppendLine($"<lastBuildDate>{DateTime.Now:R}T</lastBuildDate>");
                foreach (var version in (JArray)jdata["versions"])
                {
                    sb.AppendLine("<item>");
                    var v = (string)version["id"];
                    switch ((string)version["type"])
                    {
                        case "snapshot":
                            sb.AppendLine($"<title><![CDATA[ {v} 快照更新 ]]></title>");
                            sb.AppendLine($"<description><![CDATA[ {v} 快照更新 ]]></description>");
                            break;
                        case "release":
                            sb.AppendLine($"<title><![CDATA[ {v} 正式版更新 ]]></title>");
                            sb.AppendLine($"<description><![CDATA[ {v} 正式版更新 ]]></description>");
                            break;
                        case "old_alpha":
                            sb.AppendLine($"<title><![CDATA[ {v} 过时的预览版更新 ]]></title>");
                            sb.AppendLine($"<description><![CDATA[ {v} 过时的预览版更新 ]]></description>");
                            break;
                        case "old_beta":
                            sb.AppendLine($"<title><![CDATA[ {v} 过时的测试版更新 ]]></title>");
                            sb.AppendLine($"<description><![CDATA[ {v} 过时的测试版更新 ]]></description>");
                            break;
                    }
                    sb.AppendLine($"<pubDate>{Convert.ToDateTime((string)version["releaseTime"]):R}</pubDate>");
                    sb.AppendLine($"<guid isPermaLink=\"false\">{(string)version["url"]}</guid>");
                    sb.AppendLine("<link>https://www.minecraft.net/</link>");
                    sb.AppendLine("</item>");
                }
                sb.AppendLine("</channel>");
                sb.Append("</rss>");
                var parser = new RssParser();
                rss = parser.Parse(sb.ToString());
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Feed", "Failed to format json data from "
                    .CreateStringBuilder().AppendLine(MojangMeta)
                    .Append(ex.GetFormatString()).ToString());
            }
            if (rss == null)
                return base.Read();
            else
                return Task.FromResult(rss);
        }
    }
}
