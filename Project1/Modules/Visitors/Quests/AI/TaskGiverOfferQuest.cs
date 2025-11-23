namespace Start_a_Town_
{
    class TaskGiverOfferQuest : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var manager = actor.Map.Town.QuestManager;
            var nextQuestReceiver = manager.GetNextQuestReceiver(actor);
            if (nextQuestReceiver == null)
                return null;
            return new AITask(typeof(TaskBehaviorOfferQuest), nextQuestReceiver);
        }
    }
}
