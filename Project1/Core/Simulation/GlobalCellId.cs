using Project1.Framework;
using System;

namespace Project1.Core.Simulation;

public readonly struct GlobalCellId : IEquatable<GlobalCellId>
{
    private readonly int packed;

    /// <summary>
    /// Pack coordinates into a single int: x/y = 12 bits each, z = 8 bits
    /// Limits: x ∈ [0, 4095], y ∈ [0, 4095], z ∈ [0, 255]
    /// </summary>
    public GlobalCellId(int x, int y, int z)
    {
        packed = ((x & 0xFFF) << 20) | ((y & 0xFFF) << 8) | (z & 0xFF);
    }

    public GlobalCellId(IntVec3 global) : this(global.X, global.Y, global.Z) { }

    public int X => (packed >> 20) & 0xFFF;
    public int Y => (packed >> 8) & 0xFFF;
    public int Z => packed & 0xFF;

    public static implicit operator int(GlobalCellId id) => id.packed;
    public static implicit operator GlobalCellId(IntVec3 global) => new(global.X, global.Y, global.Z);
    public static implicit operator IntVec3(GlobalCellId id) => new(id.X, id.Y, id.Z);


    public bool Equals(GlobalCellId other) => packed == other.packed;
    public override bool Equals(object? obj) => obj is GlobalCellId other && Equals(other);
    public override int GetHashCode() => packed;

    public override string ToString() => $"({X},{Y},{Z})";
}
//public sealed class LightingEngine(MapBase map)
//{
//    readonly MapBase Map = map;

//    HashSet<IntVec3> Queued = [];
//    Queue<IntVec3> Queue = [];

//    readonly Queue<IntVec3> DarkenQueue = new();
//    readonly HashSet<IntVec3> DarkenQueued = new();

//    void Refresh(IEnumerable<IntVec3> vectors)
//    {
//        this.Queued.Clear();// = new();
//        this.Queue.Clear();// = new();

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
//            HandleSkyGlobalImmediate(this.Queue.Dequeue());

//        this.Refresh(vectors);
//        while (this.Queue.Count > 0)
//            HandleBlockGlobalImmediate(this.Queue.Dequeue());
//    }
//    void HandleSkyGlobalImmediate(IntVec3 global)
//    {
//        if (!this.Map.TryQueryPosition(global, out var pos))
//            return;

//        var neighbors = global.GetAdjacentLazy();
//        var nextLight = GetNextSunLightImmediate(pos, neighbors);

//        var oldLight = pos.Chunk.GetSkylight(pos.CellIndex);
//        int d = nextLight - oldLight;
//        if (d != 0)
//            pos.Chunk.SetSkylight(pos, nextLight);

//        if (d > 1)
//        {
//            foreach (var n in neighbors)
//                if (!Queued.Contains(n))
//                {
//                    Queue.Enqueue(n);
//                    Queued.Add(n);
//                }
//        }
//        else if (d < -1)
//        {
//            DarkenImmediateWorking(global);
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
//    void DarkenImmediateWorking(IntVec3 global)
//    {
//        this.DarkenQueue.Clear();
//        this.DarkenQueued.Clear();
//        this.DarkenQueue.Enqueue(global);
//        this.DarkenQueued.Add(global);
//        while (this.DarkenQueue.Count > 0)
//        {
//            var currentGlobal = this.DarkenQueue.Dequeue();
//            byte nlight;
//            if (!this.Map.TryQueryPosition(currentGlobal, out var pos))
//                continue;
//            if (pos.Chunk.IsAboveHeightMap(currentGlobal))
//                continue;
//            pos.Chunk.SetSkylight(pos, 0);

//            var neighbors = currentGlobal.GetAdjacentLazy();
//            foreach (var n in neighbors)
//            {
//                if (!this.Map.TryQueryPosition(n, out var nquery))
//                    continue;
//                if (nquery.Chunk.IsAboveHeightMap(n))
//                {
//                    if (!this.Queued.Contains(currentGlobal))
//                    {
//                        this.Queue.Enqueue(currentGlobal);
//                        this.Queued.Add(currentGlobal);
//                    }
//                    continue;
//                }
//                nlight = nquery.Chunk.GetSkylight(nquery.CellIndex);

//                if (!nquery.Cell.Opaque)
//                    if (!this.DarkenQueued.Contains(n))
//                    {
//                        this.DarkenQueue.Enqueue(n);
//                        this.DarkenQueued.Add(n);
//                    }
//            }
//        }
//    }
//    private void HandleBlockGlobalImmediate(IntVec3 global)
//    {
//        this.Queued.Remove(global);
//        if (!this.Map.TryQueryPosition(global, out var pos))
//            return;
//        var thisLight = pos.Chunk.GetBlockLight(pos.CellIndex);

//        var nextLight = this.GetNextBlockLightImmediate(pos);
//        pos.Chunk.SetBlockLight(pos, nextLight);

//        if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
//        {
//            foreach (var n in global.GetAdjacentLazy())
//            {
//                if (!this.Queued.Contains(n))
//                {
//                    this.Queue.Enqueue(n);
//                    this.Queued.Add(n);
//                }
//            }
//        }

//        else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
//        {
//            DarkenBlockImmediateWorking(global);
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
//    void DarkenBlockImmediateWorking(IntVec3 global)
//    {
//        this.DarkenQueue.Clear();
//        this.DarkenQueued.Clear();
//        this.DarkenQueue.Enqueue(global);
//        this.DarkenQueued.Add(global);
//        while (DarkenQueue.Count > 0)
//        {
//            var current = this.DarkenQueue.Dequeue();
//            this.DarkenQueued.Remove(current);
//            if (!this.Map.TryQueryPosition(current, out var pos))
//                continue;
//            if (pos.Cell.Opaque)
//                continue;

//            pos.Chunk.SetBlockLight(pos, pos.Cell.Luminance);

//            foreach(var n in current.GetAdjacentLazy())
//            {
//                if (!this.Map.TryQueryPosition(n, out var npos))
//                    continue;
//                var nlight = npos.Chunk.GetBlockLight(npos.CellIndex);

//                if (nlight > 0 && !npos.Cell.Opaque)
//                {
//                    if (!this.DarkenQueued.Contains(n))
//                    {
//                        this.DarkenQueue.Enqueue(n);
//                        this.DarkenQueued.Add(n);
//                    }
//                    else
//                    {
//                        if (!this.Queued.Contains(n))
//                        {
//                            this.Queue.Enqueue(n);
//                            this.Queued.Add(n);
//                        }
//                    }
//                }
//            }
//        }
//    }
//}

