using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorTavernWorkerServe : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Plan;
            var dishIndex = TargetIndex.A;
            var tableSurfaceIndex = TargetIndex.B;
            var shop = actor.Workplace as Tavern;
            yield return BehaviorHelper.MoveTo(dishIndex);
            //yield return BehaviorHelper.StartCarrying(dishIndex);
            yield return BehaviorHaulHelper.StartCarrying(this, dishIndex);
            yield return BehaviorHelper.MoveTo(tableSurfaceIndex);
            yield return BehaviorHelper.PlaceCarried(tableSurfaceIndex);
            yield return new BehaviorCustom(() =>
            {
                shop.RemoveCustomer(task.CustomerID);
            });
        }
        protected override bool ReserveExtra()
        {
            return this.Reserve(TargetIndex.A);
        }
    }
}
