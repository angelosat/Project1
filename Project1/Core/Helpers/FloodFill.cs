using Project1.Core.Simulation;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Helpers
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
            HashSet<IntVec3> handled = [begin];
            toHandle.Enqueue(begin);
            while (toHandle.Count != 0)
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
        public sealed class FloodFillResult
        {
            public readonly HashSet<IntVec3> Interior = new();
            public readonly HashSet<IntVec3> Edges = new();
            public readonly HashSet<IntVec3> Handled = new();

            public bool TouchedMapEdge;
            public bool TouchedAboveHeight;

            public bool IsEnclosed => !TouchedMapEdge && !TouchedAboveHeight;
        }
        public static FloodFillResult FloodFillRegion(
            MapBase map,
            IntVec3 begin,
            Func<Cell, IntVec3, bool> isInterior)
        {
            var result = new FloodFillResult();

            if (!map.TryGetCell(begin, out var startCell))
                return result;

            if (!isInterior(startCell, begin))
                return result;

            Queue<IntVec3> q = new();
            q.Enqueue(begin);
            result.Handled.Add(begin);
            result.Interior.Add(begin);

            while (q.Count > 0)
            {
                var current = q.Dequeue();

                foreach (var n in current.GetAdjacentLazy())
                {
                    if (result.Handled.Contains(n))
                        continue;

                    result.Handled.Add(n);

                    // Outside map → invalid room
                    if (!map.TryGetCell(n, out var ncell))
                    {
                        result.TouchedMapEdge = true;
                        continue;
                    }

                    // Above height map → outdoors leak
                    if (map.IsAboveHeightMap(n))
                    {
                        result.TouchedAboveHeight = true;
                        continue;
                    }

                    if (isInterior(ncell, n))
                    {
                        result.Interior.Add(n);
                        q.Enqueue(n);
                    }
                    else
                    {
                        result.Edges.Add(n);
                    }
                }
            }

            return result;
        }
        //struct FloodFillResult
        //{
        //    public bool IsValid;
        //    public HashSet<IntVec3> Handled;
        //    public HashSet<IntVec3> Interior;
        //    public HashSet<IntVec3> Edges;
        //}
        //static public FloodFillResult BeginIncludeEdgesNew(MapBase map, IntVec3 begin, Func<Cell, IntVec3, bool> condition)
        //{
        //    var cell = map.GetCell(begin);
        //    if (!condition(cell, begin))
        //        throw new Exception();
        //    List<IntVec3> positions = [begin];


        //    Queue<IntVec3> toHandle = new();
        //    HashSet<IntVec3> handled = [begin];
        //    toHandle.Enqueue(begin);
        //    while (toHandle.Count != 0)
        //    {
        //        var current = toHandle.Dequeue();
        //        foreach (var n in current.GetAdjacentLazy())
        //        {
        //            if (handled.Contains(n))
        //                continue;
        //            handled.Add(n);
        //            if (!map.TryGetCell(n, out var ncell))
        //                continue;
        //            positions.Add(n);
        //            if (condition(ncell, n))
        //                toHandle.Enqueue(n);
        //        }
        //    }
        //    return new() { IsValid}
        //}
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
