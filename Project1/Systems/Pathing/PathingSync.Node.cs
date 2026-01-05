using Microsoft.Xna.Framework;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_
{
    public partial class PathingSync
    {
        public class Node : NodeBase
        {
            public RegionNode RegionNodeGlobal;
            public bool IsQueued;
            public Node(MapBase map, Vector3 global, Vector3 goal)
            {
                this.Map = map;
                this.Global = global;
            }
            public override string ToString()
            {
                return this.Global.ToString() + " from " + (this.Parent != null ? this.Parent.Global.ToString() : "null");
            }
        }
    }
}
