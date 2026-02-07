using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Simulation;
using Project1.Core.Entities;
using Project1.Core.Towns.Crafting;
namespace Project1.Core.Towns.Refueling
{
    class RefuelingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var map = actor.Map;
            var refuelables = GetRefuelables(map);

            var target = refuelables.FirstOrDefault(c => 
                c.Fuel.Percentage < .5f && 
                actor.CanReachAndReserve(c.Parent.OriginGlobal));

            if (target is null)
                return null;

            if (actor.Hauled is Entity carried)
                return new Plan(PlanDefOf.GoPlace, new TargetArgs(map, target.Parent.OriginGlobal)) { TargetB = new TargetArgs(target.Parent) };
            var items = map.Stockpiles.AllItems.Where(CraftingSystem.IsFuel);
            foreach (var i in items)
            {
                if (!actor.CanReachAndReserve(i))
                    continue;
                return new Plan(PlanDefOf.GoHaul, i);
            }

            return null;
        }
        static IEnumerable<BlockFuelComp> GetRefuelables(MapBase map) => map.BlockEntities.Where(e => e.HasComp<BlockFuelComp>()).Select(e => e.GetComp<BlockFuelComp>());
    }
}
