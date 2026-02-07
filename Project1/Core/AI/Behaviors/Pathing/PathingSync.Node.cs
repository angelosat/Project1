using Microsoft.Xna.Framework;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Simulation;

namespace Project1.Core.Pathing
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
