using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.AI.Behaviors.Pathing;

public class PathNode : PathNodeBase
{
    public RegionNode RegionNodeGlobal;
    public bool IsQueued;
    public PathNode(MapBase map, IntVec3 global, IntVec3 goal)
    {
        this.Map = map;
        this.Global = global;
    }
    public override string ToString()
    {
        return this.Global.ToString() + " from " + (this.Parent != null ? this.Parent.Global.ToString() : "null");
    }
}
