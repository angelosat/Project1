using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.AI
{
    class EatingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            // if food in hands, eat it
            if (actor.Hauled is not Entity carried)
                return null;

            // if carrying non-consumable, exit
            if (!carried.TryGetComponent<ConsumableComponent>(out var comp))
                return null;

            // if carrying non-food, exit
            if (!comp.HasEffectTarget(NeedDefOf.Hunger))
                return null;

            return new Plan(PlanDefOf.Eating, carried);
        }
        protected Plan TryPlanOld(Actor actor)
        {
            var hunger = actor.GetNeed(NeedDefOf.Hunger);
            
            var isHungry = hunger.IsBelowThreshold;

            if (isHungry)
            {
                //check inventory for meals
                var foodInInventory = actor.Inventory.First(i => i.IsFood);
                if (foodInInventory != null)
                {
                    var eatingPlaces = FindEatingPlaces(actor);
                    if (eatingPlaces.Any())
                    {
                        return new Plan() { BehaviorType = typeof(TaskBehaviorEatWithTable) }
                            .SetTarget(TaskBehaviorEating.FoodInd, foodInInventory, 1)
                            .SetTarget(TaskBehaviorEating.EatingSurfaceInd, eatingPlaces.First().At(actor.Map));
                    }
                    else
                    {
                        return new Plan() { BehaviorType = typeof(TaskBehaviorEatWithoutTable) }
                            .SetTarget(TaskBehaviorEating.FoodInd, foodInInventory, 1);
                    }
                }
            }
            var food = actor.Map.GetEntities()
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
                    var eatingPlaces = FindEatingPlaces(actor);
                if (eatingPlaces.Any())
                    eatingplace = new TargetArgs(actor.Map, eatingPlaces.First());
            }
            if(eatingplace != TargetArgs.Null)
                return new Plan(typeof(TaskBehaviorEatWithTable)).SetTarget(TaskBehaviorEating.FoodInd, food, 1).SetTarget(TaskBehaviorEating.EatingSurfaceInd, eatingplace);
            else
                return new Plan(typeof(TaskBehaviorEatWithoutTable)).SetTarget(TaskBehaviorEating.FoodInd, food, 1);
        }

        private static IOrderedEnumerable<IntVec3> FindEatingPlaces(Actor actor)
        {
            return actor.Map.Town.GetUtilities(Utility.Types.Eating).Where(p => actor.CanReserve(p)).OrderBy(p => Vector3.DistanceSquared(p, actor.Global));
        }
    }
}
