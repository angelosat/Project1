using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Project1.Framework.Pathing;
using Project1.Core.AI.Behaviors.Pathing;

namespace Start_a_Town_
{
    class TaskBehaviorGoCraft : BehaviorExecutePlan
    {
        public override string Name { get; } = "Crafting";

        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(PathEndMode.InteractionSpot);
            yield return new BehaviorResolveInteraction();
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
        }
        
    }
}
