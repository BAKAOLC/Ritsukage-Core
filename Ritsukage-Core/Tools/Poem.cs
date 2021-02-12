using Ritsukage.Library.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ritsukage.Tools
{
    public static class Poem
    {
        static string DatabasePath => Program.Config.PoetryDatabasePath;

        const string SearchPage = "https://sou-yun.cn/CharInClause.aspx?c=";

        static readonly Regex Sentence = new Regex(@"<li class='label'>(?<author>[^<]+)</li><li class='none'><span class='poemSentence'>(?<sentence>[^<]+)</span>&nbsp;<a target='_blank' class='small' href=""[^""]+"">(?<poem>[^<]+)</a></li>");

        public static async Task<List<string>> Search(string _char)
            => await Task.Run(async () =>
            {
                if (string.IsNullOrWhiteSpace(DatabasePath))
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
                }
                else
                {
                    var db = new SQLiteAsyncConnection(DatabasePath);
                    var tang1 = await db.Table<Poetry_Tang>().Where(x => x.Paragraphs.Contains(_char)).ToListAsync();
                    var tang2 = await db.Table<Poetry_Tang_300>().Where(x => x.Paragraphs.Contains(_char)).ToListAsync();
                    var song = await db.Table<Poetry_Song>().Where(x => x.Paragraphs.Contains(_char)).ToListAsync();
                    List<string> result = new();
                    foreach (var p in tang1)
                    {
                        var lines = p.Paragraphs.Split("|").Where(x => x.Contains(_char)).ToList();
                        foreach (var line in lines)
                            result.Add(new StringBuilder()
                            .AppendLine($"《{p.Title}》  作者：{p.Author}")
                            .Append(line)
                            .ToString());
                    }
                    foreach (var p in tang2)
                    {
                        var lines = p.Paragraphs.Split("|").Where(x => x.Contains(_char)).ToList();
                        foreach (var line in lines)
                            result.Add(new StringBuilder()
                            .AppendLine($"《{p.Title}》  作者：{p.Author}")
                            .Append(line)
                            .ToString());
                    }
                    foreach (var p in song)
                    {
                        var lines = p.Paragraphs.Split("|").Where(x => x.Contains(_char)).ToList();
                        foreach (var line in lines)
                            result.Add(new StringBuilder()
                            .AppendLine($"《{p.Title}》  作者：{p.Author}")
                            .Append(line)
                            .ToString());
                    }
                    return result;
                }
            });

        public static async Task<string> GetOrigin(string poem)
            => await Task.Run(async () =>
            {
                if (string.IsNullOrWhiteSpace(DatabasePath))
                    throw new("未装载诗词数据库，请先装载数据库再使用本功能");
                else if (poem.Length < 4)
                    throw new("搜索语句过短，请求已屏蔽，请使用不少于4字的语句进行搜索");
                else
                {
                    var db = new SQLiteAsyncConnection(DatabasePath);
                    var classic = await db.Table<Poetry_Classic>().Where(x => x.Content.Contains(poem)).ToListAsync();
                    var tang1 = await db.Table<Poetry_Tang>().Where(x => x.Paragraphs.Contains(poem)).ToListAsync();
                    var tang2 = await db.Table<Poetry_Tang_300>().Where(x => x.Paragraphs.Contains(poem)).ToListAsync();
                    var song = await db.Table<Poetry_Song>().Where(x => x.Paragraphs.Contains(poem)).ToListAsync();
                    if (classic.Count + tang1.Count + tang2.Count + song.Count > 1)
                    {
                        int count = classic.Count + tang1.Count + tang2.Count + song.Count;
                        int limit = 10;
                        List<string> result = new();
                        foreach (var p in classic)
                        {
                            if (limit > 0)
                            {
                                result.Add($"《{p.Chapter}·{p.Section}·{p.Title}》");
                                limit--;
                            }
                        }
                        foreach (var p in tang1)
                        {
                            if (limit > 0)
                            {
                                result.Add($"《{p.Title}》  作者：{p.Author}");
                                limit--;
                            }
                        }
                        foreach (var p in tang2)
                        {
                            if (limit > 0)
                            {
                                result.Add($"《{p.Title}》  作者：{p.Author}");
                                limit--;
                            }
                        }
                        foreach (var p in song)
                        {
                            if (limit > 0)
                            {
                                result.Add($"《{p.Title}》  作者：{p.Author}");
                                limit--;
                            }
                        }
                        return new StringBuilder()
                        .AppendLine($"搜索到关于条件「{poem}」可能的结果共 {count} 项：")
                        .AppendLine(string.Join(Environment.NewLine, result))
                        .Append("(超过10项时仅显示前10项)")
                        .ToString();
                    }
                    else
                    {
                        if (classic.Count > 0)
                            return new StringBuilder()
                            .AppendLine($"《{classic[0].Chapter}·{classic[0].Section}·{classic[0].Title}》")
                            .Append(classic[0].Content.Replace("|", Environment.NewLine))
                            .ToString();
                        else if (tang1.Count > 0)
                            return new StringBuilder()
                            .AppendLine($"《{tang1[0].Title}》  作者：{tang1[0].Author}")
                            .Append(tang1[0].Paragraphs.Replace("|", Environment.NewLine))
                            .ToString();
                        else if (tang2.Count > 0)
                            return new StringBuilder()
                            .AppendLine($"《{tang2[0].Title}》  作者：{tang2[0].Author}")
                            .Append(tang2[0].Paragraphs.Replace("|", Environment.NewLine))
                            .ToString();
                        else if (song.Count > 0)
                            return new StringBuilder()
                            .AppendLine($"《{song[0].Title}》  作者：{song[0].Author}")
                            .Append(song[0].Paragraphs.Replace("|", Environment.NewLine))
                            .ToString();
                        else
                            throw new("没有搜索到结果");
                    }
                }
            });
    }
}
