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
        public static async void ET(SoraMessage e)
        {
            long unix = DateTimeOffset.FromUnixTimeSeconds(Utils.GetNetworkTimeStamp()).ToUniversalTime().ToUnixTimeSeconds();
            await e.Reply($"当前为艾欧泽亚时间：ET {GetEorzeaHour(unix),2:D2}:{GetEorzeaMinute(unix),2:D2}");
        }

        [Command("暴击")]
        public static async void CalcCriticalHit(SoraMessage e, int value)
            => await e.Reply(StatusCalculator.CriticalHit(value).ToString());

        [Command("根据暴击率求值")]
        public static async void CalcCriticalHitFromRate(SoraMessage e, double value)
            => await e.Reply(StatusCalculator.CriticalHitResult.GetFromRate(value).ToString());

        [Command("根据暴击伤害求值")]
        public static async void CalcCriticalHitFromBonus(SoraMessage e, double value)
            => await e.Reply(StatusCalculator.CriticalHitResult.GetFromBonus(value).ToString());

        [Command("直击")]
        public static async void CalcDirectHit(SoraMessage e, int value)
            => await e.Reply(StatusCalculator.DirectHit(value).ToString());

        [Command("根据直击率求值")]
        public static async void CalcDirectHitFromRate(SoraMessage e, double value)
            => await e.Reply(StatusCalculator.DirectHitResult.GetFromRate(value).ToString());

        [Command("信念")]
        public static async void CalcDetermination(SoraMessage e, int value)
            => await e.Reply(StatusCalculator.Determination(value).ToString());

        [Command("坚韧")]
        public static async void CalcTenacity(SoraMessage e, int value)
            => await e.Reply(StatusCalculator.Tenacity(value).ToString());

        [Command("根据受击伤害求值")]
        public static async void CalcTenacityFromRate(SoraMessage e, double value)
            => await e.Reply(StatusCalculator.TenacityResult.GetFromDamageRate(value).ToString());

        [Command("技速", "技能速度", "咏速", "咏唱速度", "速度")]
        public static async void CalcSpeed(SoraMessage e, int value)
            => await e.Reply(StatusCalculator.Speed(value).ToString());

        [Command("根据GCD求值")]
        public static async void CalcSpeedFromGCD(SoraMessage e, double value)
            => await e.Reply(StatusCalculator.SpeedResult.GetFromGCD25(value).ToString());
    }
}
