using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class RegionNode : Inspectable
    {
        public override string Label => nameof(RegionNode);
        public IntVec3 Local, Global;
        public RegionNode North, West, East, South;

        public override string ToString()
        {
            return $"Global: {this.Global}\nRegion: {this.Region.RegionID}\nLinks: {this.GetLinks().Count()}";
        }
        
        public Region Region;
        public RegionRoom Room { get { return this.Region.Room; } }
        public Chunk Chunk { get { return this.Region.Chunk; } }
        
        public RegionNode(Region region, IntVec3 local)
        {
            this.Region = region;
            this.Global = local.ToGlobal(region.Chunk);
            this.Local = local;
        }

        public IEnumerable<RegionNode> GetLinksLazy()
        {
            if (this.North != null) yield return this.North;
            if (this.West != null) yield return this.West;
            if (this.East != null) yield return this.East;
            if (this.South != null) yield return this.South;
        }
        public IEnumerable<RegionNode> GetLinks()
        {
            var list = new List<RegionNode>();
            if (this.North != null) list.Add(this.North);
            if (this.West != null) list.Add(this.West);
            if (this.East != null) list.Add(this.East);
            if (this.South != null) list.Add(this.South);
            return list;
        }
        public void Replace(RegionNode oldNode)
        {
            if (oldNode.North != null && Math.Abs(this.Global.Z - oldNode.North.Global.Z) <= 1) this.North = oldNode.North;
            if (oldNode.South != null && Math.Abs(this.Global.Z - oldNode.South.Global.Z) <= 1) this.South = oldNode.South;
            if (oldNode.East != null && Math.Abs(this.Global.Z - oldNode.East.Global.Z) <= 1) this.East = oldNode.East;
            if (oldNode.West != null && Math.Abs(this.Global.Z - oldNode.West.Global.Z) <= 1) this.West = oldNode.West;
        }

        internal void ClearLinks()
        {
            if (this.West != null)
                this.West.East = null;
            if (this.East != null)
                this.East.West = null;
            if (this.North != null)
                this.North.South = null;
            if (this.South != null)
                this.South.North = null;
            this.North = null;
            this.South = null;
            this.West = null;
            this.East = null;
        }
    }
}
