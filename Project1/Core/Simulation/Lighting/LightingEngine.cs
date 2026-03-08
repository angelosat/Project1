using Project1.Framework;
using System;
using System.Collections.Generic;

namespace Project1.Core.Simulation.Lighting;

public sealed class LightingEngine(MapBase map)
{
    readonly MapBase Map = map;

    HashSet<IntVec3> Queued = [];
    Queue<IntVec3> Queue = [];

    readonly Queue<IntVec3> DarkenQueue = new();
    readonly HashSet<IntVec3> DarkenQueued = new();

    void Refresh(IEnumerable<IntVec3> vectors)
    {
        this.Queued.Clear();// = new();
        this.Queue.Clear();// = new();

        foreach (var v in vectors)
        {
            Queue.Enqueue(v);
            Queued.Add(v);
        }
    }

    public void HandleImmediate(IEnumerable<IntVec3> vectors)
    {
        this.Refresh(vectors);
        while (this.Queue.Count > 0)
            HandleSkyGlobalImmediate(this.Queue.Dequeue());

        this.Refresh(vectors);
        while (this.Queue.Count > 0)
            HandleBlockGlobalImmediate(this.Queue.Dequeue());
    }
    void HandleSkyGlobalImmediate(IntVec3 global)
    {
        if (!this.Map.TryQueryPosition(global, out var pos))
            return;

        var neighbors = global.GetAdjacentLazy();
        var nextLight = GetNextSunLightImmediate(pos, neighbors);

        var oldLight = pos.Chunk.GetSkylight(pos.CellIndex);
        int d = nextLight - oldLight;
        if (d != 0)
            pos.Chunk.SetSkylight(pos, nextLight);

        if (d > 1)
        {
            foreach (var n in neighbors)
                if (!Queued.Contains(n))
                {
                    Queue.Enqueue(n);
                    Queued.Add(n);
                }
        }
        else if (d < -1)
        {
            DarkenImmediateWorking(global);
        }
    }
    byte GetNextSunLightImmediate(PositionQuery pos, IEnumerable<IntVec3> neighbors)
    {
        byte next, maxAdjLight = 0;

        if (pos.Cell.Opaque)
            next = 0;
        else
        {
            if (pos.Chunk.IsAboveHeightMap(pos.Local))
                next = 15;
            else
            {
                foreach (var n in neighbors)
                {
                    if (!this.Map.TryQueryPosition(n, out var nquery))
                        continue;

                    if (nquery.Cell.Opaque)
                        continue;
                    var l = nquery.Chunk.GetSkylight(nquery.CellIndex);
                    maxAdjLight = Math.Max(maxAdjLight, l);
                }
                next = (byte)Math.Max(0, maxAdjLight - 1);
            }
        }
        return next;
    }
    void DarkenImmediateWorking(IntVec3 global)
    {
        this.DarkenQueue.Clear();
        this.DarkenQueued.Clear();
        this.DarkenQueue.Enqueue(global);
        this.DarkenQueued.Add(global);
        while (this.DarkenQueue.Count > 0)
        {
            var currentGlobal = this.DarkenQueue.Dequeue();
            byte nlight;
            if (!this.Map.TryQueryPosition(currentGlobal, out var pos))
                continue;
            if (pos.Chunk.IsAboveHeightMap(currentGlobal))
                continue;
            pos.Chunk.SetSkylight(pos, 0);

            var neighbors = currentGlobal.GetAdjacentLazy();
            foreach (var n in neighbors)
            {
                if (!this.Map.TryQueryPosition(n, out var nquery))
                    continue;
                if (nquery.Chunk.IsAboveHeightMap(n))
                {
                    if (!this.Queued.Contains(currentGlobal))
                    {
                        this.Queue.Enqueue(currentGlobal);
                        this.Queued.Add(currentGlobal);
                    }
                    continue;
                }
                nlight = nquery.Chunk.GetSkylight(nquery.CellIndex);

                if (!nquery.Cell.Opaque)
                    if (!this.DarkenQueued.Contains(n))
                    {
                        this.DarkenQueue.Enqueue(n);
                        this.DarkenQueued.Add(n);
                    }
            }
        }
    }
    private void HandleBlockGlobalImmediate(IntVec3 global)
    {
        this.Queued.Remove(global);
        if (!this.Map.TryQueryPosition(global, out var pos))
            return;
        var thisLight = pos.Chunk.GetBlockLight(pos.CellIndex);

        var nextLight = this.GetNextBlockLightImmediate(pos);
        pos.Chunk.SetBlockLight(pos, nextLight);

        if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
        {
            foreach (var n in global.GetAdjacentLazy())
            {
                if (!this.Queued.Contains(n))
                {
                    this.Queue.Enqueue(n);
                    this.Queued.Add(n);
                }
            }
        }

        else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
        {
            DarkenBlockImmediateWorking(global);
        }
    }
    byte GetNextBlockLightImmediate(PositionQuery pos)
    {
        byte maxAdjLight = 0;

        foreach (var n in pos.Global.GetAdjacentLazy())
        {
            if (!this.Map.TryQueryPosition(n, out var nquery))
                continue;
            if (nquery.Cell.Opaque)
                continue;
            byte l = nquery.Chunk.GetBlockLight(nquery.CellIndex);
            maxAdjLight = Math.Max(maxAdjLight, l);
        }

        if (pos.Cell.Opaque)
            return 0;
        if (pos.Cell.Luminance > 0)
            return pos.Cell.Luminance;
        return (byte)Math.Max(0, maxAdjLight - 1);
    }
    void DarkenBlockImmediateWorking(IntVec3 global)
    {
        this.DarkenQueue.Clear();
        this.DarkenQueued.Clear();
        this.DarkenQueue.Enqueue(global);
        this.DarkenQueued.Add(global);
        while (DarkenQueue.Count > 0)
        {
            var current = this.DarkenQueue.Dequeue();
            this.DarkenQueued.Remove(current);
            if (!this.Map.TryQueryPosition(current, out var pos))
                continue;
            if (pos.Cell.Opaque)
                continue;

            pos.Chunk.SetBlockLight(pos, pos.Cell.Luminance);

            foreach(var n in current.GetAdjacentLazy())
            {
                if (!this.Map.TryQueryPosition(n, out var npos))
                    continue;
                var nlight = npos.Chunk.GetBlockLight(npos.CellIndex);

                if (nlight > 0 && !npos.Cell.Opaque)
                {
                    if (!this.DarkenQueued.Contains(n))
                    {
                        this.DarkenQueue.Enqueue(n);
                        this.DarkenQueued.Add(n);
                    }
                    else
                    {
                        if (!this.Queued.Contains(n))
                        {
                            this.Queue.Enqueue(n);
                            this.Queued.Add(n);
                        }
                    }
                }
            }
        }
    }
}
//public class LightingEngine(MapBase map)
//{
//    readonly MapBase Map = map;

