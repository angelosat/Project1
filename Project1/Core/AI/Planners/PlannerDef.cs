using Project1.Core.AI.Behaviors;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.AI.Planners
{
    public class PlannerDef(string name, Type workerType) : Def(name)
    {
        public Planner Worker = ActivatorSafe<Planner>.CreateInstance(workerType);
    }
}
