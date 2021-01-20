using Newtonsoft.Json.Linq;

namespace Ritsukage.Library.Roll
{
    public class ApiData
    {
        public bool Success { get; init; }
        public string Message { get; init; }
        public JToken Data { get; init; }
#pragma warning disable CS8632 // 只能在 "#nullable" 注释上下文内的代码中使用可为 null 的引用类型的注释。
        public JToken? this[object key] => Data?[key];
#pragma warning restore CS8632 // 只能在 "#nullable" 注释上下文内的代码中使用可为 null 的引用类型的注释。

        public ApiData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                Success = false;
                Message = "服务器错误，回报数据为空";
            }
            else
            {
                var _data = JToken.Parse(data);
                Success = (int)_data["code"] == 1;
                Message = (string)_data["msg"];
                Data = _data["data"];
            }
        }

        public override string ToString()
        {
            JObject data = new();
            data["code"] = Success ? 1 : 0;
            data["msg"] = Message;
            data["data"] = Data;
            return data.ToString();
        }
    }
}
