using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Rendering;
using Project1.Core.Simulation;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI.Behaviors.Pathing;

public sealed class RegionManager
{
    int RoomIDSequence = 0;
    internal int GetRoomID()
    {
        return RoomIDSequence++;
    }
    int RegionIDSequence = 0;
    internal int GetRegionID()
    {
        return RegionIDSequence++;
    }

    readonly Dictionary<Chunk, RegionTracker> Regions = new();
    readonly Dictionary<int, RegionRoom> Rooms = new();
    readonly Queue<Vector3> ChangedBlocksFromLastFrame = new();
    readonly MapBase Map;
    public RegionManager(MapBase map)
    {
        this.Map = map;
        this.Map.Events.ListenTo<CellsInvalidatedEvent>(this.HandleBlocksChanged);
    }

    internal void Init()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        foreach (var chunk in this.Map.ActiveChunks.Values)
        {
            var tracker = new RegionTracker(chunk);
            this.Regions[chunk] = tracker;
            tracker.Init();
        }
        foreach (var tracker in this.Regions.Values)
            tracker.Update();
        foreach (var tracker in this.Regions.Values)
            tracker.Refresh();
        foreach (var room in this.Rooms.Values)
            room.Init();
        watch.Stop();
        $"regions initialized in {watch.ElapsedMilliseconds} ms".ToConsole();
    }

    internal void Draw(IntVec3 global, MySpriteBatch sb, Renderer renderer, Camera camera)
    {
        var region = this.GetRegionAt(global);
        if (region is null)
            return;
        foreach (var ch in this.Regions)
        {
            foreach (var r in ch.Value.Regions.Where(r => r.RoomID == region.RoomID))
            {
                var color = region.Color * (r == region ? 1 : .5f);
                renderer.DrawGridCells(sb, color, r.GetPositions());
            }
        }
        region.DrawNode(global, sb, renderer, camera);
    }
    public RegionTracker GetRegions(Chunk chunk)
    {
        return this.Regions[chunk];
    }
    internal RegionNode GetNodeAt(Vector3 global)
    {
        var chunk = this.Map.GetChunk(global);
        if (chunk == null)
            return null;
        return this.Regions[chunk].GetNodeAt(global);
    }
    public Region GetRegionAt(Vector3 global)
    {
        var chunk = this.Map.GetChunk(global);
        if (this.Regions.TryGetValue(chunk, out RegionTracker regions))
            return regions.GetRegionAt(global);
        return null;
    }

    void HandleBlocksChanged(CellsInvalidatedEvent e)
    {
        if(e.Map == this.Map)
            this.Update(e.Positions);
    }

    private void Update(IEnumerable<IntVec3> positions)
    {
        foreach (var pos in positions)
        {
            var chunk = this.Map.GetChunk(pos);
            this.Regions[chunk].Handle(pos);
        }
        var affectedRooms = new HashSet<RegionRoom>();
        foreach (var pos in positions)
        {
            var node = this.GetNodeAt(pos);
            if (node == null)
                continue;
            affectedRooms.Add(node.Region.Room);
            foreach (var link in node.GetLinksLazy())
                affectedRooms.Add(link.Region.Room);
        }
        foreach (var room in affectedRooms)
            room.Init();
    }

    internal void Update()
    {
        while(this.ChangedBlocksFromLastFrame.Any())
        {
            var global = this.ChangedBlocksFromLastFrame.Dequeue();
            var chunk = this.Map.GetChunk(global);
            this.Regions[chunk].Handle(global);
        }
    }

    internal void Add(Region newRegion)
    {
        this.Regions[newRegion.Chunk].Add(newRegion);
    }

    public bool CanReach(GameObject entity, Vector3 target)
    {
        var startNode = this.GetNodeAt(entity.Global.Below().ToCell());
        if (startNode == null)
            throw new Exception();
        var currentRegion = startNode.Region;
        var targetNode = this.GetNodeAt(target);
        if (targetNode != null && (targetNode.Region == currentRegion || targetNode.Region.RoomID == currentRegion.RoomID))
            return true;
        foreach (var node in this.GetPotentialNodesAroundDestination((int)entity.Physics.Height, target))
        {
            if (node.Region.RoomID == startNode.Region.RoomID)
                return true;
        }
        return false;
    }
    public IEnumerable<Region> GetAdjacentRegions(int height, Vector3 target)
    {
        HashSet<Region> adjRegions = new HashSet<Region>();
        var targetNode = this.GetNodeAt(target);
        if (targetNode != null)
            adjRegions.Add(targetNode.Region);

        foreach (var node in this.GetPotentialNodesAroundDestination(height, target))
            adjRegions.Add(node.Region);

        return adjRegions;
    }
    public IEnumerable<IntVec3> GetSurroundingNodesOffset(IntVec3 global, int reach)
    {
        HashSet<RegionNode> handled = [];
        Queue<RegionNode> toHandle = [];
        toHandle.Enqueue(this.GetNodeAt(global));
        for (int i = 0; i < reach; i++)
        {
            if (toHandle.Count == 0)
                break;
            var current = toHandle.Dequeue();
            handled.Add(current);
            foreach (var adj in current.Adjacent)
                if (adj is RegionNode node)
                {
                    if (!handled.Contains(node))
                        toHandle.Enqueue(node);
                    yield return node.Global - global;
                }
        }
    }
    public IEnumerable<RegionNode> GetPotentialNodesAroundDestination(int reach, Vector3 global)
    {
        var n = global.North();
        var s = global.South();
        var w = global.West();
        var e = global.East();
        for (int i = -reach; i < 1; i++)
        {
            var nn = n + new Vector3(0, 0, i);
            var ss = s + new Vector3(0, 0, i);
            var ww = w + new Vector3(0, 0, i);
            var ee = e + new Vector3(0, 0, i);
            var nnode = this.GetNodeAt(nn);
            var snode = this.GetNodeAt(ss);
            var wnode = this.GetNodeAt(ww);
            var enode = this.GetNodeAt(ee);
            if (nnode != null)
                yield return nnode;
            if (snode != null)
                yield return snode;
            if (wnode != null)
                yield return wnode;
            if (enode != null)
                yield return enode;
        }
        var belowNode = this.GetNodeAt(global + new Vector3(0, 0, -reach));
        if (belowNode != null)
            yield return belowNode;
    }

    int GetRegionDistance(Region source, Region target)
    {
        if (source == target)
            return 0;
        var all = source.GetConnected();
        var dist = new Dictionary<Region, int>();
        var prev = new Dictionary<Region, Region>();
        var toHandle = new HashSet<Region>();
        foreach (var reg in all)
        {
            dist[reg] = int.MaxValue;
            prev[reg] = null;
            toHandle.Add(reg);
        }
        dist[source] = 0;
        while (toHandle.Any())
        {
            var ordered = toHandle.OrderBy(r => dist[r]);
            var current = ordered.First();
            toHandle.Remove(current);
            if (current == target)
            {
                return dist[current];
            }
            foreach (var neigh in current.Neighbors.Where(toHandle.Contains))
            {
                var d = dist[current] + 1;
                if (d < dist[neigh])
                {
                    dist[neigh] = d;
                    prev[neigh] = current;
                }
            }
        }
        throw new Exception(); // regions weren't connected!
    }

    internal int GetRegionDistance(Vector3 source, Vector3 target, Actor actor)
    {
        var reach = actor.Physics.Reach;
        // because the vector might have been returned by gameobject.standingon() which returns vector3(int.minvalue) and so we treat the negative z as an invalid position
        // might have to convert it to a nullable vector at some point
        if (source.Z < 0)
            return -1; 
        var sourceReg = this.GetRegionAt(source);
        if (sourceReg == null)
            return -1;
        var adjRegions = this.GetAdjacentRegions(reach, target)
            .Where(r => this.AreConnected(sourceReg, r));
        if (!adjRegions.Any())
            return -1;
        int minDist = int.MaxValue;
        minDist = adjRegions.Min(reg => this.GetRegionDistance(sourceReg, reg));
        return minDist;
    }
    internal bool CanReach(IntVec3 start, IntVec3 goal, Actor actor)
    {
        var reachHeight = actor.Physics.Reach;
        var sourceReg = this.GetRegionAt(start);
        var adjRegions = this.GetAdjacentRegions(reachHeight, goal);
        return adjRegions.Any(r => r.Room == sourceReg.Room);
    }
    internal bool AreConnected(Region reg1, Region reg2) // TODO can't just check if roomID of both regions is the same?
    {
        var toCheck = new Queue<Region>();
        toCheck.Enqueue(reg1);
        var handled = new HashSet<Region>() { reg1 };
        while(toCheck.Count != 0)
        {
            var currentRegion = toCheck.Dequeue();
            if (currentRegion == reg2)
                return true;
            foreach(var n in currentRegion.Neighbors)
            {
                if(!handled.Contains(n))
                {
                    toCheck.Enqueue(n);
                    handled.Add(n);
                }
            }
        }
        return false;
    }

    internal RegionRoom CreateRoom(Region region)
    {
        var room = new RegionRoom(this);
        this.Rooms[room.ID] = room;
        room.Add(region);
        return room;
    }

    internal RegionRoom GetRoomID(int id)
    {
        return this.Rooms[id];
    }

    internal void RemoveRoom(int roomID)
    {
        this.Rooms.Remove(roomID);
    }
}
