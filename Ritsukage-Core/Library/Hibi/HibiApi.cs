using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System.Collections.Generic;

namespace Ritsukage.Library.Hibi
{
    public class HibiApi
    {
        static readonly string[] Host =
        {
            "https://api.obfs.dev"
        };

        public static JToken Get(string path, Dictionary<string, object> param = null)
        {
            if (param != null && param.Count > 0)
                path += "?" + Utils.ToUrlParameter(param);
            foreach (var host in Host)
            {
                var result = Utils.HttpGET(host + path);
                if (!string.IsNullOrWhiteSpace(result))
                    return JToken.Parse(result);
            }
            return null;
        }
    }
}
