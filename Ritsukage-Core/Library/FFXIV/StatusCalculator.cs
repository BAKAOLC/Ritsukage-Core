using System;
using System.Text;

namespace Ritsukage.Library.FFXIV
{
    public static class StatusCalculator
    {
        const int nlv = 1900;

        static readonly string Title = $"版本 6.0  Lv90  等级基数: {nlv}";

        public struct CriticalHitResult
        {
            const int bn = 400;

            const int fn = 200;

            public int Value { get; set; }

            double Critical => Math.Floor(fn * ((double)Value - bn) / nlv);

            public double Rate => (50 + Critical) / 10;

            public double Bonus => (1400 + Critical) / 10;

            public double Expected => 1 + (Rate / 100 * (Bonus - 100) / 100);

            public int NextValue => (int)Math.Ceiling(bn + nlv * (1 + Critical) / fn);

            public CriticalHitResult(int value = bn)
                => Value = value;

            public static CriticalHitResult GetFromRate(double value)
                => new CriticalHitResult((int)Math.Ceiling((value * 10 - 50) * nlv / fn + bn));

            public static CriticalHitResult GetFromBonus(double value)
                => new CriticalHitResult((int)Math.Ceiling((value * 10 - 1400) * nlv / fn + bn));

            public override string ToString()
                => new StringBuilder()
                .AppendLine(Title)
                .AppendLine($"暴击 {Value} (基数: {bn})")
                .AppendLine($"暴击率　　　　{Rate}%")
                .AppendLine($"暴击伤害　　　{Bonus}%")
                .AppendLine($"预期收益　　　{Expected}")
                .Append($"下一临界点　　{NextValue}")
                .ToString();
        }

        public struct DirectHitResult
        {
            const int bn = 400;

            const int fn = 550;

            public int Value { get; set; }

            double Direct => Math.Floor(fn * ((double)Value - bn) / nlv);

            public double Rate => Direct / 10;

            public int NextValue => (int)Math.Ceiling(bn + nlv * (1 + Direct) / fn);

            public DirectHitResult(int value = bn)
                => Value = value;

            public static DirectHitResult GetFromRate(double value)
                => new DirectHitResult((int)Math.Ceiling(value * 10 * nlv / fn + bn));

            public override string ToString()
                => new StringBuilder()
                .AppendLine(Title)
                .AppendLine($"直击 {Value} (基数: {bn})")
                .AppendLine($"直击率　　　　{Rate}%")
                .AppendLine("直击伤害　　　125% (恒定)")
                .Append($"下一临界点　　{NextValue}")
                .ToString();
        }

        public struct DeterminationResult
        {
            const int bn = 390;

            const double fn = 140;

            public int Value { get; set; }

            double Determination => Math.Floor(fn * ((double)Value - bn) / nlv);

            public double Rate => (1000 + Determination) / 1000;

            public int NextValue => (int)Math.Ceiling(bn + nlv * (1 + Determination) / fn);

            public DeterminationResult(int value = bn)
                => Value = value;

            public static DeterminationResult GetFromRate(double value)
                => new DeterminationResult((int)Math.Ceiling((value * 1000 - 1000) * nlv / fn + bn));

            public override string ToString()
                => new StringBuilder()
                .AppendLine(Title)
                .AppendLine($"信念 {Value} (基数: {bn})")
                .AppendLine($"伤害增幅　　　{Rate}倍")
                .Append($"下一临界点　　{NextValue}")
                .ToString();
        }

        public struct TenacityResult
        {
            const int bn = 400;

            const double fn = 100;

            public int Value { get; set; }

            double Tenacity => Math.Floor(fn * ((double)Value - bn) / nlv);

            public double AttackRate => (1000 + Tenacity) / 1000;

            public double DamageRate => (1000 - Tenacity) / 10;

            public int NextValue => (int)Math.Ceiling(bn + nlv * (1 + Tenacity) / fn);

            public TenacityResult(int value = bn)
                => Value = value;

            public static TenacityResult GetFromAttackRate(double value)
                => new TenacityResult((int)Math.Ceiling((value * 1000 - 1000) * nlv / fn + bn));

            public static TenacityResult GetFromDamageRate(double value)
                => new TenacityResult((int)Math.Ceiling((1000 - value * 10) * nlv / fn + bn));

            public override string ToString()
                => new StringBuilder()
                .AppendLine(Title)
                .AppendLine($"坚韧 {Value} (基数: {bn})")
                .AppendLine($"伤害增幅　　　{AttackRate}倍 (仅防护职业)")
                .AppendLine($"受击伤害　　　{DamageRate}%")
                .Append($"下一临界点　　{NextValue}")
                .ToString();
        }

