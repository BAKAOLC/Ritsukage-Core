using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
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
                if (Program.PixivApi != null)
                {
                    var data = Program.PixivApi.GetAnimatedPictureMetadataAsync(Program.PixivApiToken, Id).Result?.UgoiraMetadata;
                    if (data == null)
                        return new UgoiraMetadata();
                    return new UgoiraMetadata()
                    {
                        ZipUrl = data.ZipUrls.Medium.ToString(),
                        Frames = data.Frames.Select(x => new UgoiraMetadataGifFrame(x.File, x.Delay)).ToArray()
                    };
                }
                return new UgoiraMetadata();
                /*
                var data = Hibi.HibiPixiv.GetIllustUgoiraMetadata(Id);
                if (data != null)
                    return new UgoiraMetadata(data["ugoira_metadata"]);
                else
                    return new UgoiraMetadata();
                */
            });

        Illust() { }

        public Illust(JToken data)
        {
            IsUgoira = (string)data["type"] == "ugoira";
            Id = (int)data["id"];
            Title = (string)data["title"];
            Author = new(data["user"]);
            Caption = GetCaption((string)data["caption"]);
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
                if (Program.PixivApi != null)
                {
                    var data = Program.PixivApi.GetIllustDetailAsync(Program.PixivApiToken, id).Result?.Illust;
                    if (data == null)
                        return null;
                    List<ImageUrls> images = new();
                    if (data.PageCount > 1)
                    {
                        foreach (var image in data.MetaPages)
                        {
                            images.Add(new ImageUrls(image.ImageUrls.SquareMedium.ToString(),
                                image.ImageUrls.Medium.ToString(),
                                image.ImageUrls.Large.ToString(),
                                image.ImageUrls.Original.ToString()));
                        }
                    }
                    else
                    {
                        images.Add(new(data.ImageUrls.SquareMedium.ToString(),
                            data.ImageUrls.Medium.ToString(),
                            data.ImageUrls.Large.ToString(),
                            data.MetaSinglePage.OriginalImageUrl.ToString()));
                    }
                    var result = new Illust()
                    {
                        IsUgoira = data.Type == "ugoira",
                        Id = data.Id,
                        Title = data.Title,
                        Caption = GetCaption(data.Caption),
                        Author = new(data.User.Id, data.User.Name, data.User.Account, data.User.ProfileImageUrls.Medium.ToString()),
                        CreateDate = data.CreateDate.DateTime,
                        PageCount = data.PageCount,
                        Images = images.ToArray(),
                        Tags = data.Tags.Select(x => new Tags(x.Name, x.TranslatedName)).ToArray(),
                        TotalView = data.TotalView,
                        TotalBookmarks = data.TotalBookmarks,
                        TotalComments = data.TotalComments
                    };
                    return result;
                }
                return null;
            });

        static string GetCaption(string original)
        {
            if (!string.IsNullOrWhiteSpace(original))
            {
                var regex = new Regex(@"<[^>]+>");
                return Utils.RemoveEmptyLine(regex.Replace(Escape(original), x =>
                {
                    return x.Value switch
                    {
                        "<br />" => Environment.NewLine,
                        _ => string.Empty,
                    };
                }));
            }
            return string.Empty;
        }

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
            => url.Replace("https://i.pximg.net", "https://i.pixiv.re");
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
