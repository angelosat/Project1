using Project1.Core.Helpers;

namespace Project1.Core;

public readonly record struct WorldSeed(int Value)
{
    public static implicit operator WorldSeed(int v) => new(v);
    public static implicit operator int(WorldSeed v) => v.Value;
    public static WorldSeed Mix(WorldSeed first, WorldSeed second) => first.MixWith(second);
}
