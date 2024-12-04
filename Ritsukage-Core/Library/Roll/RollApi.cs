using Ritsukage.Tools;
using System.Net;

namespace Ritsukage.Library.Roll
{
    public static class RollApi
    {
        private const string host = "https://www.mxnzp.com/api";

        private static string app_id;
        private static string app_secret;

        private static bool _init = false;

        public static void Init(string id, string secret)
        {
            if (_init) return;
            _init = true;
            app_id = id;
            app_secret = secret;
        }

        public static ApiData Get(string api)
        {
            if (!_init) return new("{\"code\":0,\"msg\":\"Api未初始化\"}");
            var request = (HttpWebRequest)WebRequest.Create(host + api);
            Utils.SetHttpHeaders(request, "pc");
            request.Headers.Add("app_id", app_id);
            request.Headers.Add("app_secret", app_secret);
            return new(Utils.HttpGET(request));
        }
    }
}