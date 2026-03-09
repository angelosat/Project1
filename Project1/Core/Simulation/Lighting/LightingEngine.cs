using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project1.Core.Simulation.Lighting;

public sealed class LightingEngine(MapBase map)
{
    readonly Dictionary<IntVec3, PositionQuery> Cache = [];
     bool TryQueryPosition(IntVec3 global, out PositionQuery pos)
    {
        //return this.Map.TryQueryPosition(global, out pos);

        if (this.Cache.TryGetValue(global, out pos))
            return true;

        if (!this.Map.TryQueryPosition(global, out pos))
            return false;
        
        this.Cache.Add(global, pos);
        return true;
    }

    readonly MapBase Map = map;

    HashSet<IntVec3> Queued = [];
    Queue<PositionQuery> Queue = [];

    readonly Queue<PositionQuery> DarkenQueue = new();
    readonly HashSet<IntVec3> DarkenQueued = new();

    void Refresh(IEnumerable<PositionQuery> vectors)
    {
        this.Queued.Clear();
        this.Queue.Clear();

        foreach (var v in vectors)
        {
            this.Queue.Enqueue(v);
            //this.Queued.Add(v.GlobalCellId);
            this.Queued.Add(v.Global);
        }
    }

    public void HandleImmediate(IEnumerable<PositionQuery> vectors)
    {
        this.Cache.Clear();

        this.Refresh(vectors);
        //var count = 0;
        //var s = Stopwatch.StartNew();
        while (this.Queue.Count > 0)
        {
            HandleSkyGlobalImmediate(this.Queue.Dequeue());
            //count++;

        }
        //$"sky: {count} blocks in {s.ElapsedMilliseconds}ms".ToConsole();

        this.Refresh(vectors);
        var count = 0;
        var s = Stopwatch.StartNew();
        while (this.Queue.Count > 0)
        {
            HandleBlockGlobalImmediate(this.Queue.Dequeue());
            count++;
        }
        s.Stop();
        $"block: {count} blocks in {s.ElapsedMilliseconds}ms".ToConsole();
    }
    void HandleSkyGlobalImmediate(PositionQuery pos)
    {
        var neighbors = pos.Global.GetAdjacentLazy();
        var nextLight = GetNextSunLightImmediate(pos, neighbors);

        var oldLight = pos.Chunk.GetSkylight(pos.CellIndex);
        int d = nextLight - oldLight;
        if (d != 0)
            pos.Chunk.SetSkylight(pos, nextLight);

        if (d > 1)
        {
            foreach (var n in neighbors)
            {
                if (!this.TryQueryPosition(n, out var npos))
                    continue;
                //if (!this.Queued.Contains(npos.GlobalCellId))
                if (!this.Queued.Contains(npos.Global))
                {
                    this.Queue.Enqueue(npos);
                    //this.Queued.Add(npos.GlobalCellId);
                    this.Queued.Add(npos.Global);
                }
            }
        }
        else if (d < -1)
        {
            DarkenImmediateWorking(pos);
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
                    if (!this.TryQueryPosition(n, out var nquery))
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
    void DarkenImmediateWorking(PositionQuery origin)
    {
        this.DarkenQueue.Clear();
        this.DarkenQueued.Clear();
        this.DarkenQueue.Enqueue(origin);
        //this.DarkenQueued.Add(origin.GlobalCellId);
        this.DarkenQueued.Add(origin.Global);
        while (this.DarkenQueue.Count > 0)
        {
            var pos = this.DarkenQueue.Dequeue();
            byte nlight;
            if (pos.Chunk.IsAboveHeightMap(pos.Local))
                continue;
            pos.Chunk.SetSkylight(pos, 0);

            var neighbors = pos.Global.GetAdjacentLazy();
            foreach (var n in neighbors)
            {
                if (!this.TryQueryPosition(n, out var nquery))
                    continue;
                if (nquery.Chunk.IsAboveHeightMap(n))
                {
                    //if (!this.Queued.Contains(pos.GlobalCellId))
                    if (!this.Queued.Contains(pos.Global))
                    {
                        this.Queue.Enqueue(pos);
                        //this.Queued.Add(pos.GlobalCellId);
                        this.Queued.Add(pos.Global);
                    }
                    continue;
                }
                nlight = nquery.Chunk.GetSkylight(nquery.CellIndex);

                if (!nquery.Cell.Opaque)
                    //if (!this.DarkenQueued.Contains(nquery.GlobalCellId))
                    if (!this.DarkenQueued.Contains(nquery.Global))
                    {
                        this.DarkenQueue.Enqueue(nquery);
                        //this.DarkenQueued.Add(nquery.GlobalCellId);
                        this.DarkenQueued.Add(nquery.Global);
                    }
            }
        }
    }
    private void HandleBlockGlobalImmediate(PositionQuery pos)
    {
        this.Queued.Remove(pos.Global);
        //this.Queued.Remove(pos.GlobalCellId);
        var thisLight = pos.Chunk.GetBlockLight(pos.CellIndex);

        var nextLight = this.GetNextBlockLightImmediate(pos);
        pos.Chunk.SetBlockLight(pos, nextLight);

        if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
        {
            foreach (var n in pos.Global.GetAdjacentLazy())
            {
                if (!this.TryQueryPosition(n, out var npos))
                    continue;
                //if (!this.Queued.Contains(npos.GlobalCellId))
                if (!this.Queued.Contains(npos.Global))
                {
                    this.Queue.Enqueue(npos);
                    //this.Queued.Add(npos.GlobalCellId);
                    this.Queued.Add(npos.Global);
                }
            }
        }

        else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
        {
            DarkenBlockImmediateWorking(pos);
        }
    }
    byte GetNextBlockLightImmediate(PositionQuery pos)
    {
        byte maxAdjLight = 0;

        foreach (var n in pos.Global.GetAdjacentLazy())
        {
            if (!this.TryQueryPosition(n, out var nquery))
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
    void DarkenBlockImmediateWorking(PositionQuery origin)
    {
        this.DarkenQueue.Clear();
        this.DarkenQueued.Clear();
        this.DarkenQueue.Enqueue(origin);
        //this.DarkenQueued.Add(origin.GlobalCellId);
        this.DarkenQueued.Add(origin.Global);
        while (DarkenQueue.Count > 0)
        {
            var pos = this.DarkenQueue.Dequeue();
            //this.DarkenQueued.Remove(pos.GlobalCellId);
            this.DarkenQueued.Remove(pos.Global);
            if (pos.Cell.Opaque)
                continue;

            pos.Chunk.SetBlockLight(pos, pos.Cell.Luminance);

            foreach (var n in pos.Global.GetAdjacentLazy())
            {
                if (!this.TryQueryPosition(n, out var npos))
                    continue;
                var nlight = npos.Chunk.GetBlockLight(npos.CellIndex);

                if (nlight > 0 && !npos.Cell.Opaque)
                {
                    //if (!this.DarkenQueued.Contains(npos.GlobalCellId))
                    if (!this.DarkenQueued.Contains(npos.Global))
                    {
                        this.DarkenQueue.Enqueue(npos);
                        this.DarkenQueued.Add(npos.Global);
                        //this.DarkenQueued.Add(npos.GlobalCellId);
                    }
                    else
                    {
                        if (!this.Queued.Contains(npos.Global))
                        //if (!this.Queued.Contains(npos.GlobalCellId))
                        {
                            this.Queue.Enqueue(npos);
                            //this.Queued.Add(npos.GlobalCellId);
                            this.Queued.Add(npos.Global);
                        }
                    }
                }
            }
        }
    }
}
//public sealed class LightingEngine(MapBase map)
//{
//    readonly MapBase Map = map;

//    HashSet<GlobalCellId> Queued = [];
//    Queue<PositionQuery> Queue = [];

//    readonly Queue<PositionQuery> DarkenQueue = new();
//    readonly HashSet<GlobalCellId> DarkenQueued = new();

//    void Refresh(IEnumerable<PositionQuery> vectors)
//    {
//        this.Queued.Clear();
//        this.Queue.Clear();

//        foreach (var v in vectors)
//        {
//            this.Queue.Enqueue(v);
//            this.Queued.Add(v.GlobalCellId);
//        }
//    }

//    public void HandleImmediate(IEnumerable<PositionQuery> vectors)
//    {
//        this.Refresh(vectors);
//        while (this.Queue.Count > 0)
//            HandleSkyGlobalImmediate(this.Queue.Dequeue());

//        this.Refresh(vectors);
//        while (this.Queue.Count > 0)
//            HandleBlockGlobalImmediate(this.Queue.Dequeue());
//    }
//    void HandleSkyGlobalImmediate(PositionQuery pos)
//    {
//        var neighbors = pos.Global.GetAdjacentLazy();
//        var nextLight = GetNextSunLightImmediate(pos, neighbors);

//        var oldLight = pos.Chunk.GetSkylight(pos.CellIndex);
//        int d = nextLight - oldLight;
//        if (d != 0)
//            pos.Chunk.SetSkylight(pos, nextLight);

//        if (d > 1)
//        {
//            foreach (var n in neighbors)
//            {
//                if (!this.Map.TryQueryPosition(n, out var npos))
//                    continue;
//                if (!this.Queued.Contains(npos.GlobalCellId))
//                {
//                    this.Queue.Enqueue(npos);
//                    this.Queued.Add(npos.GlobalCellId);
//                }
//            }
//        }
//        else if (d < -1)
//        {
//            DarkenImmediateWorking(pos);
//        }
//    }
//    byte GetNextSunLightImmediate(PositionQuery pos, IEnumerable<IntVec3> neighbors)
//    {
//        byte next, maxAdjLight = 0;

//        if (pos.Cell.Opaque)
//            next = 0;
//        else
//        {
//            if (pos.Chunk.IsAboveHeightMap(pos.Local))
//                next = 15;
//            else
//            {
//                foreach (var n in neighbors)
//                {
//                    if (!this.Map.TryQueryPosition(n, out var nquery))
//                        continue;

//                    if (nquery.Cell.Opaque)
//                        continue;
//                    var l = nquery.Chunk.GetSkylight(nquery.CellIndex);
//                    maxAdjLight = Math.Max(maxAdjLight, l);
//                }
//                next = (byte)Math.Max(0, maxAdjLight - 1);
//            }
//        }
//        return next;
//    }
//    void DarkenImmediateWorking(PositionQuery origin)
//    {
//        this.DarkenQueue.Clear();
//        this.DarkenQueued.Clear();
//        this.DarkenQueue.Enqueue(origin);
//        this.DarkenQueued.Add(origin.GlobalCellId);
//        while (this.DarkenQueue.Count > 0)
//        {
//            var pos = this.DarkenQueue.Dequeue();
//            byte nlight;
//            if (pos.Chunk.IsAboveHeightMap(pos.Local))
//                continue;
//            pos.Chunk.SetSkylight(pos, 0);

//            var neighbors = pos.Global.GetAdjacentLazy();
//            foreach (var n in neighbors)
//            {
//                if (!this.Map.TryQueryPosition(n, out var nquery))
//                    continue;
//                if (nquery.Chunk.IsAboveHeightMap(n))
//                {
//                    if (!this.Queued.Contains(pos.GlobalCellId))
//                    {
//                        this.Queue.Enqueue(pos);
//                        this.Queued.Add(pos.GlobalCellId);
//                    }
//                    continue;
//                }
//                nlight = nquery.Chunk.GetSkylight(nquery.CellIndex);

//                if (!nquery.Cell.Opaque)
//                    if (!this.DarkenQueued.Contains(nquery.GlobalCellId))
//                    {
//                        this.DarkenQueue.Enqueue(nquery);
//                        this.DarkenQueued.Add(nquery.GlobalCellId);
//                    }
//            }
//        }
//    }
//    private void HandleBlockGlobalImmediate(PositionQuery pos)
//    {
//        this.Queued.Remove(pos.GlobalCellId);
//        var thisLight = pos.Chunk.GetBlockLight(pos.CellIndex);

//        var nextLight = this.GetNextBlockLightImmediate(pos);
//        pos.Chunk.SetBlockLight(pos, nextLight);

//        if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
//        {
//            foreach (var n in pos.Global.GetAdjacentLazy())
//            {
//                if (!this.Map.TryQueryPosition(n, out var npos))
//                    continue;
//                if (!this.Queued.Contains(npos.GlobalCellId))
//                {
//                    this.Queue.Enqueue(npos);
//                    this.Queued.Add(npos.GlobalCellId);
//                }
//            }
//        }

//        else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
//        {
//            DarkenBlockImmediateWorking(pos);
//        }
//    }
//    byte GetNextBlockLightImmediate(PositionQuery pos)
//    {
//        byte maxAdjLight = 0;

//        foreach (var n in pos.Global.GetAdjacentLazy())
//        {
//            if (!this.Map.TryQueryPosition(n, out var nquery))
//                continue;
//            if (nquery.Cell.Opaque)
//                continue;
//            byte l = nquery.Chunk.GetBlockLight(nquery.CellIndex);
//            maxAdjLight = Math.Max(maxAdjLight, l);
//        }

//        if (pos.Cell.Opaque)
//            return 0;
//        if (pos.Cell.Luminance > 0)
//            return pos.Cell.Luminance;
//        return (byte)Math.Max(0, maxAdjLight - 1);
//    }
//    void DarkenBlockImmediateWorking(PositionQuery origin)
//    {
//        this.DarkenQueue.Clear();
//        this.DarkenQueued.Clear();
//        this.DarkenQueue.Enqueue(origin);
//        this.DarkenQueued.Add(origin.GlobalCellId);
//        while (DarkenQueue.Count > 0)
//        {
//            var pos = this.DarkenQueue.Dequeue();
//            this.DarkenQueued.Remove(pos.GlobalCellId);
//            if (pos.Cell.Opaque)
//                continue;

//            pos.Chunk.SetBlockLight(pos, pos.Cell.Luminance);

//            foreach (var n in pos.Global.GetAdjacentLazy())
//            {
//                if (!this.Map.TryQueryPosition(n, out var npos))
//                    continue;
//                var nlight = npos.Chunk.GetBlockLight(npos.CellIndex);

//                if (nlight > 0 && !npos.Cell.Opaque)
//                {
//                    if (!this.DarkenQueued.Contains(npos.GlobalCellId))
//                    {
//                        this.DarkenQueue.Enqueue(npos);
//                        this.DarkenQueued.Add(npos.GlobalCellId);
//                    }
//                    else
//                    {
//                        if (!this.Queued.Contains(npos.GlobalCellId))
//                        {
//                            this.Queue.Enqueue(npos);
//                            this.Queued.Add(npos.GlobalCellId);
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
