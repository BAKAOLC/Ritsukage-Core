using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System.Collections.Generic;

namespace Ritsukage.Library.Hibi
{
    public class HibiApi
    {
        const string Host = "https://api.obfs.dev";

        public static JToken Get(string path, Dictionary<string, object> param = null)
        {
            if (param != null && param.Count > 0)
                path += "?" + Utils.ToUrlParameter(param);
            var result = Utils.HttpGET(Host + path);
            if (string.IsNullOrWhiteSpace(result))
                return null;
            else
                return JToken.Parse(result);
        }
    }
}
