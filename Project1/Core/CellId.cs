using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core;

public readonly record struct CellId(int Value)
{
    internal static readonly CellId Null = new(0);
    public static implicit operator CellId(int v) => new(v);
    public static implicit operator int(CellId v) => v.Value;

    public int Z => Value / Chunk.SizeSquared;
    public int Y => Value / Chunk.Size % Chunk.Size;
    public int X => Value % Chunk.Size;

    public IntVec3Local Local => new(X, Y, Z);
    public IntVec3 GetGlobal(Chunk chunk) => Local.ToGlobal(chunk);
}
