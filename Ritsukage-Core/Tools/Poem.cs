using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ritsukage.Tools
{
    public static class Poem
    {
        const string SearchPage = "https://sou-yun.cn/CharInClause.aspx?c=";

        static readonly Regex Sentence = new Regex(@"<li class='label'>(?<author>[^<]+)</li><li class='none'><span class='poemSentence'>(?<sentence>[^<]+)</span>&nbsp;<a target='_blank' class='small' href=""[^""]+"">(?<poem>[^<]+)</a></li>");

        public static async Task<List<string>> Search(string _char)
        {
            return await Task.Run(() =>
            {
                var html = Utils.HttpGET(SearchPage + _char);
                var matches = Sentence.Matches(html);
                var result = new List<string>();
                foreach (Match match in matches)
                {
                    result.Add(new StringBuilder()
                    .AppendLine($"《{match.Groups["poem"].Value}》  作者：{match.Groups["author"].Value}")
                    .Append(match.Groups["sentence"].Value)
                    .ToString());
                }
                return result;
            });
        }
    }
}
