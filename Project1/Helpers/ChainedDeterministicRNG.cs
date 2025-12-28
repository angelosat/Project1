namespace Start_a_Town_
{
    public struct ChainedDeterministicRNG
    {
        private uint state;

        // Seed once per tick
        public ChainedDeterministicRNG(uint seed)
        {
            if (seed == 0) seed = 0xdeadbeef;
            state = seed;
        }

        // XORShift32 generator, updates state each call
        public uint NextUInt()
        {
            uint x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x;
        }

        // Integer in [0, max)
        public int NextInt(int max)
        {
            return (int)(NextUInt() % (uint)max);
        }

        // Float in [0.0, 1.0)
        public float NextFloat()
        {
            return NextUInt() / (float)uint.MaxValue;
        }

        // Float in [min, max)
        public float NextFloat(float min, float max)
        {
            return min + NextFloat() * (max - min);
        }
    }
}
