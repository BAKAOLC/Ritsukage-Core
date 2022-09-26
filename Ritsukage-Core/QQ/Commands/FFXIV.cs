using Ritsukage.Library.FFXIV;
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
        static long GetEorzeaHour(long unix) => unix / 175 % 24;
        static long GetEorzeaMinute(long unix) => Convert.ToInt64(60 * ((double)unix / 175 % 1));

        [Command("艾欧泽亚时间", "et")]
        [CommandDescription("获取当前的艾欧泽亚时间", "1ET分钟=175/60秒")]
        public static async void ET(SoraMessage e)
        {
            long unix = DateTimeOffset.FromUnixTimeSeconds(Utils.GetNetworkTimeStamp()).ToUniversalTime().ToUnixTimeSeconds();
            await e.Reply($"当前为艾欧泽亚时间：ET {GetEorzeaHour(unix),2:D2}:{GetEorzeaMinute(unix),2:D2}");
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
    }
}
