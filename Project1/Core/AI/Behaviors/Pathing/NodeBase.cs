using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Simulation;

namespace Project1.Core.AI.Behaviors.Pathing
{
    public abstract class NodeBase
    {
        public MapBase Map;
        public Vector3 Global;
        public TargetArgs Target;
        public NodeBase Parent;
        public float CostToGoal;
        public float CostFromStart;
        public List<Vector3> CellsToTraverse = new();
    }
}
