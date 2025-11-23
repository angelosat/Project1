using System.Linq;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.AI
{
    class TaskGiverEat : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var hunger = actor.GetNeed(NeedDef.Hunger);
            
            var isHungry = hunger.IsBelowThreshold;

            if (isHungry)
            {
                //check inventory for meals
                var foodInInventory = actor.Inventory.First(i => i.IsFood);
                if (foodInInventory != null)
                {
                    IOrderedEnumerable<Vector3> eatingPlaces = FindEatingPlaces(actor);
                    if (eatingPlaces.Any())
                    {
                        return new AITask() { BehaviorType = typeof(TaskBehaviorEatWithTable) }
                            .SetTarget(TaskBehaviorEating.FoodInd, foodInInventory, 1)
                            .SetTarget(TaskBehaviorEating.EatingSurfaceInd, new TargetArgs(eatingPlaces.First()));
                    }
                    else
                    {
                        return new AITask() { BehaviorType = typeof(TaskBehaviorEatWithoutTable) }
                            .SetTarget(TaskBehaviorEating.FoodInd, foodInInventory, 1);
                    }
                }
            }
            var food = actor.Map.GetObjects()
                .Where(obj => obj.HasComponent<ConsumableComponent>() && actor.GetUnreservedAmount(obj) > 0)
                .Select(o => new TargetArgs(o))
                .OrderByReachableRegionDistance(actor)
                .FirstOrDefault();
            if (food == null)
                return null;
            if (food.Type == TargetType.Null)
                return null;
            var unreserved = actor.GetUnreservedAmount(food);

            if (!isHungry)
            {// if not currently hungry, and not currently having a food item in inventory, pick up food and store in inventory
                if (!actor.Inventory.Contains(i => i.IsFood))
                    return null;
                return null;
            }
            var map = actor.Map;
            TargetArgs eatingplace = TargetArgs.Null;
            var belowFood = food.Global - Vector3.UnitZ;
            var cellBelowFood = actor.Map.GetCell(belowFood);
            if (!map.Town.HasUtility(belowFood, Utility.Types.Eating))
            {
                    IOrderedEnumerable<Vector3> eatingPlaces = FindEatingPlaces(actor);
                if (eatingPlaces.Any())
                    eatingplace = new TargetArgs(actor.Map, eatingPlaces.First());
            }
            if(eatingplace != TargetArgs.Null)
                return new AITask(typeof(TaskBehaviorEatWithTable)).SetTarget(TaskBehaviorEating.FoodInd, food, 1).SetTarget(TaskBehaviorEating.EatingSurfaceInd, eatingplace);
            else
                return new AITask(typeof(TaskBehaviorEatWithoutTable)).SetTarget(TaskBehaviorEating.FoodInd, food, 1);
        }

        private static IOrderedEnumerable<Vector3> FindEatingPlaces(Actor actor)
        {
            return actor.Map.Town.GetUtilities(Utility.Types.Eating).Where(p => actor.CanReserve(p)).OrderBy(p => Vector3.DistanceSquared(p, actor.Global));
        }
    }
}