//    HashSet<IntVec3> Queued;
//    Queue<IntVec3> Queue;

//    void Refresh(IEnumerable<IntVec3> vectors)
//    {
//        this.Queued = new();
//        this.Queue = new();

//        foreach (var v in vectors)
//        {
//            Queue.Enqueue(v);
//            Queued.Add(v);
//        }
//    }

//    public void HandleImmediate(IEnumerable<IntVec3> vectors)
//    {
//        this.Refresh(vectors);
//        while (this.Queue.Count > 0)
//            HandleSkyGlobalImmediate(this.Queue.Dequeue(), this.Queue, this.Queued);

//        this.Refresh(vectors);

//        while (this.Queue.Count > 0)
//            HandleBlockGlobalImmediate(this.Queue.Dequeue(), this.Queue, this.Queued);
//    }
//    //public void HandleImmediate(IEnumerable<IntVec3> vectors)
//    //{
//    //    var queued = new HashSet<IntVec3>(vectors);
//    //    var queue = new Queue<IntVec3>(vectors);
//    //    while (queue.Count > 0)
//    //        HandleSkyGlobalImmediate(queue.Dequeue(), queue, queued);

//    //    queued = [.. vectors];
//    //    queue = new Queue<IntVec3>(vectors);
//    //    while (queue.Count > 0)
//    //        HandleBlockGlobalImmediate(queue.Dequeue(), queue, queued);
//    //}
//    void HandleSkyGlobalImmediate(IntVec3 global, Queue<IntVec3> queue, HashSet<IntVec3> queued)
//    {
//        byte oldLight, nextLight;
//        int gx = global.X, gy = global.Y, z = global.Z;

