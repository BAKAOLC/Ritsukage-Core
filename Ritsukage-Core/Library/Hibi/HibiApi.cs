using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System.Collections.Generic;

namespace Ritsukage.Library.Hibi
{
    public class HibiApi
    {
        const string Host = "https://hibiapi.herokuapp.com";

        public static JToken Get(string path, Dictionary<string, object> param = null)
        {
            if (param != null)
            {
                var sb = new List<string>();
                foreach (var p in param)
                    sb.Add($"{Utils.UrlEncode(p.Key)}={Utils.UrlEncode(p.Value.ToString())}");
                path += "?" + string.Join("&", sb);
            }
            var result = Utils.HttpGET(Host + path);
            if (string.IsNullOrWhiteSpace(result))
                return null;
            else
                return JToken.Parse(result);
        }
    }
}
