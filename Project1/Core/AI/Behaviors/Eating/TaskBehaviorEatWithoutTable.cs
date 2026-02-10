using Project1.Core.AI.Behaviors.Helpers;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.Entities;
using Project1.Core.Interactions;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors.Eating
{
    class TaskBehaviorEatWithoutTable : BehaviorExecutePlan
    {
        TargetArgs Food { get { return this.Plan.GetTarget(FoodInd); } }
        public const TargetIndex FoodInd = TargetIndex.A, EatingSurfaceInd = TargetIndex.B;

        public override string Name => "Eating";
          
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Plan;
            yield return BehaviorHelper.InteractInInventoryOrWorld(FoodInd, () => null);//  new InteractionHaul(task.GetAmount(FoodInd))); //));// 
            yield return BehaviorHelper.SetTarget(FoodInd, () =>
            {
                var carried = actor.Hauled;
                var previousStack = task.GetTarget(FoodInd).Object;
                if (carried != previousStack)
                    actor.Unreserve(previousStack);
                return carried;
            });
            yield return new BehaviorResolveInteraction(FoodInd, new ConsumableComponent.InteractionConsume());
            yield return new BehaviorResolveInteraction(() => new InteractionThrow());
        }

        protected override bool ReserveExtra()
        {
            return this.Reserve(Food, 1);
        }
    }
}
