using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FeedData = CodeHollow.FeedReader.Feed;
using Reader = CodeHollow.FeedReader.FeedReader;

namespace Ritsukage.Library.Feed
{
    public class FeedReader
    {
        readonly string[] Source;

        public FeedReader(params string[] source)
        {
            Source = source;
        }

        public virtual async Task<FeedData> Read()
        {
            return await Task.Run(() =>
            {
                FeedData rss = null;
                for (var i = 0; i < Source.Length; i++)
                {
                    try
                    {
                        var data = Utils.HttpGET(Source[i]);
                        if (!string.IsNullOrWhiteSpace(data))
                        {
                            rss = Reader.ReadFromString(data);
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
