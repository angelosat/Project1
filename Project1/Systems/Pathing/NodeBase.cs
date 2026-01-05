using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.PathFinding
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
