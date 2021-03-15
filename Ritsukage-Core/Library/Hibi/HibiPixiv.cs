using Newtonsoft.Json.Linq;
using System;

namespace Ritsukage.Library.Hibi
{
    public class HibiPixiv : HibiApi
    {
        #region Enum Value
        public enum IllustType
        {
            Illust,
            Manga
        }

        public enum RankingType
        {
            Day,
            Week,
            Month,
            Day_Male,
            Day_Female,
            Week_Original,
            Week_Rookie,
            Day_R18,
            Day_Male_R18,
            Day_Female_R18,
            Week_r18,
            Week_r18g
        }

        public enum SearchModeType
        {
            Partial_Match_For_Tags,
            Exact_Match_For_Tags,
            Title_And_Caption
        }

        public enum SearchSortType
        {
            Date_Desc,
            Date_Asc
        }

        public enum SearchDuration
        {
            Within_Last_Day,
            Within_Last_Week,
            Within_Last_Month
        }
        #endregion

        public static JToken GetIllustDetail(int id)
            => Get("/api/pixiv/illust", new()
            {
                { "id", id }
            });

        public static JToken GetMemberDetail(int id)
            => Get("/api/pixiv/member", new()
            {
                { "id", id }
            });

        public static JToken GetMemberIllust(int id, IllustType type = IllustType.Illust, int page = 1, int size = 20)
            => Get("/api/pixiv/member_illust", new()
            {
                { "id", id },
                { "illust_type", type.ToString("G").ToLower() },
                { "page", page },
                { "size", size }
            });

        public static JToken GetRank(RankingType type = RankingType.Week, int page = 1, int size = 20)
            => Get("/api/pixiv/rank", new()
            {
                { "mode", type.ToString("G").ToLower() },
                { "page", page },
                { "size", size }
            });

        public static JToken GetRank(DateTime date, RankingType type = RankingType.Week, int page = 1, int size = 20)
            => Get("/api/pixiv/rank", new()
            {
                { "mode", type.ToString("G").ToLower() },
                { "date", date.Date.ToString("yyyy-MM-dd") },
                { "page", page },
                { "size", size }
            });

        public static JToken Search(string word,
            SearchModeType type = SearchModeType.Partial_Match_For_Tags,
            SearchSortType order = SearchSortType.Date_Desc, int page = 1, int size = 20)
            => Get("/api/pixiv/search", new()
            {
                { "word", word },
                { "mode", type.ToString("G").ToLower() },
                { "order", order.ToString("G").ToLower() },
                { "page", page },
                { "size", size }
            });

        public static JToken Search(string word, SearchDuration duration,
            SearchModeType type = SearchModeType.Partial_Match_For_Tags,
            SearchSortType order = SearchSortType.Date_Desc, int page = 1, int size = 20)
            => Get("/api/pixiv/search", new()
            {
                { "word", word },
                { "mode", type.ToString("G").ToLower() },
                { "order", order.ToString("G").ToLower() },
                { "duration", duration.ToString("G").ToLower() },
                { "page", page },
                { "size", size }
            });

        public static JToken GetHotTags()
            => Get("/api/pixiv/tags");

        public static JToken GetIllustRelated(int id, int page = 1, int size = 20)
            => Get("/api/pixiv/related", new()
            {
                { "id", id },
                { "page", page },
                { "size", size }
            });

        public static JToken GetIllustUgoiraMetadata(int id)
            => Get("/api/pixiv/ugoira_metadata", new()
            {
                { "id", id }
            });
    }
}
