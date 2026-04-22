using Project1.Core.Simulation;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors.Pathing;

public abstract class PathNodeBase
{
    public MapBase Map;
    public IntVec3 Global;
    //public InteractionTarget Target;
    public PathNodeBase Parent;
    public float CostToGoal;
    public float CostFromStart;
    public List<IntVec3> CellsToTraverse = new();
}
//public abstract class NodeBase
//{
//    public MapBase Map;
//    public Vector3 Global;
//    public InteractionTarget Target;
//    public NodeBase Parent;
//    public float CostToGoal;
//    public float CostFromStart;
//    public List<Vector3> CellsToTraverse = new();
//}
