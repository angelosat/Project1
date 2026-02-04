using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_;
using Project1.Core.AI.Behaviors.Pathing;

namespace Project1.Core.Towns.AI.Behaviors
{
    class TaskBehaviorBeTalkedTo : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorStopMoving();
            var actor = this.Actor;
            var state = actor.GetState();
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
