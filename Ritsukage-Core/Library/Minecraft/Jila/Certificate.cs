using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Ritsukage.Library.Minecraft.Jila
{
    public struct Certificate
    {
        public bool IsOK { get; init; }
        public string Cookie { get; init; }
        public DateTime Expires { get; init; }

        public static Certificate Login(string username, string password)
        {
            string content = $"os_username={Utils.UrlEncode(username)}&os_password={Utils.UrlEncode(password)}&os_cookie=true&os_destination=&user_role=&atl_token=&login=Log+In";
            HttpWebRequest request = null;
            try
            {
                request = Utils.CreateHttpWebRequest("https://bugs.mojang.com/login.jsp");
                Utils.SetHttpHeaders(request, "pc");
                request.Timeout = 20000;
                request.Referer = "https://bugs.mojang.com/login.jsp";
                request.AutomaticDecompression = DecompressionMethods.All;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = content.Length;
                byte[] byteResquest = Encoding.UTF8.GetBytes(content);
                using Stream stream = request.GetRequestStream();
                stream.Write(byteResquest, 0, byteResquest.Length);
                stream.Close();
                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var cookie = response.Headers["set-cookie"];
                var date = DateTime.Now.AddSeconds(1209600);
                response.Close();
                response.Dispose();
                request.Abort();
                ConsoleLog.Debug("Mojang Jira", new StringBuilder()
                    .AppendLine("Logined.")
                    .Append("Cookie : ")
                    .AppendLine(cookie)
                    .Append("Expires : ")
                    .Append(date.ToString("yyyy-MM-dd HH:mm:ss"))
                    .ToString());
                return new()
                {
                    IsOK = true,
                    Cookie = cookie,
                    Expires = date
                };
            }
            catch (Exception e)
            {
                request?.Abort();
                ConsoleLog.Error("Mojang Jira", new StringBuilder().AppendLine("Login failed").Append("Target Url: ")
                    .AppendLine("https://bugs.mojang.com/login.jsp").Append(ConsoleLog.ErrorLogBuilder(e, true)));
            }
            return new();
        }
    }
}
