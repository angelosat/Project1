using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorVisitorRentBed : BehaviorPerformTask
    {
        const TargetIndex Counter = TargetIndex.B;
        protected override IEnumerable<Behavior> GetSteps()
        {
            var task = this.Task;
            var actor = this.Actor;
            var map = actor.Map;
            var tavern = actor.Town.ShopManager.GetShop<Tavern>(task.ShopID);
            var counter = tavern.Counter.Value;
            var counterCell = map.GetCell(counter);
            var counterBehind = counter + counterCell.Back;
            var counterSurface = counter.Above;
            var customerProps = tavern.GetCustomerProperties(actor);
            var room = customerProps.Bedroom;
            this.FailOnRanOutOfPatienceWaiting(() => actor.GetVisitorProperties().BlacklistShop(task.ShopID));

            yield return BehaviorHelper.SetTarget(Counter, () => (map, map.GetFrontOfBlock(counter)));
            yield return BehaviorHelper.MoveTo(Counter, PathEndMode.Exact);

            // wait until innkeeper arrives behind counter, then take out money and drop it on counter
            yield return new BehaviorWait(tavern.IsInnkeeperServicing);
            yield return new BehaviorInteractionNew(() => actor.GetMoney(), () => new InteractionHaul(room.Value));
            yield return new BehaviorInteractionNew(() => (actor.Map, counterSurface), () => new UseHauledOnTarget());

            yield return new BehaviorWait(() => room.OwnerRef == actor.RefId); // isserved was made for dining. create a new field for bed renting?
            // TODO wait until visitor has ownership of the bed stored in customerprops 
        }

        protected override bool InitExtraReservations()
        {
            var task = this.Task;
            var actor = this.Actor;
            var tavern = actor.Town.ShopManager.GetShop<Tavern>(task.ShopID);
            var counter = tavern.Counter.Value;
            return actor.Reserve(counter);
        }
    }
}
