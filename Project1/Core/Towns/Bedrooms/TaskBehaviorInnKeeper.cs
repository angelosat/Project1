using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Helpers;
using Project1.Core.Entities;
using Project1.Core.Interactions;
using Project1.Core;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Towns
{
    class TaskBehaviorInnKeeper : BehaviorExecutePlan
    {
        const TargetIndex Customer = TargetIndex.A;
        const TargetIndex Counter = TargetIndex.B;

        protected override IEnumerable<Behavior> GetSteps()
        {
            var task = this.Plan;
            var actor = this.Actor;
            var map = actor.Map;
            var shop = actor.Workplace as Tavern;
            var counter = shop.Counter.Value;
            var counterSurface = counter.Above;
            var customerProps = shop.GetCustomerProperties(task.GetTarget(Customer));
            var customer = customerProps.Customer;
            var counterCell = map.GetCell(counter);
            var room = customerProps.Bedroom;

            yield return BehaviorHelper.SetTarget(Customer, customer);
            yield return BehaviorHelper.SetTarget(Counter, () => (map, counter + counterCell.Back));

            yield return BehaviorHelper.MoveTo(Counter, PathEndMode.Exact);
            yield return new BehaviorWait(() => customer.CellIfSpawned.Value == counter + counterCell.Front);
            yield return new BehaviorWait(() =>
            {
                var money = map.GetEntitiesAt(counterSurface).FirstOrDefault(o => o.Def == ItemDefOf.Coins);
                if (money == null)
                    return false;
                if (money.StackSize < room.Value)
                {
                    // TODO fail?
                }
                task.SetTarget(TargetIndex.C, money, room.Value);
                //actor.Reserve(this.Task, money, money.StackSize);
                this.Reserve(money, money.StackSize);
                return true;
            });
            // TODO pickup money or leave it to be hauled?
            yield return BehaviorHaulHelper.StartCarrying(this, TargetIndex.C);
            yield return new BehaviorResolveInteraction(() => new InteractionStoreHauled());
            yield return new BehaviorResolveInteraction(Customer, () => new InteractionAssignVisitorRoom(room.ID));
            yield return new BehaviorCustom(() =>
            {
            });
        }
    }
}
