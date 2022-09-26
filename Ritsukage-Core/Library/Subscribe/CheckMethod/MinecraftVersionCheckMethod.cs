using CodeHollow.FeedReader;
using Ritsukage.Library.Data;
using Ritsukage.Library.Feed;
using Ritsukage.Library.Subscribe.CheckResult;
using Ritsukage.Tools.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Library.Subscribe.CheckMethod
{
    public class MinecraftVersionCheckMethod : Base.SubscribeCheckMethod
    {
        const string type = "minecraft version";

        readonly MinecraftVersion Feed = new MinecraftVersion();

        public override async Task<CheckResult.Base.SubscribeCheckResult> Check()
        {
            FeedItem version;
            try
            {
                var feed = await Feed.Read();
                version = feed.Items.FirstOrDefault();
            }
            catch (Exception e)
            {
                ConsoleLog.Error("Minecraft Version Checker", ConsoleLog.ErrorLogBuilder(e));
                return new MinecraftVersionCheckResult();
            }
            if (version == null)
                return new MinecraftVersionCheckResult();
            var record = await Database.FindAsync<SubscribeStatusRecord>(x => x.Type == type && x.Target == "java");
            if (record != null)
            {
                if (record.Status != version.Content)
                {
                    record.Status = version.Content;
                    await Database.UpdateAsync(record);
                    return new MinecraftVersionCheckResult()
                    {
                        Updated = true,
                        Title = version.Content,
                        Time = (DateTime)version.PublishingDate
                    };
                }
                else
                    return new MinecraftVersionCheckResult();
            }
            await Database.InsertAsync(new SubscribeStatusRecord()
            {
                Type = type,
                Target = "java",
                Status = version.Content
            });
            return new MinecraftVersionCheckResult()
            {
                Updated = true,
                Title = version.Content,
                Time = (DateTime)version.PublishingDate
            };
        }
    }
}
