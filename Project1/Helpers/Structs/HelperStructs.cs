using System.Runtime.CompilerServices;

namespace Start_a_Town_
{
    public readonly struct EntityRefId(int value)
    {
        internal static readonly EntityRefId Null = new(0);
        public readonly int Value = value;
        public static implicit operator EntityRefId(int v) => new(v);
        public static implicit operator int(EntityRefId v) => v.Value;
        public override string ToString() => $"{nameof(EntityRefId)}: {this.Value}";
    }
    public readonly struct PacketId(int value)
    {
        public readonly int Value = value;
        public static implicit operator PacketId(int v) => new(v);
        public static implicit operator int(PacketId v) => v.Value;
    }
    public readonly struct SlotIndex(int value)
    {
        internal static readonly SlotIndex Null = new(-1);
        public readonly int Value = value;
        public static implicit operator SlotIndex(int v) => new(v);
        public static implicit operator int(SlotIndex v) => v.Value;
        public override string ToString() => $"{nameof(SlotIndex)}: {this.Value}";
    }
    public readonly struct WorldSeed(int value)
    {
        public readonly int Value = value;
        public static implicit operator WorldSeed(int v) => new(v);
        public static implicit operator int(WorldSeed v) => v.Value;
        public override string ToString() => $"{nameof(WorldSeed)}: {this.Value}";
        public static WorldSeed Mix(WorldSeed first, WorldSeed second)
        {
            return first.MixWith(second);
        }
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
