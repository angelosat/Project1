using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class RegionTracker
    {
        MapBase Map => this.Chunk.Map;
        readonly Chunk Chunk;
        public HashSet<Region> Regions = new();
        public RegionTracker(Chunk chunk)
        {
            this.Chunk = chunk;
        }
        
        public HashSet<Region> Init()
        {
            this.Regions.Clear();
            var maxz = this.Chunk.Map.GetMaxHeight() - 1;
            var handled = new HashSet<IntVec3>();
            var size = Chunk.Size;
            var h = 1; //2
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    bool foundSolid = false;
                    var lastSolidZ = 0;
                    for (int z = 0; z < maxz; z++)
                    {
                        var vec = new IntVec3(x, y, z);
                        var global = vec.ToGlobal(this.Chunk);
                        var cell = this.Chunk[x, y, z];
                        if (cell.IsSolid() && cell.Block is not BlockDoor)
                        {
                            foundSolid = true;
                            lastSolidZ = z;
                        }
                        else
                        {
                            var existing = this.GetRegionAt(global - new IntVec3(0, 0, h)); // why 2?
                            if (foundSolid && lastSolidZ == z - h && existing == null)
                            {
                                foreach (var region in this.FloodNew(handled, x, y, lastSolidZ))
                                {
                                    this.Regions.Add(region);
                                }
                                foundSolid = false;

                            }
                        }
                    }
                }
            return this.Regions;
        }
        
        internal void Update()
        {
            foreach (var r in this.Regions)
                r.FindEdges();
        }
        internal void Add(Region newRegion)
        {
            this.Regions.Add(newRegion);
        }

        IEnumerable<Region> FloodNew(HashSet<IntVec3> handled, int x, int y, int z)
        {
            var nextFlood = new Queue<IntVec3>();
            nextFlood.Enqueue(new IntVec3(x, y, z));
            while (nextFlood.Any())
            {
                var vec = nextFlood.Dequeue();
                var currentRegion = new Region(this.Chunk);
                var toHandle = new Queue<IntVec3>();
                toHandle.Enqueue(vec);
                handled.Add(vec);
                var adjZ = VectorHelper.Column3;
                while (toHandle.Any())
                {
                    var current = toHandle.Dequeue();
                    currentRegion.Add(current);
                    var adjsamez = VectorHelper.AdjacentXY;
                    for (int i = 0; i < adjsamez.Length; i++)
                    {
                        var adj = current + adjsamez[i];
                        if (!adj.IsWithinChunkBounds())
                            continue;
                        
                        for (int k = 0; k < adjZ.Length; k++)
                        {
                            var adjvert = adj + adjZ[k];
                            if (!adjvert.IsZWithinBounds()) // TODO: check only if z is out of bounds here
                                continue;
                            // TODO: check if already handled while building the ienumerables
                            if (handled.Contains(adjvert))
                                continue;
                            if (this.IsWalkableOn(adjvert) && this.IsTraversable(current.ToGlobal(this.Chunk), adjvert.ToGlobal(this.Chunk)))
                            {
                                handled.Add(adjvert);
                                if (this.IsNewRegion(adjvert))
                                    nextFlood.Enqueue(adjvert);
                                else
                                    toHandle.Enqueue(adjvert);
                                break; // if we found a walkable adj position, don't check other positions in same column
                            }
                        }
                    }
                }
                yield return currentRegion;
            }
        }
        
        bool IsNewRegion(Vector3 local)
        {
            var blockAbove = this.Chunk[local.Above()].Block;
            return blockAbove is BlockDoor;
        }
        Region Flood(HashSet<Vector3> handled, int x, int y, int z)
        {
            var region = new Region(this.Chunk);
            var vec = new Vector3(x, y, z);
            var toCheck = new Queue<Vector3>();
            toCheck.Enqueue(vec);
            handled.Add(vec);
            while (toCheck.Any())
            {
                var current = toCheck.Dequeue();
                region.Add(current);
                var adjsamez = GetAdjacentSameZ(current);
                foreach (var adj in adjsamez)
                {
                    if (!adj.IsWithinChunkBounds())
                        continue;
                    foreach (var adjvert in GetColumn(adj))
                    {
                        if (!adjvert.IsZWithinBounds()) // TODO: check only if z is out of bounds here
                            continue;
                        // TODO: check if already handled while building the ienumerables
                        if (handled.Contains(adjvert))
                            continue;
                        if (this.IsWalkableOn(adjvert))
                        {
                            handled.Add(adjvert);
                            toCheck.Enqueue(adjvert);
                            break; // if we found a walkable adj position, don't check other positions in same column
                        }
                    }
                }
            }
            return region;
        }
        bool IsWalkableOn(Vector3 global)
        {
            var pos = global.ToLocal();
            var above = pos.Above();
            
            var possolid = this.Chunk.IsSolid(pos);
            var abovesolid = this.Chunk.IsSolid(above);
            return possolid && !abovesolid;

        }
        static public IEnumerable<Vector3> GetAdjacentSameZ(Vector3 pos)
        {
            yield return pos.West();
            yield return pos.East();
            yield return pos.North();
            yield return pos.South();
        }
        static public IEnumerable<Vector3> GetColumn(Vector3 pos)
        {
            yield return pos.Below();
            yield return pos;
            yield return pos.Above();
        }
        
        internal Region GetRegionAt(IntVec3 global)
        {
            return this.Regions.FirstOrDefault(r => r.Contains(global));
        }
        internal RegionNode GetNodeAt(Vector3 global)
        {
            if (!this.Chunk.Contains(global)) //slow?
                return this.Map.Regions.GetNodeAt(global);
            foreach (var r in this.Regions)
            {
                var node = r.Nodes.GetValueOrDefault(global);
                if(node != null)
                    return node;
            }
            return null;
        }
        internal void Refresh()
        {
            foreach (var r in this.Regions)
                r.Refresh();
        }
        
        internal void Handle(Vector3 global)
        {
            var isSolid = this.Map.IsSolid(global);// TODO: dont fetch cell twice!!!
            var below = global.Below();
            var belowbelow = below.Below();

            RegionNode newNode = null;
            var cell = this.Map.GetCell(global);// TODO: dont fetch cell twice!!!
            var block = cell.Block;
            if (block is BlockDoor)
            {
                if (cell.Origin != IntVec3.Zero) // only handle the base of the door
                    return;
                var regionbelow = this.GetRegionAt(below);
                if (regionbelow != null)
                    regionbelow.TrySplitRegion(below);
                return;
            }
            else if (isSolid)
            {
                if (this.GetNodeAt(global) != null)
                    return; // existing solid block was replaced with another solid block so nothing changed region-wise
                var regionbelowbelow = this.GetRegionAt(belowbelow);
                var regionbelow = this.GetRegionAt(below);
                
                if (regionbelowbelow != null)
                    regionbelowbelow.TrySplitRegion(belowbelow);
                else if (regionbelow != null)
                    regionbelow.TrySplitRegion(below);

                if (!IsWalkableOn(global))
                    return;
                newNode = new RegionNode(new Region(this.Chunk), global.ToLocal());
                this.TryCreateNodeLinks(newNode);
            }
            else // block has been removed OR a nonsolid block has been added, like a designation
            {
                /// WRONG!!!! I DO RAISE EVENTS
                //if (block == Block.Designation) // this isn't needed because i don't raise blockchanged events when placing designations  /// WRONG!!!! I DO RAISE EVENTS
                /// WRONG!!!! I DO RAISE EVENTS
                //    return; // TODO: surely this is wrong 


                var deletedNode = GetNodeAt(global);
                if (deletedNode != null)
                {
                    deletedNode.Region.TrySplitRegion(deletedNode);
                }

                if (IsWalkableOn(below))
                {
                    if (this.GetNodeAt(below) != null)
                        return;
                    newNode = this.InsertNewNode(new Region(this.Chunk), below.ToLocal());//
                }
                else if (IsWalkableOn(belowbelow))
                {
                    if (this.GetNodeAt(belowbelow) != null)
                        return;
                    newNode = this.InsertNewNode(new Region(this.Chunk), belowbelow.ToLocal());//
                }
                else
                {
                    var walkable = IsWalkableOn(belowbelow);
                    var nodebelow2 = this.GetNodeAt(belowbelow);
                    if (walkable && nodebelow2 != null)
                    {
                        "trying to connect existing belowbelowbelow node".ToConsole();
                        
                        nodebelow2.ClearLinks();
                        this.TryCreateNodeLinks(nodebelow2);
                        this.TryLinkAdjacentRegions(nodebelow2);
                        return;
                    }
                }
            }
            if (newNode == null)
                return;
            this.TryLinkAdjacentRegions(newNode);

            if (!newNode.Region.Contains(newNode))
            {
                newNode.Region.Add(newNode);
            }
            if (!this.Regions.Contains(newNode.Region)) // no idea why i had removed this from here but regions weren't getting added when removing a batch blocks containing the edges of neighboring chunks
                this.Regions.Add(newNode.Region);
        }

        void TryLinkAdjacentRegions(RegionNode newNode)
        {
            var adjRegions = new HashSet<Region>();
            foreach (var link in newNode.GetLinks())
            {
                adjRegions.Add(link.Region);
            }
            if (adjRegions.Count == 1)
            {
                var reg = adjRegions.First();
                if(newNode.Region != reg)
                {
                    newNode.Region.Remove(newNode);
                    reg.Add(newNode);
                }
                return;
            }
            Region biggestLocalAdjRegion = null;
            int biggestLocalAdjRegionSize = 0;
            int biggestGlobalAdjRegionSize = 0;
            Region biggestRegion = null;
            int biggestRegionSize = 0;
            foreach (var r in adjRegions)
            {
                var size = r.Nodes.Count;
                if (r.Chunk != this.Chunk)
                {
                    if (size > biggestGlobalAdjRegionSize)
                    {
                        biggestGlobalAdjRegionSize = size;
                    }
                }
                else
                {
                    if (size > biggestLocalAdjRegionSize)
                    {
                        biggestLocalAdjRegion = r;
                        biggestLocalAdjRegionSize = size;
                    }
                }
                if (size > biggestRegionSize)
                {
                    biggestRegion = r;
                    biggestRegionSize = size;
                }
            }

            // first merge local regions
            if (biggestLocalAdjRegion == null)
            {
                biggestLocalAdjRegion = newNode.Region;
                if (biggestRegion != null)
                    biggestLocalAdjRegion.Paint(biggestRegion);
            }
            else
            {
                foreach (var r in adjRegions)
                {
                    if (r.Chunk != this.Chunk)
                        continue;
                    if (r == biggestLocalAdjRegion)
                        continue;
                    biggestLocalAdjRegion.Add(r);
                    r.Delete();
                }
            }
            // merge adj chunk regions with eachother and with local region
            foreach (var r in adjRegions)
            {
                if (r != biggestRegion)
                    r.Paint(biggestRegion);
                if (r.Chunk == this.Chunk)
                    continue;
                biggestLocalAdjRegion.Neighbors.Add(r);
                r.Neighbors.Add(biggestLocalAdjRegion);
            }
            if(!biggestLocalAdjRegion.Nodes.ContainsKey(newNode.Global))
                biggestLocalAdjRegion.Add(newNode);
        }
        
        RegionNode TryCreateNodeLinks(RegionNode newNode)
        {
            var global = newNode.Global;
            foreach (var adj in GetAdjacentSameZ(global))
                foreach (var adjvert in GetColumn(adj))
                {
                    var adjnode = this.GetNodeAt(adjvert);

                    if (adjnode == null)
                        continue;

                    if (!IsTraversable(newNode, adjnode))
                        continue;

                    if (adjnode.Global.X == global.X - 1) // west
                    {
                        newNode.West = adjnode;
                        adjnode.East = newNode;
                    }
                    else if (adjnode.Global.X == global.X + 1) // east
                    {
                        newNode.East = adjnode;
                        adjnode.West = newNode;
                    }
                    else if (adjnode.Global.Y == global.Y - 1) // north
                    {
                        newNode.North = adjnode;
                        adjnode.South = newNode;
                    }
                    else if (adjnode.Global.Y == global.Y + 1) // south
                    {
                        newNode.South = adjnode;
                        adjnode.North = newNode;
                    }
                    break;
                }
            return newNode;
        }
        bool IsTraversable(RegionNode source, RegionNode target)
        {
            return this.IsTraversable(source.Global, target.Global);
        }
        bool IsTraversable(Vector3 sourceGlobal, Vector3 targetGlobal)
        {
            return this.Map.IsTraversable(sourceGlobal, targetGlobal);
        }
        RegionNode InsertNewNode(Region region, Vector3 local)
        {
            var node = new RegionNode(region, local);
            this.TryCreateNodeLinks(node);
            return node;
        }

        [Obsolete]
        public HashSet<Region> InitNoDoors()
        {
            this.Regions.Clear();
            var maxz = this.Chunk.Map.GetMaxHeight() - 1;
            var handled = new HashSet<Vector3>();
            for (int x = 0; x < Chunk.Size; x++)
                for (int y = 0; y < Chunk.Size; y++)
                {
                    bool foundSolid = false;
                    var lastSolidZ = 0;
                    for (int z = 0; z < maxz; z++)
                    {
                        var vec = new Vector3(x, y, z);
                        var global = vec.ToGlobal(this.Chunk);
                        var cell = this.Chunk[x, y, z];
                        if (cell.IsSolid())
                        {
                            foundSolid = true;
                            lastSolidZ = z;
                        }
                        else
                        {
                            var existing = this.GetRegionAt(global - new Vector3(0, 0, 2));
                            if (foundSolid && lastSolidZ == z - 2 && existing == null)
                            {
                                var region = Flood(handled, x, y, lastSolidZ);
                                region.LinkNodes();
                                this.Regions.Add(region);
                                foundSolid = false;
                            }
                        }
                    }
                }
            return this.Regions;
        }
        [Obsolete]
        internal void Handle2Height(Vector3 global)
        {
            var isSolid = this.Map.IsSolid(global);// TODO: dont fetch cell twice!!!
            var below = global.Below();
            var belowbelow = below.Below();
            var belowbelowbelow = belowbelow.Below();

            RegionNode newNode = null;
            var cell = this.Map.GetCell(global);// TODO: dont fetch cell twice!!!
            var block = cell.Block;
            if (block is BlockDoor)
            {
                var part = BlockDoor.GetPart(cell.BlockData);
                if (part != 0)
                    return;
                var regionbelow = this.GetRegionAt(below);
                if (regionbelow != null)
                    regionbelow.TrySplitRegion(below);
                return;
            }
            else if (isSolid)
            {
                if (this.GetNodeAt(global) != null)
                    return; // existing solid block was replaced with another solid block so nothing changed region-wise
                var regionbelowbelowbelow = this.GetRegionAt(belowbelowbelow);
                var regionbelowbelow = this.GetRegionAt(belowbelow);
                var regionbelow = this.GetRegionAt(below);
                if (regionbelowbelowbelow != null)
                {
                    var nodebbb = GetNodeAt(belowbelowbelow);
                    if (nodebbb != null)
                    {
                        regionbelowbelowbelow.TrySplitRegion(belowbelowbelow);
                        newNode = new RegionNode(new Region(this.Chunk), belowbelowbelow.ToLocal());
                        this.TryCreateNodeLinks(newNode);
                        this.TryLinkAdjacentRegions(newNode);
                        return;
                    }
                }
                else
                if (regionbelowbelow != null)
                    regionbelowbelow.TrySplitRegion(belowbelow);
                else if (regionbelow != null)
                    regionbelow.TrySplitRegion(below);

                if (!IsWalkableOn(global))
                    return;
                newNode = new RegionNode(new Region(this.Chunk), global.ToLocal());
                this.TryCreateNodeLinks(newNode);
            }
            else // block has been removed OR a nonsolid block has been added, like a designation
            {
                /// WRONG!!!! I DO RAISE EVENTS
                //if (block == Block.Designation) // this isn't needed because i don't raise blockchanged events when placing designations  /// WRONG!!!! I DO RAISE EVENTS
                /// WRONG!!!! I DO RAISE EVENTS
                //    return; // TODO: surely this is wrong 

                var deletedNode = GetNodeAt(global);
                if (deletedNode != null)
                {
                    deletedNode.Region.TrySplitRegion(deletedNode);
                }

                if (IsWalkableOn(below))
                {
                    if (this.GetNodeAt(below) != null)
                        return;
                    newNode = this.InsertNewNode(new Region(this.Chunk), below.ToLocal());//
                }
                else if (IsWalkableOn(belowbelow))
                {
                    if (this.GetNodeAt(belowbelow) != null)
                        return;
                    newNode = this.InsertNewNode(new Region(this.Chunk), belowbelow.ToLocal());//
                }
                else
                {
                    var walkable = IsWalkableOn(belowbelowbelow);
                    var nodebelow3 = this.GetNodeAt(belowbelowbelow);
                    if (walkable && nodebelow3 != null)
                    {
                        "trying to connect existing belowbelowbelow node".ToConsole();
                        nodebelow3.ClearLinks();
                        this.TryCreateNodeLinks(nodebelow3);
                        this.TryLinkAdjacentRegions(nodebelow3);
                        return;
                    }
                }
            }
            if (newNode == null)
                return;
            this.TryLinkAdjacentRegions(newNode);

            if (!newNode.Region.Contains(newNode))
            {
                newNode.Region.Add(newNode);
            }
            if (!this.Regions.Contains(newNode.Region)) // no idea why i had removed this from here but regions weren't getting added when removing a batch blocks containing the edges of neighboring chunks
                this.Regions.Add(newNode.Region);
        }
        [Obsolete]
        public HashSet<Region> Init2Height()
        {
            this.Regions.Clear();
            var maxz = this.Chunk.Map.GetMaxHeight() - 1;
            var handled = new HashSet<IntVec3>();
            var size = Chunk.Size;
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    bool foundSolid = false;
                    var lastSolidZ = 0;
                    for (int z = 0; z < maxz; z++)
                    {
                        var vec = new IntVec3(x, y, z);
                        var global = vec.ToGlobal(this.Chunk);
                        var cell = this.Chunk[x, y, z];
                        if (cell.IsSolid() && cell.Block is not BlockDoor)
                        {
                            foundSolid = true;
                            lastSolidZ = z;
                        }
                        else
                        {
                            var existing = this.GetRegionAt(global - new IntVec3(0, 0, 2)); // why 2?
                            if (foundSolid && lastSolidZ == z - 2 && existing == null)
                            {
                                foreach (var region in this.FloodNew(handled, x, y, lastSolidZ))
                                {
                                    this.Regions.Add(region);
                                }
                                foundSolid = false;

                            }
                        }
                    }
                }
            return this.Regions;
        }
    }
}
