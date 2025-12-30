namespace Start_a_Town_
{
    public readonly record struct EntityRefId(int Value)
    {
        internal static readonly EntityRefId Null = new(0);
        public static implicit operator EntityRefId(int v) => new(v);
        public static implicit operator int(EntityRefId v) => v.Value;
    }
    public readonly record struct PacketId(int Value)
    {
        public static implicit operator PacketId(int v) => new(v);
        public static implicit operator int(PacketId v) => v.Value;
    }
    public readonly record struct SlotIndex(int Value)
    {
        internal static readonly SlotIndex Null = new(-1);
        public static implicit operator SlotIndex(int v) => new(v);
        public static implicit operator int(SlotIndex v) => v.Value;
    }
    public readonly record struct WorldSeed(int Value)
    {
        public static implicit operator WorldSeed(int v) => new(v);
        public static implicit operator int(WorldSeed v) => v.Value;
        public static WorldSeed Mix(WorldSeed first, WorldSeed second) => first.MixWith(second);
        
    }
    public static class SeedExtensions
    {
        static public WorldSeed MixWith(this WorldSeed seed, WorldSeed other)
        {
            unchecked
            {
                return seed * 397 ^ other;
            }
        }
    }
}
