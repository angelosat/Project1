using Project1.Framework;

namespace Project1.Core;

public readonly record struct MapId(int Value) : IStructIdInt<MapId>
{
    internal static readonly MapId Null = new(0);
    public static MapId Create(int value) => new(value);

    public static implicit operator MapId(int v) => new(v);
    public static implicit operator int(MapId v) => v.Value;
}
