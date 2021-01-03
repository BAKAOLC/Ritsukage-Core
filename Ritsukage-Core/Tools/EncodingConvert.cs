using System.IO;
using System.Text;

namespace Ritsukage.Tools
{
    public static class EncodingConvert
    {
        public static string Convert(Encoding srcEncoding, Encoding dstEncoding, string text)
            => dstEncoding.GetString(Encoding.Convert(srcEncoding, dstEncoding, srcEncoding.GetBytes(text)));

        public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes)
            => Encoding.Convert(srcEncoding, dstEncoding, bytes);

        public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, Stream stream)
        {
            var bs = new byte[stream.Length];
            var pos = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bs, 0, bs.Length);
            stream.Position = pos;
            return Encoding.Convert(srcEncoding, dstEncoding, bs);
        }

        public static string Convert(string srcEncoding, string dstEncoding, string text)
        {
            var src = Encoding.GetEncoding(srcEncoding);
            var dst = Encoding.GetEncoding(dstEncoding);
            return dst.GetString(Encoding.Convert(src, dst, src.GetBytes(text)));
        }

        public static byte[] Convert(string srcEncoding, string dstEncoding, byte[] bytes)
        {
            var src = Encoding.GetEncoding(srcEncoding);
            var dst = Encoding.GetEncoding(dstEncoding);
            return Encoding.Convert(src, dst, bytes);
        }

        public static byte[] Convert(string srcEncoding, string dstEncoding, Stream stream)
        {
            var src = Encoding.GetEncoding(srcEncoding);
            var dst = Encoding.GetEncoding(dstEncoding);
            var bs = new byte[stream.Length];
            var pos = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bs, 0, bs.Length);
            stream.Position = pos;
            return Encoding.Convert(src, dst, bs);
        }

        public static string UTF8ToGB18030(string text)
            => Convert(Encoding.UTF8, Encoding.GetEncoding("gb18030"), text);

        public static byte[] UTF8ToGB18030(byte[] bytes)
            => Convert(Encoding.UTF8, Encoding.GetEncoding("gb18030"), bytes);

        public static byte[] UTF8ToGB18030(Stream stream)
            => Convert(Encoding.UTF8, Encoding.GetEncoding("gb18030"), stream);

        public static string UTF8ToGB2312(string text)
            => Convert(Encoding.UTF8, Encoding.GetEncoding("gb2312"), text);

        public static byte[] UTF8ToGB2312(byte[] bytes)
            => Convert(Encoding.UTF8, Encoding.GetEncoding("gb2312"), bytes);

        public static byte[] UTF8ToGB2312(Stream stream)
            => Convert(Encoding.UTF8, Encoding.GetEncoding("gb2312"), stream);

        public static string GB18030ToUTF8(string text)
            => Convert(Encoding.GetEncoding("gb18030"), Encoding.UTF8, text);

        public static byte[] GB18030ToUTF8(byte[] bytes)
            => Convert(Encoding.GetEncoding("gb18030"), Encoding.UTF8, bytes);

        public static byte[] GB18030ToUTF8(Stream stream)
            => Convert(Encoding.GetEncoding("gb18030"), Encoding.UTF8, stream);

        public static string GB2312ToUTF8(string text)
            => Convert(Encoding.GetEncoding("gb2312"), Encoding.UTF8, text);

        public static byte[] GB2312ToUTF8(byte[] bytes)
            => Convert(Encoding.GetEncoding("gb2312"), Encoding.UTF8, bytes);

        public static byte[] GB2312ToUTF8(Stream stream)
            => Convert(Encoding.GetEncoding("gb2312"), Encoding.UTF8, stream);
    }
}
