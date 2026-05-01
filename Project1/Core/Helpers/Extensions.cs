using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Framework.Helpers;
using System.Collections.Generic;

namespace Project1.Core.Helpers;

public static class ExtensionsNew
{
    public static Vector3 ToLocal(this Vector3 global)
    {
        float rx, ry;
        rx = global.X % Chunk.Size;
        rx = rx < 0 ? rx + Chunk.Size : rx;
        ry = global.Y % Chunk.Size;
        ry = ry < 0 ? ry + Chunk.Size : ry;
        return new Vector3(rx, ry, global.Z);
    }
    
    static public bool ContainsEntityFootprint(this Vector3 blockGlobal, GameObject entity)
    {
        var footprint = entity.GetFootprint();
        var blockbox = blockGlobal.GetBoundingBox();
        var containment = blockbox.Contains(footprint);
        return containment == ContainmentType.Contains;
    }
    public static List<Vector2> GetSpiral(this Vector2 center, int radius = Engine.ChunkRadius)
    {
        var list = new List<Vector2>();


        for (int i = -radius; i <= radius; i++)
            for (int j = -radius; j <= radius; j++)
            {
                var vec2 = new Vector2(i, j);
                if (vec2.Length() <= radius)
                    list.Add(center + vec2);
            }
        list.Sort((u, v) =>
        {
            if (u == v) return 0;
            else if (Vector2.DistanceSquared(center, u) < Vector2.DistanceSquared(center, v)) return -1;
            else return 1;
        });
        return list;
    }

}
