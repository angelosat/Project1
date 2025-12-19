namespace Start_a_Town_
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
