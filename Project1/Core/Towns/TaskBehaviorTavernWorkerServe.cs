using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Helpers;
using Project1.Core.AI.Behaviors.NodeTypes;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns
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
            yield return BehaviorHaulHelper.StartCarrying(this, dishIndex);
            yield return BehaviorHelper.MoveTo(tableSurfaceIndex);
            throw new NotImplementedException();
            //yield return BehaviorHelper.PlaceCarried(tableSurfaceIndex);
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
