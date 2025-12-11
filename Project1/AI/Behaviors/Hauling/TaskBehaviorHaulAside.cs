using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class TaskBehaviorHaulAside : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            yield return BehaviorHaulHelper.StartCarrying(TargetIndex.A);
            yield return new BehaviorGetAtNewNew(TargetIndex.B);
            //yield return BehaviorHaulHelper.DropInStorage(TargetIndex.B);
            yield return new BehaviorInteractionNew(TargetIndex.B, () => new UseHauledOnTargetNew());
        }
        protected override bool InitExtraReservations()
        {
            var task = this.Task;
            return
                this.Actor.Reserve(task, task.GetTarget(TargetIndex.A), task.Count) &&
                this.Actor.Reserve(task, task.GetTarget(TargetIndex.B), 1);
        }
    }
}
