using System.Linq;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class TaskGiverItemOwnership : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var possesions = actor.GetPossesions();

            // first see if they need to drop an unowned item
            var carriedItems = actor.Inventory.All;

            // drop only items that have another specific owner set, instead of every unowned item
            var itemToDrop = carriedItems.Where(i => !actor.OwnsOrCanClaim(i)).FirstOrDefault();

            if (itemToDrop != null)
                return new Plan(typeof(TaskBehaviorDropItem)) { TargetA = new TargetArgs(itemToDrop) };

            // then continue to try and go pick up any owned items in the world
            foreach (var item in possesions)
            {
                if (!item.Exists)
                    continue;
                if (!actor.Inventory.Contains(item))
                {
                    return new Plan(typeof(BehaviorPickUpItemNew)) { TargetA = new TargetArgs(item) };
                }
            }
            return null;
        }

        public override Plan TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false)
        {
            if (target.Object is not Entity item)
                return null;
            if (!item.IsHaulable)
                return null;
            return new Plan(typeof(BehaviorPickUpItemNew), target);
        }
    }
}
