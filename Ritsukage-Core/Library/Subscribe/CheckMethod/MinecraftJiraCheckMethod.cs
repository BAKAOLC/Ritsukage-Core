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
            var to = now.Date.AddHours(now.Hour);
            var from = to.AddHours(-1);
            Issue[] issues = null;
            try
            {
                issues = JiraExtension.GetMCFixedIssues(from, to);
            }
            catch (Exception e)
            {
                ConsoleLog.Error("Minecraft Jira Checker", ConsoleLog.ErrorLogBuilder(e));
                return new MinecraftJiraCheckResult();
            }
            var record = await Database.FindAsync<SubscribeStatusRecord>(x => x.Type == type && x.Target == "java");
            if (record != null && record.Status != from.ToString(DateFormat))
            {
                update = true;
                record.Status = from.ToString(DateFormat);
                await Database.UpdateAsync(record);
            }
            else if (record == null)
            {
                update = true;
                await Database.InsertAsync(new SubscribeStatusRecord()
                {
                    Type = type,
                    Target = "java",
                    Status = from.ToString(DateFormat)
                });
            }
            if (update && issues != null && issues.Length > 0)
            {
                return new MinecraftJiraCheckResult()
                {
                    Updated = true,
                    From = from,
                    To = to,
                    Data = issues
                };
            }
            else
                return new MinecraftJiraCheckResult();
        }
    }
}
