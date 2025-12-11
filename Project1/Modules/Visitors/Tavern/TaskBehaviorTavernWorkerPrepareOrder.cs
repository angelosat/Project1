using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class TaskBehaviorTavernWorkerPrepareOrder : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            var ingredientIndex = TargetIndex.A;
            var workstationIndex = TargetIndex.B;
            var workstationAbove = TargetIndex.C;
            var beginHaul = BehaviorHelper.ExtractNextTargetAmount(ingredientIndex);
            var shop = actor.Workplace as Tavern;
            var customerProps = shop.GetCustomerProperties(actor);
            yield return beginHaul;
            yield return BehaviorHelper.MoveTo(ingredientIndex);
            //yield return BehaviorHelper.StartCarrying(ingredientIndex, ingredientIndex);
            yield return BehaviorHaulHelper.StartCarrying(this, ingredientIndex);
            yield return BehaviorHelper.MoveTo(workstationIndex);
            yield return BehaviorHelper.SetTarget(workstationAbove, (actor.Map, task.GetTarget(workstationIndex).Global.Above()));
            yield return BehaviorHelper.PlaceCarried(workstationAbove);
            yield return BehaviorHelper.JumpIfMoreTargets(beginHaul, ingredientIndex);
            yield return new BehaviorInteractionNew(TargetIndex.B, () => new InteractionCraftVisitorRequest(shop, task.Order, task.IngredientsUsed));
            yield return new BehaviorCustom(() => customerProps.Dish = task.CraftedItems.First());
        }

        protected override bool InitExtraReservations()
        {
            var actor = this.Actor;
            var task = this.Task;

            return this.ReserveAll(TargetIndex.A)
                && this.Reserve(TargetIndex.B);
        }
    }
}
