using Microsoft.Toolkit.Parsers.Rss;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ritsukage.Library.Feed
{
    public class FeedReader
    {
        readonly string[] Source;

        public FeedReader(params string[] source)
        {
            Source = source;
        }

        public virtual async Task<IEnumerable<RssSchema>> Read()
        {
            return await Task.Run(() =>
            {
                IEnumerable<RssSchema> rss = null;
                for (var i = 0; i < Source.Length; i++)
                {
                    try
                    {
                        var data = Utils.HttpGET(Source[i]);
                        if (!string.IsNullOrEmpty(data))
                        {
                            var parser = new RssParser();
                            rss = parser.Parse(data);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleLog.Error("Feed", ex.GetFormatString());
                        ConsoleLog.Error("Feed", "Target Url: ".CreateStringBuilder()
                            .AppendLine(Source[i]).Append(ConsoleLog.ErrorLogBuilder(ex, true)).ToString());
                    }
                }
                return rss;
            });
        }
    }
}
