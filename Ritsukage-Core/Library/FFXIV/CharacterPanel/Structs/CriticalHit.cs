using System;
using System.Text;

namespace Ritsukage.Library.FFXIV.CharacterPanel.Structs
{
    public readonly struct CriticalHit
    {
        private const int Fn = 200;

        public int Level { get; }

        public int Value { get; }

        private int Sub { get; }

        private int Div { get; }

        private double CalcValue { get; }

        public double Rate { get; }

        public double Bonus { get; }

        public double Expected { get; }

        public int PrevValue { get; }

        public int NextValue { get; }

        public CriticalHit(int level, int value)
        {
            Level = level;
            Value = value;
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            Sub = modifier.Sub;
            Div = modifier.Div;

            CalcValue = Math.Floor(Fn * ((double)Value - Sub) / Div);
            Rate = (50 + CalcValue) / 10;
            Bonus = (1400 + CalcValue) / 10;
            Expected = Rate * (Bonus - 100) / 100;
            PrevValue = (int)Math.Ceiling(Sub + Div * CalcValue / Fn);
            NextValue = (int)Math.Ceiling(Sub + Div * (1 + CalcValue) / Fn);
        }

        public static CriticalHit FromRate(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, (int)Math.Ceiling((value * 10 - 50) * modifier.Div / Fn + modifier.Sub));
        }

        public static CriticalHit FromBonus(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, (int)Math.Ceiling((value * 10 - 1400) * modifier.Div / Fn + modifier.Sub));
        }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendLine($"版本 {LevelModifiers.Version}  Lv{Level}  等级基数: {Div}")
                .AppendLine($"暴击 {Value} (基数: {Sub})")
                .AppendLine($"暴击率　　　　{Rate}%")
                .AppendLine($"暴击伤害　　　{Bonus}%")
                .AppendLine($"预期收益　　　{Expected}%")
                .AppendLine($"上一临界点　　{PrevValue}")
                .Append($"下一临界点　　{NextValue}")
                .ToString();
        }
    }
}