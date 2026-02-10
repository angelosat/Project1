using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Towns.AI.Behaviors
{
    class TaskGiverBeTalkedTo : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var state = actor.AI.State;
            if(state.ConversationPartner is null)
                return null;
            return new Plan(typeof(TaskBehaviorBeTalkedTo));
        }
    }
}
