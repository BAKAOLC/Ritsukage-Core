using System;
using System.Text;

namespace Ritsukage.Library.FFXIV.CharacterPanel.Structs
{
    public readonly struct Tenacity
    {
        private const int Fn = 200;

        public int Level { get; }

        public int Value { get; }

        private int Sub { get; }

        private int Div { get; }

        private double CalcValue { get; }

        public double Rate { get; }

        public double Bonus { get; }

        public int PrevValue { get; }

        public int NextValue { get; }

        public Tenacity(int level, int value)
        {
            Level = level;
            Value = value;
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            Sub = modifier.Sub;
            Div = modifier.Div;

            CalcValue = Math.Floor(Fn * ((double)Value - Sub) / Div);
            Rate = (1000 - CalcValue) / 10;
            Bonus = (1000 + CalcValue) / 1000;
            PrevValue = (int)Math.Ceiling(Sub + Div * CalcValue / Fn);
            NextValue = (int)Math.Ceiling(Sub + Div * (1 + CalcValue) / Fn);
        }

        public static Tenacity FromRate(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, (int)Math.Ceiling((1000 - value * 10) * modifier.Div / Fn + modifier.Sub));
        }

        public static Tenacity FromBonus(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, (int)Math.Ceiling((value * 1000 - 1000) * modifier.Div / Fn + modifier.Sub));
        }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendLine($"版本 {LevelModifiers.Version}  Lv{Level}  等级基数: {Div}")
                .AppendLine($"坚韧 {Value} (基数: {Sub})")
                .AppendLine($"伤害增幅　　　{Bonus}倍")
                .AppendLine($"受击伤害　　　{Rate}%")
                .AppendLine($"上一临界点　　{PrevValue}")
                .Append($"下一临界点　　{NextValue}")
                .ToString();
        }
    }
}