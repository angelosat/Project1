using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class TaskBehaviorEatWithoutTable : BehaviorPerformTask
    {
        TargetArgs Food { get { return this.Task.GetTarget(FoodInd); } }
        public const TargetIndex FoodInd = TargetIndex.A, EatingSurfaceInd = TargetIndex.B;

        public override string Name => "Eating";
          
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            yield return BehaviorHelper.InteractInInventoryOrWorld(FoodInd, () => new InteractionHaul(task.GetAmount(FoodInd))); //));// 
            yield return BehaviorHelper.SetTarget(FoodInd, () =>
            {
                var carried = actor.Hauled;
                var previousStack = task.GetTarget(FoodInd).Object;
                if (carried != previousStack)
                    actor.Unreserve(previousStack);
                return carried;
            });
            yield return new BehaviorInteractionNew(FoodInd, new Components.ConsumableComponent.InteractionConsume());
            yield return new BehaviorInteractionNew(() => new InteractionThrow());
        }

        protected override bool InitExtraReservations()
        {
            return Actor.Reserve(this.Task, Food, 1);
        }
    }
}
