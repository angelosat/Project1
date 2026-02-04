using Project1.Framework.Entities.Actors;
using Start_a_Town_;

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
