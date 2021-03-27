using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ritsukage.Library.Pixiv.Model
{
    public class Illust
    {
        public bool IsUgoira { get; init; }

        public int Id { get; init; }

        public string Title { get; init; }

        public string Caption { get; init; }

        public IllustAuthor Author { get; init; }

        public DateTime CreateDate { get; init; }

        public int PageCount { get; init; }

        public ImageUrls[] Images { get; init; }

        public Tags[] Tags { get; init; }

        public int TotalView { get; init; }

        public int TotalBookmarks { get; init; }

        public int TotalComments { get; init; }

        public string Url => "https://www.pixiv.net/artworks/" + Id;

        public async Task<UgoiraMetadata> GetUgoiraMetadata()
            => await Task.Run(() =>
            {
                var data = Hibi.HibiPixiv.GetIllustUgoiraMetadata(Id);
                if (data != null)
                    return new UgoiraMetadata(data["ugoira_metadata"]);
                else
                    return new UgoiraMetadata();
            });

        static readonly Regex HtmlTagParser = new Regex(@"<[^>]+>");
        public Illust(JToken data)
        {
            IsUgoira = (string)data["type"] == "ugoira";
            Id = (int)data["id"];
            Title = (string)data["title"];
            Caption = Escape(RemoveEmptyLine(HtmlTagParser.Replace((string)data["caption"], (s) =>
            {
                var text = s.Value;
                if (text == "<br />")
                    return Environment.NewLine;
                else
                    return "";
            })));
            Author = new(data["user"]);
            CreateDate = Convert.ToDateTime((string)data["create_date"], new DateTimeFormatInfo()
            {
                FullDateTimePattern = "yyyy-MM-ddTHH:mm:sszzz"
            });
            PageCount = (int)data["page_count"];
            List<ImageUrls> images = new();
            if (PageCount > 1)
                foreach (var image in (JArray)data["meta_pages"])
                    images.Add(new(image["image_urls"]));
            else
                images.Add(new()
                {
                    SquareMedium = (string)data["image_urls"]["square_medium"],
                    Medium = (string)data["image_urls"]["medium"],
                    Large = (string)data["image_urls"]["large"],
                    Original = (string)data["meta_single_page"]["original_image_url"]
                });
            Images = images.ToArray();
            List<Tags> tags = new();
            foreach (var tag in (JArray)data["tags"])
                tags.Add(new(tag));
            Tags = tags.ToArray();
            TotalView = (int)data["total_view"];
            TotalBookmarks = (int)data["total_bookmarks"];
            TotalComments = (int)data["total_comments"];

        }

        public override string ToString()
            => new StringBuilder().AppendLine()
                    .AppendLine(Title)
                    .AppendLine($"Author: {Author}")
                    .AppendLine(Caption)
                    .AppendLine($"Tags: {string.Join(" | ", Tags)}")
                    .AppendLine($"Publish Date: {CreateDate:yyyy-MM-dd HH:mm:ss}")
                    .AppendLine($"Bookmarks: {TotalBookmarks} Comments:{TotalComments} Views:{TotalView}")
                    .Append(Url)
                    .ToString();

        public static async Task<Illust> Get(int id)
            => await Task.Run(() =>
            {
                var data = Hibi.HibiPixiv.GetIllustDetail(id);
                if (data == null)
                    return null;
                else
                    return new Illust(data["illust"]);
            });

        static string RemoveEmptyLine(string text)
            => string.Join(Environment.NewLine, text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .GroupBy(x => x).Select(x => x.Key).ToArray());

        public static string Escape(string s) => System.Web.HttpUtility.HtmlDecode(s);
    }

    public struct UgoiraMetadata
    {
        public string ZipUrl { get; init; }
        public UgoiraMetadataGifFrame[] Frames { get; init; }

        public UgoiraMetadata(JToken data)
        {
            ZipUrl = (string)data["zip_urls"]["medium"];
            List<UgoiraMetadataGifFrame> frames = new();
            foreach (var frame in (JArray)data["frames"])
                frames.Add(new(frame));
            Frames = frames.ToArray();
        }
    }

    public struct UgoiraMetadataGifFrame
    {
        public string File { get; init; }
        public int Delay { get; init; }

        public UgoiraMetadataGifFrame(JToken data) : this((string)data["file"], (int)data["delay"]) { }

        public UgoiraMetadataGifFrame(string file, int delay)
        {
            File = file;
            Delay = delay;
        }
    }

    public struct ImageUrls
    {
        public string SquareMedium { get; init; }
        public string Medium { get; init; }
        public string Large { get; init; }
        public string Original { get; init; }

        public ImageUrls(JToken data) : this((string)data["square_medium"], (string)data["medium"], (string)data["large"], (string)data["original"]) { }

        public ImageUrls(string square_medium, string medium, string large, string original)
        {
            SquareMedium = square_medium;
            Medium = medium;
            Large = large;
            Original = original;
        }

        public override string ToString()
            => Medium;

        public static string ToPixivCat(string url)
            => url.Replace("https://i.pximg.net", "https://i.pixiv.cat");
    }

    public struct IllustAuthor
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Account { get; init; }
        public string ProfileImage { get; init; }

        public string Url => "https://www.pixiv.net/member.php?id=" + Id;

        public IllustAuthor(JToken data)
            : this((int)data["id"], (string)data["name"], (string)data["account"], (string)data["profile_image_urls"]["medium"]) { }

        public IllustAuthor(int id, string name, string account, string profile_image)
        {
            Id = id;
            Name = name;
            Account = account;
            ProfileImage = profile_image;
        }

        public override string ToString()
            => $"{Name} ({Url})";
    }

    public struct Tags
    {
        public string Name { get; init; }
        public string TranslatedName { get; init; }

        public Tags(JToken data) : this((string)data["name"], (string)data["translated_name"]) { }

        public Tags(string name, string translated_name)
        {
            Name = name;
            TranslatedName = translated_name;
        }

        public override string ToString()
            => string.IsNullOrWhiteSpace(TranslatedName) ? Name : TranslatedName;
    }
}
