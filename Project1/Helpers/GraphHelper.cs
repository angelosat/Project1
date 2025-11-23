using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public static class GraphHelper
    {
        static public bool IsConnectedNew(this IEnumerable<IntVec3> globals)
        {
            var unvisited = globals.ToHashSet();
            var first = globals.First();
            var queue = new Queue<Vector3>();
            queue.Enqueue(first);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                unvisited.Remove(current);
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (unvisited.Contains(n))
                        queue.Enqueue(n);
                }
            }
            return !unvisited.Any();
        }
        static public List<HashSet<IntVec3>> GetAllConnectedSubGraphs(this IEnumerable<IntVec3> all)
        {
            var splitgraphs = new List<HashSet<IntVec3>>();
            var tocheck = all;

            do
            {
                var (connected, disconnected) = tocheck.GetConnectedSubGraph();
                splitgraphs.Add(connected);
                tocheck = disconnected;
            } while (tocheck.Any());
            return splitgraphs;
        }
        /// <summary>
        /// the disconnected graph might be further disconnected in itself
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        static public (HashSet<IntVec3> connected, HashSet<IntVec3> disconnected) GetConnectedSubGraph(this IEnumerable<IntVec3> positions)
        {
            var disconnected = positions.ToHashSet();
            var connected = new HashSet<IntVec3>();
            var first = positions.First();
            var queue = new Queue<Vector3>();
            queue.Enqueue(first);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                disconnected.Remove(current);
                connected.Add(current);
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (disconnected.Contains(n))
                        queue.Enqueue(n);
                }
            }
            return (connected, disconnected);
        }
    }
}
