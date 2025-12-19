namespace Start_a_Town_
{
    class TaskGiverOfferQuest : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var manager = actor.Map.Town.QuestManager;
            var nextQuestReceiver = manager.GetNextQuestReceiver(actor);
            if (nextQuestReceiver == null)
                return null;
            return new Plan(typeof(TaskBehaviorOfferQuest), nextQuestReceiver);
        }
    }
}
