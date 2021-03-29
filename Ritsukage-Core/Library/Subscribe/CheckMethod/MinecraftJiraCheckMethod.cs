using Ritsukage.Library.Data;
using Ritsukage.Library.Minecraft.Jila;
using Ritsukage.Library.Subscribe.CheckResult;
using Ritsukage.Tools.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Library.Subscribe.CheckMethod
{
    public class MinecraftJiraCheckMethod : Base.SubscribeCheckMethod
    {
        const string type = "minecraft jira";

        const string DateFormat = "yyyy-MM-dd HH:mm";

        public override async Task<CheckResult.Base.SubscribeCheckResult> Check()
        {
            bool update = false;
            var now = DateTime.Now;
            var from = now.Date.AddHours(now.Hour - 1);
            var to = from.AddHours(1);
            Issue[] issues = null;
            try
            {
                issues = Issue.GetIssues($"project = MC AND resolution = Fixed AND resolved > \"{from:yyyy-MM-dd HH:mm}\" AND resolved < \"{to:yyyy-MM-dd HH:mm}\" ORDER BY resolved ASC, updated DESC, created DESC");
            }
            catch (Exception e)
            {
                ConsoleLog.Error("Minecraft Jira Checker", ConsoleLog.ErrorLogBuilder(e));
                return new MinecraftJiraCheckResult();
            }
            var t = await Database.Data.Table<SubscribeStatusRecord>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                var record = t.Where(x => x.Type == type && x.Target == "java").FirstOrDefault();
                if (record != null && record.Status != from.ToString(DateFormat))
                {
                    update = true;
                    record.Status = from.ToString(DateFormat);
                    await Database.Data.UpdateAsync(record);
                }
            }
            else
            {
                update = true;
                await Database.Data.InsertAsync(new SubscribeStatusRecord()
                {
                    Type = type,
                    Target = "java",
                    Status = from.ToString(DateFormat)
                });
            }
            if (update && issues.Length > 0)
            {
                return new MinecraftJiraCheckResult()
                {
                    Updated = true,
                    Data = issues
                };
            }
            else
                return new MinecraftJiraCheckResult();
        }
    }
}
