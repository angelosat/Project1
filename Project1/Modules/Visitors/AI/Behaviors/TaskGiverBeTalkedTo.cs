namespace Start_a_Town_
{
    class TaskGiverBeTalkedTo : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var state = actor.GetState();
            if(state.ConversationPartner == null)
                return null;
            return new AITask(typeof(TaskBehaviorBeTalkedTo));
        }
    }
}
