using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class TaskBehaviorEating : BehaviorExecutePlan
    {
        TargetArgs Food { get { return this.Plan.GetTarget(FoodInd); } }
        TargetArgs Table { get { return this.Plan.GetTarget(EatingSurfaceInd); } }
        public const TargetIndex FoodInd = TargetIndex.A, EatingSurfaceInd = TargetIndex.B;

        public override string Name => "Eating";
           
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Plan;
            yield return BehaviorHelper.InteractInInventoryOrWorld(FoodInd, () => new InteractionHaul(task.GetAmount(FoodInd)));
            yield return BehaviorHelper.SetTarget(FoodInd, () =>
            {
                var carried = actor.Hauled;
                var previousStack = task.GetTarget(FoodInd).Object;
                if (carried != previousStack)
                    actor.Unreserve(previousStack);
                return carried;
            });
            var eat = new BehaviorBeginInteraction(FoodInd, new Components.ConsumableComponent.InteractionConsume());

            yield return BehaviorHelper.JumpIfTrue(eat, () => this.Table.Type == TargetType.Null);

            yield return new BehaviorResolvePath(EatingSurfaceInd);
            var auxIndex = TargetIndex.C;
            yield return new BehaviorCustom() { InitAction = () => { this.Plan.SetTarget(auxIndex, Table.Global.Above().At(actor.Map)); } };
            yield return new BehaviorBeginInteraction(auxIndex, new UseHauledOnTarget());

            yield return eat;
            yield return new BehaviorBeginInteraction(() => new InteractionThrow());
        }

        protected override bool InitExtraReservations()
        {
            var tableaRes = (this.Table.Type == TargetType.Null) ? true : this.Reserve(Table, 1) && this.Reserve(Table.Global.Above());
            var tableRes = (this.Table.Type == TargetType.Null) ? true : this.Reserve(EatingSurfaceInd, 1) && this.Reserve(EatingSurfaceInd);// Table.Global.Above());
            return this.Reserve(Food, 1) && tableRes;
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
