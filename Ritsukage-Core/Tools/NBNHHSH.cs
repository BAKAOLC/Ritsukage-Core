using Newtonsoft.Json.Linq;
using Ritsukage.Tools.Console;

namespace Ritsukage.Tools
{
    public static class NBNHHSH
    {
        const string API = "https://lab.magiconch.com/api/nbnhhsh/guess";

        public static string[] Get(string origin)
        {
            var result = Utils.HttpPOST(API, new JObject
            {
                ["text"] = origin
            }.ToString(), 20000, null, null, null, "application/json");
            var data = JArray.Parse(result)[0];
            var trans = (JArray)data["trans"];
            var s = new string[trans.Count];
            for (var i = 0; i < trans.Count; i++)
                s[i] = (string)trans[i];
            return s;
        }
    }
}
