using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class TaskBehaviorEatWithTable : BehaviorPerformTask
    {
        TargetArgs Food { get { return this.Task.GetTarget(FoodInd); } }
        TargetArgs Table { get { return this.Task.GetTarget(EatingSurfaceInd); } }
        public const TargetIndex FoodInd = TargetIndex.A, EatingSurfaceInd = TargetIndex.B;

        public override string Name => "Eating";
       
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            yield return BehaviorHelper.InteractInInventoryOrWorld(FoodInd, () => new InteractionHaul(task.GetAmount(FoodInd)));
            yield return BehaviorHelper.SetTarget(FoodInd, () =>
            {
                var carried = actor.Hauled;
                var previousStack = task.GetTarget(FoodInd).Object;
                if (carried != previousStack)
                    actor.Unreserve(previousStack);
                return carried;
            });
            yield return new BehaviorGetAtNewNew(EatingSurfaceInd);
            var onTable = TargetIndex.C;
            yield return BehaviorHelper.SetTarget(onTable, Table.Global.Above());
            yield return new BehaviorInteractionNew(onTable, new UseHauledOnTarget());
            yield return new BehaviorInteractionNew(FoodInd, new Components.ConsumableComponent.InteractionConsume());
            yield return new BehaviorInteractionNew(() => new InteractionThrow()); // in case somehow more food than necessary is been carried
        }

        protected override bool InitExtraReservations()
        {
            var tableRes = (Table.Type == TargetType.Null) ? true : Actor.Reserve(this.Task, Table, 1) && Actor.Reserve(this.Task, Table.Global.Above());
            return Actor.Reserve(this.Task, Food, 1) && tableRes;
        }

        private bool IsTableSurfaceEmpty(TargetArgs table)
        {
            return !this.Actor.Map.GetObjects(table.Global).Any();
        }
        private static bool IsTableSurfaceEmpty(MapBase map, Vector3 table)
        {
            return !map.GetObjects(table).Any();
        }
    }
}