        public struct SpeedResult
        {
            const int bn = 400;

            const double fn = 130;

            public int Value { get; set; }

            double Speed => Math.Floor(fn * ((double)Value - bn) / nlv);

            public double Dot => (1000 + Speed) / 1000;

            double GCD => (1000 - Speed) / 1000;

            public double GCD15 => Math.Floor(Math.Floor(10000 * (Math.Floor(1500 * GCD) / 1000)) / 100) / 100;

            public double GCD20 => Math.Floor(Math.Floor(10000 * (Math.Floor(2000 * GCD) / 1000)) / 100) / 100;

            public double GCD25 => Math.Floor(Math.Floor(10000 * (Math.Floor(2500 * GCD) / 1000)) / 100) / 100;

            public double GCD28 => Math.Floor(Math.Floor(10000 * (Math.Floor(2800 * GCD) / 1000)) / 100) / 100;

            public double GCD30 => Math.Floor(Math.Floor(10000 * (Math.Floor(3000 * GCD) / 1000)) / 100) / 100;

            public double GCD80 => Math.Floor(Math.Floor(10000 * (Math.Floor(8000 * GCD) / 1000)) / 100) / 100;

            public int NextValue => (int)Math.Ceiling(bn + nlv * (1 + Speed) / fn);

            public int GCD25NextValue
            {
                get
                {
                    var gcd25 = GCD25;
                    var x = new SpeedResult(NextValue);
                    while (gcd25 == x.GCD25)
                        x = new SpeedResult(x.NextValue);
                    return x.Value;
                }
            }

            public int GCD28NextValue
            {
                get
                {
                    var gcd28 = GCD28;
                    var x = new SpeedResult(NextValue);
                    while (gcd28 == x.GCD28)
                        x = new SpeedResult(x.NextValue);
                    return x.Value;
                }
            }

            public SpeedResult(int value = bn)
                => Value = value;

            public static SpeedResult GetFromDOT(double value)
                => new SpeedResult((int)Math.Ceiling((value * 1000 - 1000) * nlv / fn + bn));

            public static SpeedResult GetFromGCD15(double value)
                => new SpeedResult((int)Math.Ceiling((1000 - value * 1000 / 1500 * 1000) * nlv / fn + bn));

            public static SpeedResult GetFromGCD20(double value)
                => new SpeedResult((int)Math.Ceiling((1000 - value * 1000 / 2000 * 1000) * nlv / fn + bn));

            public static SpeedResult GetFromGCD25(double value)
                => new SpeedResult((int)Math.Ceiling((1000 - value * 1000 / 2500 * 1000) * nlv / fn + bn));

            public static SpeedResult GetFromGCD28(double value)
                => new SpeedResult((int)Math.Ceiling((1000 - value * 1000 / 2800 * 1000) * nlv / fn + bn));

            public static SpeedResult GetFromGCD30(double value)
                => new SpeedResult((int)Math.Ceiling((1000 - value * 1000 / 3000 * 1000) * nlv / fn + bn));

            public static SpeedResult GetFromGCD80(double value)
                => new SpeedResult((int)Math.Ceiling((1000 - value * 1000 / 8000 * 1000) * nlv / fn + bn));

            public override string ToString()
                => new StringBuilder()
                .AppendLine(Title)
                .AppendLine($"速度 {Value} (基数: {bn})")
                .AppendLine($"DOT收益　　　　{Dot}倍")
                .AppendLine($"GCD　　　　　　{GCD25}s")
                .AppendLine($"1.5s　　　　　　{GCD15}s")
                .AppendLine($"2.0s　　　　　　{GCD20}s")
                .AppendLine($"2.8s　　　　　　{GCD28}s")
                .AppendLine($"3.0s　　　　　　{GCD30}s")
                .AppendLine($"复活 (8.0s)　　　{GCD80}s")
                .AppendLine("下一临界点")
                .AppendLine($"2.5s　　　　　　{GCD25NextValue}")
                .Append($"2.8s　　　　　　{GCD28NextValue}")
                .ToString();
        }

        public static CriticalHitResult CriticalHit(int value)
            => new CriticalHitResult(value);

        public static DirectHitResult DirectHit(int value)
            => new DirectHitResult(value);

        public static DeterminationResult Determination(int value)
            => new DeterminationResult(value);

        public static TenacityResult Tenacity(int value)
            => new TenacityResult(value);

        public static SpeedResult Speed(int value)
            => new SpeedResult(value);
    }
}
