using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Framework;

namespace Project1.Core
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
