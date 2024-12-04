using Newtonsoft.Json.Linq;

namespace Ritsukage.Library.Hibi
{
    public class HibiBilibili : HibiApi
    {
        public static JToken GetVideoInfo(long id)
        {
            return Get("/api/bilibili/v3/video_info", new()
            {
                { "aid", id },
            });
        }

        public static JToken GetUserInfo(int id)
        {
            return Get("/api/bilibili/v3/user_info", new()
            {
                { "uid", id },
                { "page", 1 },
                { "size", 1 },
            });
        }
    }
}