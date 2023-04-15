using Newtonsoft.Json.Linq;
using Ritsukage.Library.FFXIV.XivAPI.Attribute;
using Ritsukage.Library.FFXIV.XivAPI.Enum;
using Ritsukage.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Library.FFXIV.XivAPI
{
    public class API
    {
        ApiHost _apiHost;
        string _apiHostUrl;

        public ApiHost ApiHost
        {
            get => _apiHost;
            set
            {
                _apiHost = value;
                _apiHostUrl = GetApiHostUrl(value);
            }
        }
        public string ApiHostUrl => _apiHostUrl;

        JToken Get(string path, Dictionary<string, object> param = null)
        {
            if (param != null && param.Count > 0)
                path += "?" + Utils.ToUrlParameter(param);
            var result = Utils.HttpGET(ApiHostUrl + path);
            if (!string.IsNullOrWhiteSpace(result))
                return JToken.Parse(result);
            return null;
        }

        static readonly Type ApiHostUrlAttribute = typeof(ApiHostUrlAttribute);
        static string GetApiHostUrl(System.Enum @enum)
        {
            FieldInfo field = @enum.GetType().GetField(@enum.ToString());
            if (field.IsDefined(ApiHostUrlAttribute))
                return field.GetCustomAttribute<ApiHostUrlAttribute>().Url;
            return GetApiHostUrl(default(ApiHost));
        }
    }
}
