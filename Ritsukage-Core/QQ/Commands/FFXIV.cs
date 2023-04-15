using Ritsukage.Library.FFXIV;
using Ritsukage.Library.FFXIV.Data;
using Ritsukage.Library.FFXIV.Struct;
using Ritsukage.Library.FFXIV.WanaHome;
using Ritsukage.Library.FFXIV.WanaHome.Enum;
using Ritsukage.Tools;
using System;
using System.Linq;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("FFXIV")]
    public static class FFXIV
    {

        [Command("艾欧泽亚时间", "et")]
        [CommandDescription("获取当前的艾欧泽亚时间", "1ET分钟=175/60秒")]
        public static async void ET(SoraMessage e)
        {
            var now = EorzeaTime.Now;
            await e.Reply($"当前为艾欧泽亚时间：ET {now.Hour,2:D2}:{now.Minute,2:D2}");
        }

        [Command("暴击")]
        [CommandDescription("求指定暴击属性的信息")]
        [ParameterDescription(1, "属性值")]
        public static async void CalcCriticalHit(SoraMessage e, int value)
            => await e.Reply(StatusCalculator.CriticalHit(value).ToString());

        [Command("根据暴击率求值")]
        [CommandDescription("根据指定暴击率求对应的暴击属性应该是多少")]
        [ParameterDescription(1, "暴击率", "不带百分号的百分比数值")]
        public static async void CalcCriticalHitFromRate(SoraMessage e, double value)
            => await e.Reply(StatusCalculator.CriticalHitResult.GetFromRate(value).ToString());

        [Command("根据暴击伤害求值")]
        [CommandDescription("根据指定暴击伤害倍率求对应的暴击属性应该是多少")]
        [ParameterDescription(1, "伤害倍率")]
        public static async void CalcCriticalHitFromBonus(SoraMessage e, double value)
            => await e.Reply(StatusCalculator.CriticalHitResult.GetFromBonus(value).ToString());

        [Command("直击")]
        [CommandDescription("求指定直击属性的信息")]
        [ParameterDescription(1, "属性值")]
        public static async void CalcDirectHit(SoraMessage e, int value)
            => await e.Reply(StatusCalculator.DirectHit(value).ToString());

        [Command("根据直击率求值")]
        [CommandDescription("根据指定直击率求对应的直击属性应该是多少")]
        [ParameterDescription(1, "直击率", "不带百分号的百分比数值")]
        public static async void CalcDirectHitFromRate(SoraMessage e, double value)
            => await e.Reply(StatusCalculator.DirectHitResult.GetFromRate(value).ToString());

        [Command("信念")]
        [CommandDescription("求指定信念属性的信息")]
        [ParameterDescription(1, "属性值")]
        public static async void CalcDetermination(SoraMessage e, int value)
            => await e.Reply(StatusCalculator.Determination(value).ToString());

        [Command("坚韧")]
        [CommandDescription("求指定坚韧属性的信息")]
        [ParameterDescription(1, "属性值")]
        public static async void CalcTenacity(SoraMessage e, int value)
            => await e.Reply(StatusCalculator.Tenacity(value).ToString());

        [Command("根据受击伤害求值")]
        [CommandDescription("根据指定受伤比率求对应的坚韧属性应该是多少")]
        [ParameterDescription(1, "受伤比率", "不带百分号的百分比数值")]
        public static async void CalcTenacityFromRate(SoraMessage e, double value)
            => await e.Reply(StatusCalculator.TenacityResult.GetFromDamageRate(value).ToString());

        [Command("技速", "技能速度", "咏速", "咏唱速度", "速度")]
        [CommandDescription("求指定技能速度/咏唱速度属性的信息")]
        [ParameterDescription(1, "属性值")]
        public static async void CalcSpeed(SoraMessage e, int value)
            => await e.Reply(StatusCalculator.Speed(value).ToString());

        [Command("根据GCD求值")]
        [CommandDescription("根据指定GCD长度求对应的技能速度/咏唱速度属性应该是多少")]
        [ParameterDescription(1, "2.5sGCD长度")]
        public static async void CalcSpeedFromGCD(SoraMessage e, double value)
            => await e.Reply(StatusCalculator.SpeedResult.GetFromGCD25(value).ToString());

        [Command("检查房区")]
        [CommandDescription("获取指定房区的当前状态")]
        [ParameterDescription(1, "服务器名称")]
        public static async void CheckHouseList(SoraMessage e, string server_name)
        {
            Server server = Server.Unknown;
            server = WanaHomeApi.MatchServer(server_name);
            if (server == Server.Unknown)
            {
                await e.ReplyToOriginal("#未能识别服务器名称：" + server_name);
                return;
            }
            var result = WanaHomeApi.GetTerritoryState(server);
            var sb = new StringBuilder();
            sb.AppendLine($"### {server} ###");
            if (result.OnSale.Any())
            {
                sb.Append("> 空闲房屋：");
                foreach (var house in result.OnSale)
                {
                    sb.AppendLine();
                    sb.Append($"[{house.HouseName}] 房型：{house.Size} 价格：{house.Price}Gil 空闲时长：{house.SellTimeSpan}");
                }
            }
            else
            {
                sb.Append("> 当前没有空闲房屋");
            }
            sb.AppendLine();
            sb.Append("> 历史记录（仅显示最新的10条动态）");
            foreach (var change in result.Changes.Take(10))
            {
                sb.AppendLine();
                sb.Append(change.ToString());
            }

            sb.AppendLine();
            sb.Append($"数据更新时间：{result.LastUpdate.ToLocalTime():yyyy年MM月dd日 HH:mm:ss}");

            await e.ReplyToOriginal(sb.ToString());
        }

        [Command("获取天气", "查询天气")]
        [CommandDescription("查询指定区域的当前天气")]
        [ParameterDescription(1, "区域名称")]
        public static async void GetWeather(SoraMessage e, string zone)
        {
            var search = Zone.SearchZoneID(zone);
            if (search.Length == 0)
            {
                await e.ReplyToOriginal($"[FFXIV] 未找到任何有关于 {zone} 的区域");
                return;
            }
            var et = EorzeaTime.Now;
            var next = ZoneWeather.SyncToEorzeaWeather(et, 1);
            var nextTS = next - et;
            var sb = new StringBuilder("[FFXIV]");
            for (int i = 0; i < search.Length; i++)
            {
                var zoneWeather = new ZoneWeather(search[i].Key);
                var currentWeather = zoneWeather.GetWeather(et, 0);
                var nextWeather = zoneWeather.GetWeather(et, 1);
                sb.AppendLine().Append($">> {search[i].Value}");
                sb.AppendLine().Append($"当前天气    {Weather.GetWeatherName(currentWeather.WeatherID)}");
                sb.AppendLine().Append($"下一天气    {Weather.GetWeatherName(nextWeather.WeatherID)}");
            }
            sb.AppendLine().Append($"天气将于{next.DateTime:yyyy-MM-dd HH:mm:ss}切换");
            if (next.TotalSeconds > 60)
                sb.AppendLine().Append($"距离切换时间还有{System.Math.Floor(nextTS.TotalMinutes)}分{nextTS.Seconds}秒");
            else
                sb.AppendLine().Append($"距离切换时间还有{nextTS.Seconds}秒");
            await e.ReplyToOriginal(sb.ToString());
        }

        [Command("获取天气列表", "查询天气列表")]
        [CommandDescription("查询指定区域的天气列表")]
        [ParameterDescription(1, "区域名称")]
        public static async void GetWeatherList(SoraMessage e, string zone)
        {
            var search = Zone.SearchZoneID(zone);
            if (search.Length == 0)
            {
                await e.ReplyToOriginal($"[FFXIV] 未找到任何有关于 {zone} 的区域");
                return;
            }
            var et = EorzeaTime.Now;
            var sb = new StringBuilder("[FFXIV]");
            for (int i = 0; i < search.Length; i++)
            {
                var zoneWeather = new ZoneWeather(search[i].Key);
                var list = zoneWeather.GetWeatherList(et, -1, 10);
                sb.AppendLine().Append($">> {search[i].Value}");
                for (int j = 0; j < list.Length; j++)
                    sb.AppendLine().Append($"{list[j].BeginTime.DateTime:HH:mm:ss}    {Weather.GetWeatherName(list[j].WeatherID)}");
            }
            await e.ReplyToOriginal(sb.ToString());
        }

        [Command("获取下一次天气", "查询下一次天气")]
        [CommandDescription("查询下一次天气在什么时候")]
        [ParameterDescription(1, "区域名称")]
        [ParameterDescription(2, "天气名称")]
        public static async void GetWeatherList(SoraMessage e, string zone, string weather)
        {
            var search = Zone.SearchZoneID(zone);
            if (search.Length == 0)
            {
                await e.ReplyToOriginal($"[FFXIV] 未找到任何有关于 {zone} 的区域");
                return;
            }
            var et = EorzeaTime.Now;
            var sb = new StringBuilder("[FFXIV]");
            for (int i = 0; i < search.Length; i++)
            {
                sb.AppendLine().Append($">> {search[i].Value}");
                var weatherRateListID = ZoneWeatherIndex.GetZoneWeatherIndex(search[i].Key);
                var weatherList = WeatherRate.GetWeatherRateList(weatherRateListID);
                var weatherID = weatherList.GetWeathers()?.FirstOrDefault(x => Weather.GetWeatherName(x) == weather, 0) ?? 0;
                if (weatherID == 0)
                {
                    sb.AppendLine().Append($"本区域天气列表中不存在 {weather}");
                }
                else
                {
                    var zoneWeather = new ZoneWeather(search[i].Key);
                    var currentWeather = zoneWeather.GetWeather(et);
                    sb.AppendLine().Append($"当前天气：{Weather.GetWeatherName(currentWeather.WeatherID)}");
                    (var found, var resultWeather) = zoneWeather.FindWeather(et, weatherID, currentWeather.WeatherID == weatherID ? 1 : 0);
                    if (found)
                    {
                        var ts = resultWeather.BeginTime - et;
                        sb.AppendLine()
                            .Append($"下一次 {Weather.GetWeatherName(resultWeather.WeatherID)} 将出现于")
                            .AppendLine()
                            .Append(resultWeather.BeginTime.DateTime.ToString("yyyy-MM-dd HH:mm:ss"))
                            .AppendLine()
                            .Append(ts.TotalSeconds > 60 ? $"({System.Math.Floor(ts.TotalMinutes)}分{ts.Seconds}秒后)" : $"({ts.Seconds}秒后)");
                    }
                    else
                        sb.AppendLine().Append($"无法找到下一次的 {Weather.GetWeatherName(resultWeather.WeatherID)} 天气出现时间");
                }
            }
            await e.ReplyToOriginal(sb.ToString());
        }
    }
}
