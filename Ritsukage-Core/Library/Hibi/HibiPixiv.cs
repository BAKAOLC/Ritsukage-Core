using Newtonsoft.Json.Linq;

namespace Ritsukage.Library.Hibi
{
    public class HibiPixiv : HibiApi
    {
        public static JToken GetIllustDetail(int id)
            => Get("/api/pixiv/illust", new()
            {
                { "id", id }
            });
    }
}
