using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ritsukage.Tools
{
    public static class BilibiliAVBVConverter
    {
        static readonly char[] CharSet = "fZodR9XQDSUm21yCkr6zBqiveYah8bt4xsWpHnJE7jL5VG3guMTKNPAwcF".ToCharArray();
        static readonly Dictionary<char, int> CharValue = new Dictionary<char, int>();
        static readonly int[] Pos = new int[] { 11, 10, 3, 8, 4, 6, 2, 9, 5, 7 };
        const long XOR = 177451812;
        const long ADD = 8728348608;

        public static string ToBV(long av)
        {
            if (av <= 0)
                throw new("AV号应为正整数");

            av ^= XOR;
            av += ADD;

            string[] result = { "B", "V", "1", "", "", "4", "", "1", "", "7", "", "" };
            for (var i = 0; i <= 5; ++i)
                result[Pos[i]] = CharSet[av / (long)Math.Pow(58, i) % 58].ToString();

            return string.Join("", result);
        }

        static readonly Regex BVCheck1 = new Regex("^[Bb][Vv]1[1-9a-km-zA-HJ-NP-Z]{2}4[1-9a-km-zA-HJ-NP-Z]1[1-9a-km-zA-HJ-NP-Z]7[1-9a-km-zA-HJ-NP-Z]{2}$");
        static readonly Regex BVCheck2 = new Regex("^1[1-9a-km-zA-HJ-NP-Z]{2}4[1-9a-km-zA-HJ-NP-Z]1[1-9a-km-zA-HJ-NP-Z]7[1-9a-km-zA-HJ-NP-Z]{2}$");

        public static long ToAV(string bv)
        {
            lock (CharValue)
            {
                if (CharValue.Count == 0)
                {
                    for (var i = 0; i < CharSet.Length; i++)
                        CharValue[CharSet[i]] = i;
                }
            }

            if (!BVCheck1.IsMatch(bv))
            {
                if (!BVCheck2.IsMatch(bv))
                    throw new("BV号格式非法，正确的BV号应是以 BV1..4.1.7.. 为格式且满足base58字符集设定的字符串");
                else
                    bv = "BV" + bv;
            }

            var chars = bv.ToCharArray();
            long av = 0;
            for (var i = 0; i <= 5; ++i)
                av += CharValue[chars[Pos[i]]] * (long)Math.Pow(58, i);
            av = (av - ADD) ^ XOR;

            if (av <= 0)
                throw new($"得出错误的转换结果({av})");

            return av;
        }
    }
}