//        if (!this.Map.TryGetAll(gx, gy, z, out var thisChunk, out var thisCell, out int lx, out int ly))
//            return;
//        var neighbors = global.GetAdjacentLazy();
//        nextLight = GetNextSunLightImmediate(thisCell, thisChunk, gx, gy, z, lx, ly, neighbors);

//        oldLight = thisChunk.GetSunlight(lx, ly, z);
//        var local = global.ToLocal();
//        int d = nextLight - oldLight;
//        if (d != 0)
//            thisChunk.SetSunlight(local, nextLight);

//        if (d > 1)
//        {
//            foreach (var n in neighbors)
//                if (!queued.Contains(n))
//                {
//                    queue.Enqueue(n);// TODO: maybe check if the position is already queued?
//                    queued.Add(n);
//                }
//        }
//        else if (d < -1)
//        {
//            DarkenImmediateWorking(global, queue, queued);
//        }
//    }
//    byte GetNextSunLightImmediate(Cell cell, Chunk chunk, int gx, int gy, int z, int lx, int ly, IEnumerable<IntVec3> neighbors)
//    {
//        byte next, maxAdjLight = 0;

//        if (cell.Opaque)
//            next = 0;
//        else
//        {
//            if (chunk.IsAboveHeightMap(lx, ly, z))
//                next = 15;
//            else
//            {
//                foreach (var n in neighbors)
//                {
//                    if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
//                        continue;
//                    if (ncell.Opaque)
//                        continue;
//                    var l = nchunk.GetSunlight(n);
//                    maxAdjLight = Math.Max(maxAdjLight, l);
//                }
//                next = (byte)Math.Max(0, maxAdjLight - 1);
//            }
//        }
//        return next;
//    }
//    void DarkenImmediateWorking(IntVec3 global, Queue<IntVec3> queue, HashSet<IntVec3> queued)
//    {
//        var queueToDarken = new Queue<IntVec3>();
//        var queueToDarkenQueued = new HashSet<IntVec3>();
//        queueToDarken.Enqueue(global);
//        queueToDarkenQueued.Add(global);
//        while (queueToDarken.Count > 0)
//        {
//            var currentGlobal = queueToDarken.Dequeue();
//            byte nlight;
//            if (!this.Map.TryGetAll(currentGlobal, out var chunk, out var cell))
//                continue;

//            var local = currentGlobal.ToLocal();
//            if (chunk.IsAboveHeightMap(local))
//                continue;
//            chunk.SetSunlight(local, 0);

//            var neighbors = currentGlobal.GetAdjacentLazy();
//            foreach (var n in neighbors)
//            {
//                if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
//                    continue;
//                var nlocal = n.ToLocal();
//                if (nchunk.IsAboveHeightMap(nlocal))
//                {
//                    if (!queued.Contains(currentGlobal))
//                    {
//                        queue.Enqueue(currentGlobal);
//                        queued.Add(currentGlobal);
//                    }
//                    continue;
//                }
//                nlight = nchunk.GetSunlight(nlocal);

