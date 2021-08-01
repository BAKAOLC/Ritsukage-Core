using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Ritsukage.Tools
{
    public static class BaiduTranslate
    {
        public static readonly List<Language> LanguageTable = new()
        {
            new("auto", "自动检测"),
            new("zh", "中文"),
            new("en", "英语"),
            new("yue", "粤语"),
            new("wyw", "文言文"),
            new("jp", "日语"),
            new("kor", "韩语"),
            new("fra", "法语"),
            new("spa", "西班牙语"),
            new("th", "泰语"),
            new("ara", "阿拉伯语"),
            new("ru", "俄语"),
            new("pt", "葡萄牙语"),
            new("de", "德语"),
            new("it", "意大利语"),
            new("el", "希腊语"),
            new("nl", "荷兰语"),
            new("pl", "波兰语"),
            new("bul", "保加利亚语"),
            new("est", "爱沙尼亚语"),
            new("dan", "丹麦语"),
            new("fin", "芬兰语"),
            new("cs", "捷克语"),
            new("rom", "罗马尼亚语"),
            new("slo", "斯洛文尼亚语"),
            new("swe", "瑞典语"),
            new("hu", "匈牙利语"),
            new("cht", "繁体中文"),
            new("vie", "越南语"),
        };

        public static readonly List<ResultCode> ResultCodeTable = new()
        {
            new("52000", true, "成功"),
            new("52001", false, "请求超时", "重试"),
            new("52002", false, "系统错误", "重试"),
            new("52003", false, "未授权用户", "检查您的 appid 是否正确，或者服务是否开通"),
            new("54000", false, "必填参数为空", "检查是否少传参数"),
            new("54001", false, "签名错误", "请检查您的签名生成方法"),
            new("54003", false, "访问频率受限", "请降低您的调用频率"),
            new("54004", false, "账户余额不足", "请前往管理控制台为账户充值"),
            new("54005", false, "长query请求频繁", "请降低长query的发送频率，3s后再试"),
            new("58000", false, "客户端IP非法", "检查个人资料里填写的 IP地址 是否正确\n可前往管理控制平台修改IP限制，IP可留空"),
            new("58001", false, "译文语言方向不支持", "检查译文语言是否在语言列表里"),
            new("58002", false, "服务当前已关闭", "请前往管理控制台开启服务"),
            new("90107", false, "认证未通过或未生效", "请前往我的认证查看认证进度"),
        };

        public struct Language
        {
            public string Id { get; init; }
            public string Name { get; init; }

            public Language(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public static Language GetById(string id)
                => LanguageTable.Find(x => x.Id == id);

            public override string ToString() => Name;
        }

        public struct ResultCode
        {
            public string Id { get; init; }
            public bool IsSuccess { get; init; }
            public string Result { get; init; }
            public string TipMessage { get; init; }

            public ResultCode(string id, bool isSuccess, string result, string tip = "")
            {
                Id = id;
                IsSuccess = isSuccess;
                Result = result;
                TipMessage = tip;
            }

            public static ResultCode GetById(string id)
                => ResultCodeTable.Find(x => x.Id == id);

            public static ResultCode GetSuccessCode()
                => ResultCodeTable.Find(x => x.IsSuccess);
        }

        public struct TranslateResult
        {
            public ResultCode Code { get; init; }
            public bool Success => Code.IsSuccess;
            public string Info => Code.Result;
            public string TipMessage => Code.TipMessage;
            public Language? OriginalLanguage { get; init; }
            public string OriginalString { get; init; }
            public Language? TranslateLanguage { get; init; }
            public string TranslateString { get; init; }

            public TranslateResult(JToken data)
            {
                if (data["error_code"] != null)
                {
                    Code = ResultCode.GetById((string)data["error_code"]);
                    OriginalLanguage = null;
                    OriginalString = null;
                    TranslateLanguage = null;
                    TranslateString = null;
                }
                else
                {
                    Code = ResultCode.GetSuccessCode();
                    OriginalLanguage = Language.GetById((string)data["from"]);
                    OriginalString = (string)data["trans_result"][0]["src"];
                    TranslateLanguage = Language.GetById((string)data["to"]);
                    TranslateString = (string)data["trans_result"][0]["dst"];
                }
            }
        }

        public static string Translate(string appId, string secretKey, string salt,
            string str, string from = "auto", string to = "zh")
        {
            string sign = EncryptString(appId + str + salt + secretKey);
            string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            url += "q=" + HttpUtility.UrlEncode(str);
            url += "&from=" + from;
            url += "&to=" + to;
            url += "&appid=" + appId;
            url += "&salt=" + salt;
            url += "&sign=" + sign;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }
        public static string Translate(string str, string from = "auto", string to = "zh")
            => Translate(Program.Config.BaiduTranslateAppId,
                Program.Config.BaiduTranslateKey,
                new Rand().Int(100000000, 999999999).ToString(),
                str, from, to);

        public static TranslateResult GetTranslateResult(string appId, string secretKey, string salt,
            string str, string from = "auto", string to = "zh")
            => new(JObject.Parse(Translate(appId, secretKey, salt, str, from, to)));
        public static TranslateResult GetTranslateResult(string str, string from = "auto", string to = "zh")
            => new(JObject.Parse(Translate(str, from, to)));

        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            byte[] byteNew = md5.ComputeHash(byteOld);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
