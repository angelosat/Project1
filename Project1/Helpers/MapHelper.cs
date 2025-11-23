using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static public class MapHelper
    {
        static public IEnumerable<Vector3> FloodFill(this MapBase map, Vector3 begin)
        {
            var discovered = new HashSet<Vector3>();
            var current = begin;
            var cell = map.GetCell(current);
            discovered.Add(current);
            if (cell.IsRoomBorder)
                throw new Exception();
            var tohandle = new Queue<Vector3>();
            tohandle.Enqueue(current);
            yield return current;
            while (tohandle.Any())
            {
                current = tohandle.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (discovered.Contains(n))
                        continue;
                    if (map.TryGetCell(n, out var ncell))
                    {
                        yield return n;
                        if (!ncell.IsRoomBorder)
                        {
                            tohandle.Enqueue(n);
                            discovered.Add(n);
                        }
                    }
                    else
                    {

                    }
                }
            }
        }

        public static byte GetData(this Vector3 global, INetwork net)
        {
            return GetData(global, net.Map);
        }
        public static byte GetData(this Vector3 global, MapBase map)
        {
            return map.TryGetCell(global, out Cell cell) ? cell.BlockData : (byte)0;
        }
       
        public static IntVec2 GetChunkCoords(this Vector3 global)
        {
            int chunkX = (int)Math.Floor(Math.Round(global.X) / Chunk.Size);
            int chunkY = (int)Math.Floor(Math.Round(global.Y) / Chunk.Size);
            return new IntVec2(chunkX, chunkY);
        }

        static public bool IsWithinChunkBounds(this Vector3 local)
        {
            return !(local.X < 0 || local.X > Chunk.Size - 1 || local.Y < 0 || local.Y > Chunk.Size - 1 || local.Z < 0 || local.Z > MapBase.MaxHeight - 1);

        }
        static public bool IsWithinChunkBounds(this IntVec3 local)
        {
            return !(local.X < 0 || local.X > Chunk.Size - 1 || local.Y < 0 || local.Y > Chunk.Size - 1 || local.Z < 0 || local.Z > MapBase.MaxHeight - 1);

        }
        static public bool IsZWithinBounds(this Vector3 local)
        {
            return !(local.Z < 0 || local.Z > MapBase.MaxHeight - 1);
        }
        static public bool IsZWithinBounds(this IntVec3 local)
        {
            return !(local.Z < 0 || local.Z > MapBase.MaxHeight - 1);
        }
    }
}
