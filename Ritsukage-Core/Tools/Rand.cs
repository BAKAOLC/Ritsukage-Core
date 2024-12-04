using System;

namespace Ritsukage.Tools
{
    public class Rand
    {
        private readonly WELL512 rand = new();

        public uint Seed()
        {
            return rand.GetSeed();
        }

        public void Seed(uint seed)
        {
            rand.SetSeed(seed);
        }

        public int Int(int a, int b)
        {
            return a < b ? a + (int)rand.GetRandUInt((uint)(b - a))
                : a == b ? a : b + (int)rand.GetRandUInt((uint)(a - b));
        }

        public float Float()
        {
            return rand.GetRandFloat();
        }

        public float Float(float a, float b)
        {
            return rand.GetRandFloat(a, b);
        }

        public int Sign()
        {
            return (int)rand.GetRandUInt(1) * 2 - 1;
        }

        public double BoxMuller(double a, double b)
        {
            if (a > b)
                return BoxMuller(b, a);
            else if (a == b)
                return a;
            else
                return a + (b - a) * _BoxMuller(Float(), Float());
        }

        public double BoxMuller()
        {
            return BoxMuller(0, 1);
        }

        private double _BoxMuller(double u, double v)
        {
            var z = Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v);
            z = (z + 3) / 6;
            return Math.Clamp(z, 0, 1);
        }
    }
}