using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ritsukage.Tools
{
    public static partial class BilibiliAVBVConverter
    {
        private static readonly char[] CharSet =
            "FcwAPNKTMug3GV5Lj7EJnHpWsx4tb8haYeviqBz6rkCy12mUSDQX9RdoZf".ToCharArray();

        private static readonly Dictionary<char, int> CharValue = [];
        private const ulong XOR = 23442827791579UL;
        private const ulong MASK = 2251799813685247UL;
        private const ulong AID = 1UL << 51;
        private const ulong BASE = 58UL;

        public static string ToBV(ulong av)
        {
            if (av <= 0)
                throw new("AV号应为正整数");


            char[] result = ['B', 'V', '1', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '];
            var idx = result.Length - 1;
            var tmp = (AID | av) ^ XOR;

            while (tmp > 0)
            {
                result[idx] = CharSet[(int)(tmp % BASE)];
                tmp /= BASE;
                idx--;
            }

            (result[3], result[9]) = (result[9], result[3]);
            (result[4], result[7]) = (result[7], result[4]);

            return string.Join("", result);
        }

        public static ulong ToAV(string bv)
        {
            lock (CharValue)
            {
                if (CharValue.Count == 0)
                    for (var i = 0; i < CharSet.Length; i++)
                        CharValue[CharSet[i]] = i;
            }

            if (!GetBVCheckRegex1().IsMatch(bv))
            {
                if (!GetBVCheckRegex2().IsMatch(bv))
                    throw new("BV号格式非法，正确的BV号应是以 BV1xxxxxxxxx 为格式且满足base58字符集设定的字符串");
                bv = "BV" + bv;
            }

            var chars = bv.ToCharArray();

            (chars[3], chars[9]) = (chars[9], chars[3]);
            (chars[4], chars[7]) = (chars[7], chars[4]);

            chars = chars[3..];

            var av = chars.Aggregate(0UL, (current, c) => current * BASE + (ulong)CharValue[c]);

            av = (av & MASK) ^ XOR;

            return av;
        }

        [GeneratedRegex(
            "^[Bb][Vv]1[1-9a-km-zA-HJ-NP-Z]{9}$")]
        private static partial Regex GetBVCheckRegex1();

        [GeneratedRegex("^1[1-9a-km-zA-HJ-NP-Z]{9}$")]
        private static partial Regex GetBVCheckRegex2();
    }
}