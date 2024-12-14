using System;
using System.Text;

namespace Ritsukage.Library.FFXIV.CharacterPanel.Structs
{
    public readonly struct Piety
    {
        private const int Fn = 150;
        private const int Base = 200;

        public int Level { get; }

        public int Value { get; }

        private int Main { get; }

        private int Div { get; }

        private double CalcValue { get; }

        public int Result { get; }

        public int PrevValue { get; }

        public int NextValue { get; }

        public Piety(int level, int value)
        {
            Level = level;
            Value = value;
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            Main = modifier.Main;
            Div = modifier.Div;

            CalcValue = Math.Floor(Fn * ((double)Value - Main) / Div);
            Result = (int)(Base + CalcValue);
            PrevValue = (int)Math.Ceiling(Main + Div * CalcValue / Fn);
            NextValue = (int)Math.Ceiling(Main + Div * (1 + CalcValue) / Fn);
        }

        public static Piety FromResult(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, (int)Math.Floor((value - Base + 1) * modifier.Div / Fn + modifier.Main));
        }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendLine($"版本 {LevelModifiers.Version}  Lv{Level}  等级基数: {Div}")
                .AppendLine($"信仰 {Value} (基数: {Main})")
                .AppendLine($"回蓝量　　　　{Result}")
                .AppendLine($"上一临界点　　{PrevValue}")
                .Append($"下一临界点　　{NextValue}")
                .ToString();
        }
    }
}