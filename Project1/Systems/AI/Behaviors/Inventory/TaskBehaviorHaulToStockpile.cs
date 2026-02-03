using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_.AI.Behaviors;
using Project1.Framework.Pathing;

namespace Start_a_Town_
{
    class TaskBehaviorHaulToStockpile : BehaviorExecutePlan
    {
        public override string Name { get; } = "Hauling to stockpile";

        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var cell = this.Plan.TargetA;
            this.FailOn(() => 
                actor.Map.Town.GetZoneAt(cell.Global.Below()) is not Stockpile stockpile ||
                (actor.Hauled is Entity carried && !stockpile.Accepts(actor.Hauled as Entity)));
            yield return new BehaviorResolvePath(PathEndMode.Any);
            yield return new BehaviorResolveInteraction();
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
            return this.Reserve(TargetIndex.A);
        }
    }
}
