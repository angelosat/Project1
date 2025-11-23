namespace Start_a_Town_
{
    class TaskGiverHaulToStockpile : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Hauler))
                return null;
            if (actor.IsTooTiredToWork)
                return null;
            if (actor.Hauled is not null)
                return null;
            var items = HaulHelper.GetPotentialItemsNew(actor);
            var task = StockpileAIHelper.FindBestTask(actor, items);
            return task;
        }
        
        public override AITask TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false)
        {
            return target.Object is Entity item ? StockpileAIHelper.TryHaulNew(actor, item, ignoreOtherReservations) : null;
        }
    }
}
