using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Framework;

namespace Project1.Core.Towns
{
    class TaskBehaviorTavernWorkerPrepareOrder : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Plan;
            var ingredientIndex = TargetIndex.A;
            var workstationIndex = TargetIndex.B;
            var workstationAbove = TargetIndex.C;
            var beginHaul = BehaviorHelper.ExtractNextTargetAmount(ingredientIndex);
            var shop = actor.Workplace as Tavern;
            var customerProps = shop.GetCustomerProperties(actor);
            yield return beginHaul;
            yield return BehaviorHelper.MoveTo(ingredientIndex);
            yield return BehaviorHaulHelper.StartCarrying(this, ingredientIndex);
            yield return BehaviorHelper.MoveTo(workstationIndex);
            yield return BehaviorHelper.SetTarget(workstationAbove, (actor.Map, task.GetTarget(workstationIndex).Global.Above()));
            yield return BehaviorHelper.PlaceCarried(workstationAbove);
            yield return BehaviorHelper.JumpIfMoreTargets(beginHaul, ingredientIndex);
            throw new NotImplementedException();
            yield return new BehaviorCustom(() => customerProps.Dish = task.CraftedItems.First());
        }

        protected override bool ReserveExtra()
        {
            var actor = this.Actor;
            var task = this.Plan;

            return this.ReserveAll(TargetIndex.A)
                && this.Reserve(TargetIndex.B);
        }
    }
}