//                if (!ncell.Opaque)
//                    if (!queueToDarkenQueued.Contains(n))
//                    {
//                        queueToDarken.Enqueue(n);
//                        queueToDarkenQueued.Add(n);
//                    }
//            }
//        }
//    }
//    private void HandleBlockGlobalImmediate(IntVec3 global, Queue<IntVec3> queue, HashSet<IntVec3> queued)
//    {
//        byte nextLight;
//        queued.Remove(global);
//        if (!this.Map.TryGetAll(global, out var thisChunk, out var thisCell))
//            return;
//        var local = global.ToLocal();
//        var thisLight = thisChunk.GetBlockLight(local);

//        nextLight = GetNextBlockLightImmediate(thisCell, global);
//        thisChunk.SetBlockLight(local, nextLight);

//        if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
//        {
//            var adj = IntVec3.AdjacentIntVec3;
//            for (int i = 0; i < adj.Length; i++)
//            {
//                var n = global + adj[i];
//                if (!queued.Contains(n))
//                {
//                    queue.Enqueue(n);
//                    queued.Add(n);
//                }
//            }
//        }

//        else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
//        {
//            DarkenBlockImmediateWorking(global, queue, queued);
//        }
//    }
//    private byte GetNextBlockLightImmediate(Cell cell, IntVec3 center)
//    {
//        byte maxAdjLight = 0;
//        var adj = IntVec3.AdjacentIntVec3;
//        for (int i = 0; i < adj.Length; i++)
//        {
//            var n = center + adj[i];
//            if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
//                continue;
//            if (ncell.Opaque)
//                continue;
//            byte l = nchunk.GetBlockLight(n);
//            maxAdjLight = Math.Max(maxAdjLight, l);
//        }

//        if (cell.Opaque)
//            return 0;
//        if (cell.Luminance > 0)
//            return cell.Luminance;
//        return (byte)Math.Max(0, maxAdjLight - 1);
//    }
//    void DarkenBlockImmediateWorking(IntVec3 global, Queue<IntVec3> queue, HashSet<IntVec3> queued)
//    {
//        var queueToDarken = new Queue<IntVec3>();
//        var queueToDarkenQueued = new HashSet<IntVec3>();
//        queueToDarken.Enqueue(global);
//        queueToDarkenQueued.Add(global);
//        while (queueToDarken.Count > 0)
//        {
//            var current = queueToDarken.Dequeue();
//            queueToDarkenQueued.Remove(current);
//            if (!this.Map.TryGetAll(current, out var chunk, out var cell))
//                continue;
//            if (cell.Opaque)
//                continue;
//            var local = current.ToLocal();// cell.LocalCoords;// GetLocalCoords(chunk);

//            var prevLight = chunk.GetBlockLight(local);
//            chunk.SetBlockLight(local, cell.Luminance);

//            var adj = IntVec3.AdjacentIntVec3;
//            for (int i = 0; i < adj.Length; i++)
//            {
//                var n = current + adj[i];
//                if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
//                    continue;
//                var nlocal = n.ToLocal();// ncell.LocalCoords;// GetLocalCoords(nchunk);

//                var nlight = nchunk.GetBlockLight(nlocal);

//                // if neighbor light was less then current previous light, it means that the neighbor was lit from the current cell. so turn the neighbor light off
//                //if (nlight < prevLight) // maybe i have to remvoe this line as i did with the darkenskyblocks?
//                //{
//                    if (nlight > 0)
//                        // if neighbor cell isn't opaque, enqueue it to darken it
//                        if (!ncell.Opaque)
//                            if (!queueToDarkenQueued.Contains(n))
//                            {
//                                queueToDarken.Enqueue(n);
//                                queueToDarkenQueued.Add(n);
//                            }
//                //}
//                else
//                {
//                    if(!queued.Contains(n))
//                    {
//                        queue.Enqueue(n);
//                        queued.Add(n);
//                    }
//                }
//            }
//        }

//    }
//}
