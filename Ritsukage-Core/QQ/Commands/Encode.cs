using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class Encode
    {
        [Command("base64encode")]
        [CommandDescription("将字符串转换为base64编码的文本")]
        public static async void Base64Encode(SoraMessage e, string text)
        {
            var result = Convert.ToBase64String(Encoding.UTF8.GetBytes(e.Message.GetText()[14..]));
            await e.ReplyToOriginal("[Base64 Encode]" + Environment.NewLine + result);
        }

        [Command("base64decode")]
        [CommandDescription("将base64编码的文本转换为字符串")]
        public static async void Base64Decode(SoraMessage e, string text)
        {
            var result = Encoding.UTF8.GetString(Convert.FromBase64String(e.Message.GetText()[14..]));
            await e.ReplyToOriginal("[Base64 Decode]" + Environment.NewLine + result);
        }

        [Command("md5")]
        [CommandDescription("将字符串编码为md5")]
        public static async void MD5Convert(SoraMessage e, string text)
        {
            MD5 md5 = MD5.Create();
            byte[] byteOld = Encoding.UTF8.GetBytes(e.Message.GetText()[4..]);
            byte[] byteNew = md5.ComputeHash(byteOld);
            await e.ReplyToOriginal("[MD5]" + string.Join(string.Empty, byteNew.Select(x => x.ToString("X2"))));
        }
    }
}