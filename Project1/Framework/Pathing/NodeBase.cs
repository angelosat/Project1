using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Framework.Base;
using Project1.Framework.WorldGen;

namespace Project1.Framework.Pathing
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
