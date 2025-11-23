using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    static public class FloodFill
    {
        static public IEnumerable<IntVec3> BeginIncludeEdges(MapBase map, IntVec3 begin, Func<Cell, IntVec3, bool> condition)
        {
            var cell = map.GetCell(begin);
            if (!condition(cell, begin))
                throw new Exception();
            yield return begin;

            Queue<IntVec3> toHandle = new();
            HashSet<IntVec3> handled = new() { begin };
            toHandle.Enqueue(begin);
            while (toHandle.Any())
            {
                var current = toHandle.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    if (!map.TryGetCell(n, out var ncell))
                        continue;
                    yield return n;
                    if (condition(ncell, n))
                        toHandle.Enqueue(n);
                }
            }
        }

        static public HashSet<IntVec3> BeginExclusiveAsList(MapBase map, IntVec3 global)
        {
            var area = new HashSet<IntVec3>
            {
                global
            };
            var queue = new Queue<IntVec3>();
            var handled = new HashSet<IntVec3>() { global };
            queue.Enqueue(global);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    if (!map.Contains(n))
                        continue;

                    var cell = map.GetCell(n);
                    if (!cell.IsRoomBorder)
                    {
                        if (map.IsAboveHeightMap(n))
                            return null;
                        queue.Enqueue(n);
                        area.Add(n);
                    }

                }
            }
            return area;
        }
    }
}
