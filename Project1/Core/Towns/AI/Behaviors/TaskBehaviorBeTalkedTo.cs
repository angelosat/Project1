using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Towns.AI.Behaviors
{
    class TaskBehaviorBeTalkedTo : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorStopMoving();
            var actor = this.Actor;
            var state = actor.AI.State;
            var task = this.Plan;
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
