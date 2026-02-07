using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Towns.AI.Behaviors
{
    class TaskGiverBeTalkedTo : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var state = actor.GetState();
            if(state.ConversationPartner == null)
                return null;
            return new Plan(typeof(TaskBehaviorBeTalkedTo));
        }
    }
}
