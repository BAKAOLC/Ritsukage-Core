using Ritsukage.Library.FFXIV;
using Ritsukage.Tools;
using System;

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
    }
}
