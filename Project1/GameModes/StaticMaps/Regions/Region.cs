using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class Region : Inspectable
    {
        public override string Label => nameof(Region);
        internal Chunk Chunk;
        MapBase Map => this.Chunk.Map;
        internal int RoomID
        {
            get => this.Room.ID;
            set
            {
                if (value == this.Room.ID)
                    return;
                this.Room.Remove(this);
                var newRoom = this.Map.Regions.GetRoomID(value);
                newRoom.Add(this);
                this.Room = newRoom;
            }
        }
        public int RegionID;
        internal RegionNodeCollection Nodes;
        public int Count => this.Nodes.Count;
        internal Color Color => this.Room.Color;
        int Size = 1;
        public HashSet<Region> Neighbors = new();
        bool Validated;
        public RegionRoom Room;
        internal Region(Chunk chunk)
        {
            this.Nodes = new RegionNodeCollection(this);
            this.Chunk = chunk;
            this.Room = chunk.Map.Regions.CreateRoom(this);
            this.RegionID = chunk.Map.Regions.GetRegionID();
        }

        public override string ToString()
        {
            return $"Region: {this.RegionID}\nRoom: {this.RoomID}\nNodes: {this.Nodes.Count}\nNeighbors: {this.Neighbors.Count}";
        }

        internal RegionNode Add(Vector3 local)
        {
            var node = new RegionNode(this, local);
            this.Nodes.Add(node);
            return node;
        }
        internal void Add(RegionNode node)
        {
            this.Nodes.Add(node);
        }
        void Add(IEnumerable<RegionNode> nodes)
        {
            foreach (var n in nodes)
                this.Add(n);
        }

        void Remove(IntVec3 global)
        {
            if(this.Nodes.TryGetValue(global, out var node))
                node.ClearLinks();
            this.Nodes.Remove(global);
        }
        internal void Remove(RegionNode n)
        {
            this.Remove(n.Global);
        }
        internal IEnumerable<IntVec3> GetPositions()
        {
            return this.Nodes.Values.Select(n => n.Global);
        }
        internal bool Contains(IntVec3 global)
        {
            return this.Nodes.ContainsKey(global);
        }
        internal bool Contains(RegionNode newNode)
        {
            return this.Nodes.ContainsKey(newNode.Global);
        }

        internal void LinkNodes()
        {
            var toHandle = new Queue<RegionNode>(this.Nodes.Values);
            var handled = new HashSet<IntVec3>();
            while (toHandle.Any())
            {
                var node = toHandle.Dequeue();
                handled.Add(node.Local);

                if (node.North is null)
                {
                    var northColumn = node.Global.North;
                    foreach (var north in RegionTracker.GetColumn(northColumn))
                        if (this.Nodes.ContainsKey(north))
                        {
                            var nNode = this.Nodes[north];
                            node.North = nNode;
                            nNode.South = node;
                            if (!handled.Contains(north))
                                toHandle.Enqueue(nNode);
                        }
                }
                if (node.South is null)
                {
                    var southolumn = node.Global.South;
                    foreach (var south in RegionTracker.GetColumn(southolumn))
                        if (this.Nodes.ContainsKey(south))
                        {
                            var sNode = this.Nodes[south];
                            node.South = sNode;
                            sNode.North = node;
                            if (!handled.Contains(south))
                                toHandle.Enqueue(sNode);
                        }
                }
                if (node.West is null)
                {
                    var westColumn = node.Global.West;
                    foreach (var west in RegionTracker.GetColumn(westColumn))
                        if (this.Nodes.ContainsKey(west))
                        {
                            var wNode = this.Nodes[west];
                            node.West = wNode;
                            wNode.East = node;
                            if (!handled.Contains(west))
                                toHandle.Enqueue(wNode);
                        }
                }
                if (node.East is null)
                {
                    var eastColumn = node.Global.East;
                    foreach (var east in RegionTracker.GetColumn(eastColumn))
                        if (this.Nodes.ContainsKey(east))
                        {
                            var eNode = this.Nodes[east];
                            node.East = eNode;
                            eNode.West = node;
                            if (!handled.Contains(east))
                                toHandle.Enqueue(eNode);
                        }
                }
            }
        }
        internal void FindEdges()
        {
            foreach (var node in this.Nodes.Values)
            {
                // if one of node's neighbors is null, check all other regions for a neighbor
                if (node.West is null)
                {
                    var adj = node.Global.West;
                    if (this.Map.IsInBounds(adj))
                    {
                        for (int i = 0; i < VectorHelper.Column3.Length; i++)
                        {
                            var pos = adj + VectorHelper.Column3[i];
                            var otherNode = this.Map.Regions.GetNodeAt(pos);
                            if (otherNode != null
                                && this.Map.IsTraversable(node.Global, pos)
                                )
                            {
                                node.West = otherNode;
                                otherNode.East = node;
                                if (otherNode.Region != this)
                                {
                                    this.Neighbors.Add(otherNode.Region);
                                    otherNode.Region.Neighbors.Add(this);
                                }
                            }
                        }
                    }
                }
                if (node.East is null)
                {
                    var adj = node.Global.East;
                    if (this.Map.IsInBounds(adj))
                    {
                        for (int i = 0; i < VectorHelper.Column3.Length; i++)
                        {
                            var pos = adj + VectorHelper.Column3[i];
                            var otherNode = this.Map.Regions.GetNodeAt(pos);
                            if (otherNode != null
                                && this.Map.IsTraversable(node.Global, pos)
                                )
                            {
                                node.East = otherNode;
                                otherNode.West = node;
                                if (otherNode.Region != this)
                                {
                                    this.Neighbors.Add(otherNode.Region);

                                    otherNode.Region.Neighbors.Add(this);
                                }
                            }
                        }
                    }
                }
                if (node.North is null)
                {
                    var adj = node.Global.North;
                    if (this.Map.IsInBounds(adj))
                    {
                        for (int i = 0; i < VectorHelper.Column3.Length; i++)
                        {
                            var pos = adj + VectorHelper.Column3[i];
                            var otherNode = this.Map.Regions.GetNodeAt(pos);
                            if (otherNode != null
                                && this.Map.IsTraversable(node.Global, pos)
                                )
                            {
                                node.North = otherNode;
                                otherNode.South = node;
                                if (otherNode.Region != this)
                                {
                                    this.Neighbors.Add(otherNode.Region);
                                    otherNode.Region.Neighbors.Add(this);
                                }
                            }
                        }
                    }
                }
                if (node.South is null)
                {
                    var adj = node.Global.South;
                    if (this.Map.IsInBounds(adj))
                    {
                        for (int i = 0; i < VectorHelper.Column3.Length; i++)
                        {
                            var pos = adj + VectorHelper.Column3[i];
                            var otherNode = this.Map.Regions.GetNodeAt(pos);
                            if (otherNode != null
                                && this.Map.IsTraversable(node.Global, pos)
                                )
                            {
                                node.South = otherNode;
                                otherNode.North = node;
                                if (otherNode.Region != this)
                                {
                                    this.Neighbors.Add(otherNode.Region);
                                    otherNode.Region.Neighbors.Add(this);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void Paint(Region sourceRegion)
        {
            foreach (var r in this.GetConnected())
            {
                r.RoomID = sourceRegion.RoomID;
            }
        }
        internal IEnumerable<Region> GetConnected()
        {
            var queue = new Queue<Region>();
            foreach (var n in this.Neighbors)
                queue.Enqueue(n);
            var counted = new HashSet<Region>() { this };
            while (queue.Any())
            {
                var r = queue.Dequeue();
                counted.Add(r);
                foreach (var n in r.Neighbors)
                {
                    if (!counted.Contains(n))
                        queue.Enqueue(n);
                }
            }
            return counted;
        }
        internal void Refresh()
        {
            if (this.Validated)
                return;
            this.Validated = true;
            this.Size = 1;
            if (this.Nodes.Count == 1 && this.Map.GetBlock(this.Nodes.First().Key.Above()) is BlockDoor) // TODO: FIX LOL
                return;
            var queue = new Queue<Region>();
            foreach (var n in this.Neighbors)
                queue.Enqueue(n);
            var counted = new HashSet<Region>() { this };
            while (queue.Any())
            {
                var r = queue.Dequeue();
                r.RoomID = this.RoomID;
                r.Validated = true;
                counted.Add(r);
                this.Size++;
                foreach (var n in r.Neighbors)
                {
                    if (n.Nodes.Count == 1 && this.Map.GetBlock(n.Nodes.First().Key.Above()) is BlockDoor) // TODO: FIX LOL
                        continue;
                    if (!counted.Contains(n))
                        queue.Enqueue(n);
                }
            }
            foreach (var r in counted)
                r.Size = this.Size;
        }

        internal void DrawNode(IntVec3 global, MySpriteBatch sb, Camera cam)
        {
            cam.DrawGridCell(sb, Color.Lime, global);
            var node = this.Nodes[global];//.ToLocal()];
            cam.DrawGridCells(sb, Color.Lime, node.GetLinks().Select(link => link.Global));
        }

        internal void Add(Region r)
        {
            foreach(var node in r.Nodes.Values.ToList())
            {
                this.Add(node);
            }
            r.Nodes.Clear();
            foreach (var nRegion in r.Neighbors) 
                // foreach otherregion's neighbors
                // remove otherregion and add this region
                // and add this region
            {
                nRegion.Neighbors.Remove(r);
                nRegion.Neighbors.Add(this);
                this.Neighbors.Add(nRegion);
            }
        }
        internal void Delete()
        {
            this.Room.Remove(this);
            this.Map.Regions.GetRegions(this.Chunk).Regions.Remove(this);
        }

        bool TryGetSubRegionNodesNew(RegionNode startNode, IEnumerable<HashSet<RegionNode>> existing, out HashSet<RegionNode> nodes, out HashSet<Region> neighbors, out int size)
        {
            var toCount = new Queue<RegionNode>();
            var toCountAdded = new HashSet<RegionNode>();
            var counted = new HashSet<RegionNode>();
            toCount.Enqueue(startNode);
            var n = 0;
            HashSet<Region> neighborsFound = new HashSet<Region>();
            while (toCount.Any())
            {
                n++;
                var node = toCount.Dequeue();
                if (existing.Any(e => e.Contains(node)))
                {
                    nodes = null;
                    neighbors = null;
                    size = -1;
                    return false;
                }
                counted.Add(node);
                foreach (var link in node.GetLinks())
                {
                    if (counted.Contains(link) || toCountAdded.Contains(link)) // TODO: optimize
                        continue;
                    if (link.Region != this)
                    {
                        if (link.Region.RoomID == this.RoomID)
                            neighborsFound.Add(link.Region);
                        continue;
                    }
                    toCount.Enqueue(link);
                    toCountAdded.Add(link);
                }
            }
            neighbors = neighborsFound;
            size = n;
            nodes = counted;
            return n < this.Nodes.Count;
        }

        internal void TrySplitRegion(IntVec3 split)
        {
            var node = this.Nodes.GetValueOrDefault(split);
            if (node is null)
                return;
            this.TrySplitRegion(node);
        }
        internal void TrySplitRegion(RegionNode split) // with door handling
        {
            var isBelowDoor = this.Map.GetBlock(split.Global.Above) is BlockDoor;

            Region doorRegion = null;
            var links = split.GetLinks();
            if (isBelowDoor)
            {
                doorRegion = new Region(this.Chunk);
                doorRegion.Add(split);
                MakeNeighborsNoPaint(this, doorRegion); // HOW WERE DOORS WORKING BEFORE WITHOUT THIS LINE???
                this.Map.Regions.Add(doorRegion);
            }
            else
                split.ClearLinks();

            this.Nodes.Remove(split.Global);

            var subRegions = new Dictionary<HashSet<RegionNode>, HashSet<Region>>();
            var maxSize = 0;
            IEnumerable<RegionNode> biggest = null;
            foreach (var link in links.Where(l => l.Chunk == this.Chunk))
            {
                if (!this.TryGetSubRegionNodesNew(link, subRegions.Keys, out var nodes, out var neighbors, out var size))
                    continue;
                if (size > maxSize)
                {
                    maxSize = size;
                    biggest = nodes;
                }
                subRegions[nodes] = neighbors;
            }
            if (!subRegions.Any())
                return;
            foreach (var nodesPair in subRegions)
            {
                var nodes = nodesPair.Key;
                var neighbors = nodesPair.Value;
                if (nodes == biggest)
                    continue;
                var newRegion = new Region(this.Chunk);
                newRegion.Add(nodes);
                foreach (var n in nodes)
                    this.Nodes.Remove(n.Global);
                foreach (var neigh in neighbors)
                {
                    UnLink(this, neigh);
                    MakeNeighborsAndPaint(newRegion, neigh);
                }
                this.Map.Regions.Add(newRegion);
                if (isBelowDoor)
                    MakeNeighborsNoPaint(newRegion, doorRegion);
            }
        }
        static void MakeNeighborsNoPaint(Region master, Region slave)
        {
            master.Neighbors.Add(slave);
            slave.Neighbors.Add(master);
        }
        static void MakeNeighborsAndPaint(Region master, Region slave)
        {
            slave.Paint(master);
            master.Neighbors.Add(slave);
            slave.Neighbors.Add(master);
        }
        static void UnLink(Region region1, Region region2)
        {
            region1.Neighbors.Remove(region2);
            region2.Neighbors.Remove(region1);
        }

        internal bool IsOutdoors()
        {
            foreach(var node in this.Nodes)
            {
                var heightmapvalue = this.Chunk.GetHeightMapValue(node.Value.Local);
                if (node.Key.Z == heightmapvalue)
                    return true;
            }
            return false;
        }

        internal void Write(BinaryWriter w)
        {
            this.Nodes.Write(w);
        }
    }
}
