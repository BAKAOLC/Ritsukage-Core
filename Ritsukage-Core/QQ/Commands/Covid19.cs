using Ritsukage.Library.Covid19;
using Ritsukage.Tools;
using System;
using System.Linq;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Covid19")]
    public static class Covid19
    {
        static DateTime lastUpdated = default;

        [Command("新冠疫情", "covid19")]
        [CommandDescription("获取当前新冠疫情数据", "API接口来自 " + Covid19Api.ApiHost)]
        public static async void Normal(SoraMessage e)
        {
            var now = DateTime.Now;
            if ((now - lastUpdated).TotalSeconds >= 60)
            {
                if (!Covid19Api.Update())
                {
                    await e.ReplyToOriginal("数据获取失败，请稍后再试");
                    return;
                }
            }
            await e.Reply(Covid19Api.ToString());
        }

        [Command("国内新冠城市")]
        [CommandDescription("获取国内存在新冠患者的城市", "API接口来自 " + Covid19Api.ApiHost)]
        public static async void ChinaDiagnosisCity(SoraMessage e)
        {
            var now = DateTime.Now;
            if ((now - lastUpdated).TotalSeconds >= 60)
            {
                if (!Covid19Api.Update())
                {
                    await e.ReplyToOriginal("数据获取失败，请稍后再试");
                    return;
                }
            }
            var chinaData = Covid19Api.InWorld.AreaList.FirstOrDefault(x => x.Name == "中国");
            if (default(Covid19DateReport).Equals(chinaData))
            {
                await e.ReplyToOriginal("数据获取失败，请稍后再试");
                return;
            }
            var cityList = chinaData.Children.Where(x => x.ExistingDiagnosed > 0)
                .OrderBy(x => x.ExistingDiagnosed)
                .Select(x => $"{x.Name} - 当前确诊 {x.ExistingDiagnosed} 人");
            if (cityList.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine($"以下{cityList.Count()}个城市存在新冠确诊患者:");
                sb.AppendJoin(Environment.NewLine, cityList);
                sb.AppendLine().AppendLine("数据更新时间: " + Covid19Api.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.Append("数据来源: ").Append(Covid19Api.ApiHost);
                await e.Reply(sb.ToString());
            }
            else
                await e.Reply("未找到存在确诊患者的城市");

        }

        [Command("新冠严重前十国家")]
        [CommandDescription("获取新冠严重前十的国家", "API接口来自 " + Covid19Api.ApiHost)]
        public static async void DiagnosisTop10(SoraMessage e)
        {
            var now = DateTime.Now;
            if ((now - lastUpdated).TotalSeconds >= 60)
            {
                if (!Covid19Api.Update())
                {
                    await e.ReplyToOriginal("数据获取失败，请稍后再试");
                    return;
                }
            }
            var list = Covid19Api.InWorld.AreaList.Where(x => x.ExistingDiagnosed > 0)
                .OrderByDescending(x => x.ExistingDiagnosed).Take(10)
                .Select(x => $"{x.Name} - 当前确诊 {x.ExistingDiagnosed} 人，距昨日 {Utils.ToSignNumberString(x.DifferenceExistingDiagnosed)} 人");
            var sb = new StringBuilder();
            sb.AppendLine($"新冠严重程度排行前10的国家");
            sb.AppendJoin(Environment.NewLine, list);
            sb.AppendLine().AppendLine("数据更新时间: " + Covid19Api.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.Append("数据来源: ").Append(Covid19Api.ApiHost);
            await e.Reply(sb.ToString());
        }
    }
}