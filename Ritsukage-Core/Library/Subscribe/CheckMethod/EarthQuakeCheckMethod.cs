using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ritsukage.Library.Data;
using Ritsukage.Library.Subscribe.CheckResult;
using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ritsukage.Library.EarthQuake.EarthQuake;

namespace Ritsukage.Library.Subscribe.CheckMethod
{
    public class EarthQuakeCheckMethod : Base.SubscribeCheckMethod
    {
        const string type = "earth quake";
        const string target = "cn";

        public override async Task<CheckResult.Base.SubscribeCheckResult> Check()
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var data = GetData();
                    if (data == null || data.Count == 0)
                    {
                        return new EarthQuakeCheckResult();
                    }
                    var record = await Database.FindAsync<SubscribeStatusRecord>(x => x.Type == type && x.Target == target);
                    if (record != null)
                    {
                        EarthQuakeData statusRecord = null;
                        try
                        {
                            statusRecord = JObject.Parse(record.Status)?.ToObject<EarthQuakeData>();
                        }
                        catch (Exception e)
                        {
                            ConsoleLog.Error("Earth Quake Checker", ConsoleLog.ErrorLogBuilder(e));
                        }
                        if (statusRecord != null)
                        {
                            var index = data.FindIndex(x => x.地区 == statusRecord.地区
                            && x.预警时间 == statusRecord.预警时间
                            && x.发生时间 == statusRecord.发生时间);
                            if (index != 0)
                            {
                                var first = data.First();
                                var result = new List<EarthQuakeData>();
                                if (index > 0)
                                {
                                    for (int i = 0; i < index; i++)
                                    {
                                        result.Add(data[i]);
                                    }
                                }
                                else
                                {
                                    result.Add(first);
                                }
                                record.Status = JsonConvert.SerializeObject(first);
                                await Database.UpdateAsync(record);
                                return new EarthQuakeCheckResult()
                                {
                                    Updated = true,
                                    Data = result,
                                };
                            }
                            else
                                return new EarthQuakeCheckResult();
                        }
                    }
                    {
                        var first = data.First();
                        await Database.InsertAsync(new SubscribeStatusRecord()
                        {
                            Type = type,
                            Target = target,
                            Status = JsonConvert.SerializeObject(first),
                        });
                        return new EarthQuakeCheckResult()
                        {
                            Updated = true,
                            Data = new List<EarthQuakeData>() { first },
                        };
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLog.Error("Earth Quake Checker", ConsoleLog.ErrorLogBuilder(ex));
                    return new EarthQuakeCheckResult();
                }
            });
        }
    }
}
