using Microsoft.Toolkit.Parsers.Rss;
using Ritsukage.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ritsukage.Library.Feed
{
    public class FeedReader
    {
        readonly string Source;

        public FeedReader(string source)
        {
            Source = source;
        }

        public async Task<IEnumerable<RssSchema>> Read()
        {
            return await Task.Run(() =>
            {
                var data = Utils.HttpGET(Source);
                var parser = new RssParser();
                return parser.Parse(data);
            });
        }
    }
}
