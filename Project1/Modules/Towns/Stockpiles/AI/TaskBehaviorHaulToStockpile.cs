using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorHaulToStockpile : BehaviorPerformTask
    {
        public const TargetIndex ItemInd = TargetIndex.A, StorageInd = TargetIndex.B;
        TargetArgs Item => this.Task.GetTarget(ItemInd);
        TargetArgs Storage => this.Task.GetTarget(StorageInd);

        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnForbidden(ItemInd);

            var gotohaul = new BehaviorGetAtNewNew(ItemInd).FailOn(failCollecting);
            yield return gotohaul;
            yield return BehaviorHaulHelper.StartCarrying(ItemInd);
            yield return BehaviorHaulHelper.FindNearbyHaulOpportunity(gotohaul, ItemInd).FailOnNotCarrying();

            var gotoStorage = new BehaviorGetAtNewNew(StorageInd);
            var findNextStorage = BehaviorHaulHelper.JumpIfNextStorageFound(gotoStorage, StorageInd);
            gotoStorage.JumpIf(deliverFail, findNextStorage);

            yield return gotoStorage;
            yield return BehaviorHaulHelper.DropInStorage(StorageInd).JumpIf(deliverFail, findNextStorage);
            yield return findNextStorage;

            bool deliverFail()
            {
                var o = this.Actor.Hauled;
                if (o == null)
                    return true;
                var fail = !HaulHelper.IsValidStorage(this.Task.GetTarget(StorageInd), this.Actor.Map, o);
                if (fail)
                    this.Actor.StopPathing();
                return fail;
            }
            bool failCollecting()
            {
                var map = this.Actor.Map;
                var o = this.Task.GetTarget(ItemInd).Object;
                foreach (var destination in this.Task.GetTargetQueue(StorageInd))
                    if (!destination.IsValidStorage(map, o))
                        return true;
                return false;
            }
        }

        protected override bool InitExtraReservations()
        {
            var task = this.Task;
            var item = task.GetTarget(ItemInd);
            var storageTarget = task.GetTarget(StorageInd);
            return
                this.Actor.ReserveAsManyAsPossible(item, task.Count) &&
                this.Actor.Reserve(storageTarget, 1);
        }
    }
}
