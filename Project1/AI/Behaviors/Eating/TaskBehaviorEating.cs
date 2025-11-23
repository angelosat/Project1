using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class TaskBehaviorEating : BehaviorPerformTask
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
            var eat = new BehaviorInteractionNew(FoodInd, new Components.ConsumableComponent.InteractionConsume());

            yield return BehaviorHelper.JumpIfTrue(eat, () => this.Table.Type == TargetType.Null);

            yield return new BehaviorGetAtNewNew(EatingSurfaceInd);
            var auxIndex = TargetIndex.C;
            yield return new BehaviorCustom() { InitAction = () => { this.Task.SetTarget(auxIndex, Table.Global.Above()); } };
            yield return new BehaviorInteractionNew(auxIndex, new UseHauledOnTarget());

            yield return eat;
            yield return new BehaviorInteractionNew(() => new InteractionThrow());
        }

        protected override bool InitExtraReservations()
        {
            var tableRes = (Table.Type == TargetType.Null) ? true : Actor.Reserve(Table, 1) && Actor.Reserve(Table.Global.Above());
            return Actor.Reserve(Food, 1) && tableRes;
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
