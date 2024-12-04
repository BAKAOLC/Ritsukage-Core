using System;

namespace Ritsukage.Tools
{
    public class WELL512
    {
        private uint _seed;
        private uint _index = 0;
        private readonly uint[] _state = new uint[16];

        private static uint GetMillisecond()
        {
            return Convert.ToUInt32(DateTimeOffset.Now.ToUnixTimeMilliseconds() % (Convert.ToInt64(uint.MaxValue) + 1));
        }

        public WELL512(uint seed)
        {
            SetSeed(seed);
        }

        public WELL512()
        {
            SetSeed(GetMillisecond());
        }

        private const uint mask = ~0u;

        public void SetSeed(uint seed)
        {
            _seed = seed;
            _index = 0;

            _state[0] = seed & mask;
            for (uint i = 1; i < 16; ++i)
                _state[i] = (uint)((1812433253UL * (_state[i - 1] ^ (_state[i - 1] >> 30)) + i) & mask);
        }

        public uint GetSeed()
        {
            return _seed;
        }

        public uint GetRandUInt()
        {
            uint a, b, c, d;
            a = _state[_index];
            c = _state[(_index + 13) & 15];
            b = a ^ c ^ (a << 16) ^ (c << 15);
            c = _state[(_index + 9) & 15];
            c ^= c >> 11;
            a = _state[_index] = b ^ c;
            d = (uint)(a ^ ((a << 5) & 0xDA442D24UL));
            _index = (_index + 15) & 15;
            a = _state[_index];
            _state[_index] = a ^ b ^ d ^ (a << 2) ^ (b << 18) ^ (c << 28);
            return _state[_index];
        }

        public uint GetRandUInt(uint max)
        {
            return GetRandUInt() % (max + 1);
        }

        public float GetRandFloat()
        {
            return GetRandUInt(1000000) / (float)1000000;
        }

        public float GetRandFloat(float min, float max)
        {
            return GetRandFloat() * (max - min) + min;
        }
    }
}