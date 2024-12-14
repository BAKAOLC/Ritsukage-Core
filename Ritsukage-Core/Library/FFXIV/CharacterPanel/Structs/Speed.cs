using System;
using System.Text;

namespace Ritsukage.Library.FFXIV.CharacterPanel.Structs
{
    public readonly struct Speed
    {
        private const int Fn = 130;

        public int Level { get; }

        public int Value { get; }

        private int Sub { get; }

        private int Div { get; }

        private double CalcValue { get; }

        public double Dot { get; }

        private double Gcd { get; }

        public double Gcd15 { get; }

        public double Gcd20 { get; }

        public double Gcd25 { get; }

        public double Gcd28 { get; }

        public double Gcd30 { get; }

        public double Gcd80 { get; }

        public int PrevValue { get; }

        public int NextValue { get; }

        public int DotPrevValue { get; }

        public int DotNextValue { get; }

        public int Gcd25PrevValue { get; }

        public int Gcd25NextValue { get; }

        public int Gcd28PrevValue { get; }

        public int Gcd28NextValue { get; }

        public Speed(int level, int value)
        {
            Level = level;
            Value = value;
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            Sub = modifier.Sub;
            Div = modifier.Div;

            CalcValue = Math.Floor(Fn * ((double)Value - Sub) / Div);
            Dot = (1000 + CalcValue) / 1000;
            Gcd = (1000 - CalcValue) / 1000;
            Gcd15 = GetGcdLength(1500, Gcd);
            Gcd20 = GetGcdLength(2000, Gcd);
            Gcd25 = GetGcdLength(2500, Gcd);
            Gcd28 = GetGcdLength(2800, Gcd);
            Gcd30 = GetGcdLength(3000, Gcd);
            Gcd80 = GetGcdLength(8000, Gcd);
            PrevValue = (int)Math.Ceiling(Sub + Div * CalcValue / Fn);
            NextValue = (int)Math.Ceiling(Sub + Div * (1 + CalcValue) / Fn);
            DotPrevValue = GetValueFromDot(Dot - 0.01, Div, Sub);
            DotNextValue = GetValueFromDot(Dot + 0.01, Div, Sub);
            Gcd25PrevValue = GetValueFromGcdLength(Gcd25 + 0.01, 2500, Div, Sub);
            Gcd25NextValue = GetValueFromGcdLength(Gcd25 - 0.01, 2500, Div, Sub);
            Gcd28PrevValue = GetValueFromGcdLength(Gcd28 + 0.01, 2800, Div, Sub);
            Gcd28NextValue = GetValueFromGcdLength(Gcd28 - 0.01, 2800, Div, Sub);
        }

        public static Speed FromRate(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, (int)Math.Ceiling((1000 - value * 10) * modifier.Div / Fn + modifier.Sub));
        }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendLine($"版本 {LevelModifiers.Version}  Lv{Level}  等级基数: {Div}")
                .AppendLine($"速度 {Value} (基数: {Sub})")
                .AppendLine($"DOT收益　　　　{Dot}倍")
                .AppendLine($"GCD　　　　　　{Gcd25}s")
                .AppendLine($"1.5s　　　　　　{Gcd15}s")
                .AppendLine($"2.0s　　　　　　{Gcd20}s")
                .AppendLine($"2.8s　　　　　　{Gcd28}s")
                .AppendLine($"3.0s　　　　　　{Gcd30}s")
                .AppendLine($"复活 (8.0s)　　　{Gcd80}s")
                .AppendLine("上一临界点")
                .AppendLine($"DOT收益　　　　{PrevValue}")
                .AppendLine($"2.5s　　　　　　{Gcd25PrevValue}")
                .AppendLine($"2.8s　　　　　　{Gcd28PrevValue}")
                .AppendLine("下一临界点")
                .AppendLine($"DOT收益　　　　{NextValue}")
                .AppendLine($"2.5s　　　　　　{Gcd25NextValue}")
                .Append($"2.8s　　　　　　{Gcd28NextValue}")
                .ToString();
        }

        public static Speed FromDot(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, GetValueFromDot(value, modifier.Div, modifier.Sub));
        }

        public static Speed FromGcd15(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, GetValueFromGcdLength(value, 1500, modifier.Div, modifier.Sub));
        }

        public static Speed FromGcd20(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, GetValueFromGcdLength(value, 2000, modifier.Div, modifier.Sub));
        }

        public static Speed FromGcd25(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, GetValueFromGcdLength(value, 2500, modifier.Div, modifier.Sub));
        }

        public static Speed FromGcd28(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, GetValueFromGcdLength(value, 2800, modifier.Div, modifier.Sub));
        }

        public static Speed FromGcd30(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, GetValueFromGcdLength(value, 3000, modifier.Div, modifier.Sub));
        }

        public static Speed FromGcd80(int level, double value)
        {
            if (!LevelModifiers.LevelTable.TryGetValue(level, out var modifier))
                throw new ArgumentOutOfRangeException(nameof(level), @"Invalid level");
            return new(level, GetValueFromGcdLength(value, 8000, modifier.Div, modifier.Sub));
        }

        private static int GetValueFromDot(double length, int div, int sub)
        {
            return (int)Math.Ceiling((length * 1000 - 1000) * div / Fn + sub);
        }

        private static int GetValueFromGcdLength(double length, double original, int div, int sub)
        {
            return (int)Math.Ceiling((1001 - (length + 0.01) * 1000 / original * 1000) * div / Fn + sub);
        }

        private static double GetGcdLength(int length, double gcd)
        {
            return Math.Floor(length * gcd / 10) / 100;
        }
    }
}