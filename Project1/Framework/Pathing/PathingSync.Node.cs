using Microsoft.Xna.Framework;
using Project1.Framework.WorldGen;

namespace Project1.Framework.Pathing
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
