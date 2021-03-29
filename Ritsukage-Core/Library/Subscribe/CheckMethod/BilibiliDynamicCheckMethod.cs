using Ritsukage.Library.Bilibili.Model;
using Ritsukage.Library.Data;
using Ritsukage.Library.Subscribe.CheckResult;
using Ritsukage.Tools.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Library.Subscribe.CheckMethod
{
    public class BilibiliDynamicCheckMethod : Base.SubscribeCheckMethod
    {
        const string type = "bilibili dynamic";

        public int UserId { get; init; }

        public BilibiliDynamicCheckMethod(int userid)
        {
            UserId = userid;
        }

        public override async Task<CheckResult.Base.SubscribeCheckResult> Check()
        {
            User user = null;
            Dynamic[] dynamics = null;
            try
            {
                user = User.Get(UserId);
                dynamics = user?.GetDynamicList();
            }
            catch (Exception e)
            {
                ConsoleLog.Error("Bilibili Live Checker", ConsoleLog.ErrorLogBuilder(e));
                return new BilibiliDynamicCheckResult();
            }
            var record = await Database.FindAsync<SubscribeStatusRecord>(x => x.Type == type && x.Target == UserId.ToString());
            if (record != null)
            {
                if (ulong.TryParse(record.Status, out var recordId))
                {
                    if (dynamics[0].Id > recordId)
                    {
                        record.Status = dynamics[0].Id.ToString();
                        await Database.UpdateAsync(record);
                        return new BilibiliDynamicCheckResult()
                        {
                            Updated = true,
                            User = user,
                            Dynamics = dynamics.TakeWhile(x => x.Id > recordId).ToArray()
                        };
                    }
                    return new BilibiliDynamicCheckResult();
                }
            }
            Dynamic dy = dynamics[0];
            await Database.InsertAsync(new SubscribeStatusRecord()
            {
                Type = type,
                Target = UserId.ToString(),
                Status = dy.Id.ToString()
            });
            return new BilibiliDynamicCheckResult()
            {
                Updated = true,
                User = user,
                Dynamics = new[] { dy }
            };
        }
    }
}
