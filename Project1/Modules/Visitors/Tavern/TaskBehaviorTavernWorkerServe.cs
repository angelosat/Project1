using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorTavernWorkerServe : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            var dishIndex = TargetIndex.A;
            var tableSurfaceIndex = TargetIndex.B;
            var shop = actor.Workplace as Tavern;
            yield return BehaviorHelper.MoveTo(dishIndex);
            //yield return BehaviorHelper.StartCarrying(dishIndex);
            yield return BehaviorHaulHelper.StartCarrying(dishIndex);
            yield return BehaviorHelper.MoveTo(tableSurfaceIndex);
            yield return BehaviorHelper.PlaceCarried(tableSurfaceIndex);
            yield return new BehaviorCustom(() =>
            {
                shop.RemoveCustomer(task.CustomerID);
            });
        }
        protected override bool InitExtraReservations()
        {
            return this.Task.Reserve(this.Actor, TargetIndex.A);
        }
    }
}
