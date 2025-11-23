using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorBeTalkedTo : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorStopMoving();
            var actor = this.Actor;
            var state = actor.GetState();
            var task = this.Task;
            yield return new BehaviorWait(() =>
            {
                return state.ConversationPartner == null;
            })
            {
                TickAction = () =>
                {
                    actor.FaceTowards(task.TargetA);
                }
            };
        }
    }
}
