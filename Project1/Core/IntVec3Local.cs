using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core;

public readonly record struct IntVec3Local//IntVec3 Value) //int X, int Y, int Z)// 
{
    public readonly int X, Y, Z;
    public IntVec3Local(IntVec3 value)
    {
        var local = value.ToLocal();
        X = local.X;
        Y = local.Y;
        Z = local.Z;
    }
    public IntVec3Local(int x, int y, int z)
    {
        var local = new IntVec3(x, y, z).ToLocal();
        X = local.X;
        Y = local.Y;
        Z = local.Z;
    }
    public IntVec3 ToGlobal(Chunk chunk) => new IntVec3(X, Y, Z).ToGlobal(chunk);
    public static implicit operator IntVec3Local(IntVec3 v) => new(v);
}
