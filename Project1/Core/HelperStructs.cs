using Microsoft.Xna.Framework;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;
using System;

namespace Project1.Core
{
    public readonly record struct EntityRefId(int Value)
    {
        internal static readonly EntityRefId Null = new(0);
        public static implicit operator EntityRefId(int v) => new(v);
        public static implicit operator int(EntityRefId v) => v.Value;
    }
    public readonly record struct MapId(int Value)
    {
        internal static readonly MapId Null = new(0);
        public static implicit operator MapId(int v) => new(v);
        public static implicit operator int(MapId v) => v.Value;
    }
    public readonly record struct PacketId(int Value)
    {
        public static implicit operator PacketId(int v) => new(v);
        public static implicit operator int(PacketId v) => v.Value;
    }
    public readonly record struct Tick(double Value)
    {
        public static implicit operator Tick(double v) => new(v);
        public static implicit operator double(Tick v) => v.Value;
    }
    public record struct PlayerId(int Value)
    {
        public static implicit operator PlayerId(int v) => new(v);
        public static implicit operator int(PlayerId v) => (int)v.Value;
    }
    public readonly record struct ZoneId(int Value)
    {
        internal static readonly ZoneId Null = new(0);
        public static implicit operator ZoneId(int v) => new(v);
        public static implicit operator int(ZoneId v) => v.Value;
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
    public readonly record struct IntVec3Local//IntVec3 Value) //int X, int Y, int Z)// 
    {
        public readonly int X, Y, Z;
        public IntVec3Local(IntVec3 value)
        {
            var local = value.ToLocal();
            this.X = local.X;
            this.Y = local.Y;
            this.Z = local.Z;
        }
        public IntVec3Local(int x, int y, int z)
        {
            var local = new IntVec3(x, y, z).ToLocal();
            this.X = local.X;
            this.Y = local.Y;
            this.Z = local.Z;
        }
        public IntVec3 ToGlobal(Chunk chunk) => new IntVec3(this.X, this.Y, this.Z).ToGlobal(chunk);
        public static implicit operator IntVec3Local(IntVec3 v) => new(v);
    }
}
