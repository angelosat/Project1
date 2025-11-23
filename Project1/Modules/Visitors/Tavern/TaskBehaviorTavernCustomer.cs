using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorTavernCustomer : BehaviorPerformTask
    {
        static int Patience = Ticks.PerGameMinute; // TODO get patience value from actor

        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            var tableindex = TargetIndex.A;
            var table = task.GetTarget(tableindex);
            var tavern = actor.Town.GetShop(task.ShopID) as Tavern;
            var customerProps = tavern.GetCustomerProperties(actor);

            bool lostPatience()
            {
                if( task.TicksWaited > Patience)
                {
                    actor.GetVisitorProperties().BlacklistShop(tavern.ID);
                    return true;
                }
                return false;
            }

            yield return BehaviorHelper.MoveTo(tableindex);
            yield return new BehaviorCustom(() => customerProps.IsSeated = true);
            // TODO fail if waited too long
            yield return new BehaviorWait(() => customerProps.IsOrderTaken).FailOn(lostPatience);
            yield return new BehaviorCustom(() =>
            {
                task.TicksWaited = 0;
                task.SetTarget(TargetIndex.B, customerProps.OrderTakenBy);
            });
            yield return new BehaviorWait(() => customerProps.Dish?.CellIfSpawned == table.Global.Above()).FailOn(lostPatience);
            yield return new BehaviorCustom(() => actor.Reserve(customerProps.Dish));
            yield return new BehaviorCustom(() =>
            {
                var money = actor.GetMoney();
                task.SetTarget(TargetIndex.C, money, Math.Min(money.StackSize, customerProps.Dish.GetValueTotal())); // HACK temporary solution
            });
            yield return BehaviorHelper.SetTarget(TargetIndex.A, ()=> customerProps.Dish);
            yield return new BehaviorInteractionNew(TargetIndex.A, new Components.ConsumableComponent.InteractionConsume());
            yield return new BehaviorInteractionNew(TargetIndex.C, () => new InteractionHaul(this.Task.AmountC));
            yield return BehaviorHelper.SetTarget(TargetIndex.B, table.Global.Above());
            yield return BehaviorHelper.PlaceCarried(TargetIndex.B);
            yield return new BehaviorCustom(() => actor.GetVisitorProperties().AddRecentlyVisitedShop(tavern));
            /// TODO wait until a tavern worker
            /// a) has the corresponding taskbehavior
            /// b) or has started/completed a "rquest order" interaction
        }
    }
}
