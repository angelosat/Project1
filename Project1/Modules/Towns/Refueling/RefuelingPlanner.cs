using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Start_a_Town_
{
    class RefuelingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var map = actor.Map;

            if (actor.Hauled is Entity carried)
            {
                var refuelables = GetRefuelables(map);
                foreach(var e in refuelables)
                {
                    var blockEntity = e.Parent;
                    foreach(var cell in blockEntity.CellsOccupied)
                        if(actor.CanReachAndReserve(cell))
                            return new Plan(PlanDefOf.GoPlace, new TargetArgs(map, cell)) { TargetB = new TargetArgs(blockEntity) };
                }
            }
            var items = map.Stockpiles.AllItems.Where(CraftingSystem.IsFuel);
            //var fuelitems = items.Where(IsFuel);
            foreach (var i in items)
            {
                if (!actor.CanReachAndReserve(i))
                    continue;
                return new Plan(PlanDefOf.GoHaul, i);
            }

            return null;
        }
        static IEnumerable<BlockFuelComp> GetRefuelables(MapBase map) => map.BlockEntities.Where(e => e.HasComp<BlockFuelComp>()).Select(e => e.GetComp<BlockFuelComp>());
        

        //protected override AITask TryAssignTask(Actor actor)
        //{
        //    var refuelables = actor.Town.GetRefuelablesNew();
        //    foreach (var target in refuelables)
        //    {
        //        if (!actor.CanReserve(target.Key))
        //            continue;
        //        var refComp = target.Value.GetComp<BlockEntityCompRefuelable>();
        //        if (refComp?.Fuel.Percentage > .5f)
        //            continue;
        //        var fuelProgress = refComp.Fuel;
        //        var fuelMissing = fuelProgress.Max - fuelProgress.Value;
        //        var allObjects = actor.Map.GetObjectsLazy();
        //        var handled = new HashSet<GameObject>();
        //        foreach (var fuel in allObjects)
        //        {
        //            handled.Add(fuel);
        //            if (!actor.CanReachNew(fuel))
        //                continue;
        //            if (!actor.CanReserve(fuel))
        //                continue;
        //            if (!fuel.IsHaulable)
        //                continue;
        //            if (!refComp.Accepts(fuel as Entity))
        //                continue;
        //            if (fuel.Material?.Fuel?.Value > 0)
        //            {
        //                var task = new AITask(TaskDefOf.Refueling);// 
        //                task.SetTarget(TaskBehaviorRefueling.DestinationIndex, new TargetArgs(actor.Map, target.Key));
        //                foreach (var similar in CollectUntilFull(actor, refComp, fuel, fuelMissing, handled))
        //                    task.AddTarget(TaskBehaviorRefueling.SourceIndex, new TargetArgs(similar.Key), similar.Value);
        //                return task;
        //            }
        //        }
        //    }
        //    return null;
        //}
        protected Plan TryPlanOld(Actor actor)
        {
            return null;
            var refuelables = actor.Town.GetRefuelablesNew();
            foreach (var target in refuelables)
            {
                if (!actor.CanReserve(target.Key))
                    continue;
                var freeInteractionSpots = Cell.GetFreeInteractionSpots(actor.Map, target.Key, actor);
                if (!freeInteractionSpots.Any())
                    continue;
                var refComp = target.Value.GetComp<BlockEntityCompRefuelable>();
                if (refComp?.Fuel.Percentage > .5f)
                    continue;
                var fuelProgress = refComp.Fuel;
                var fuelMissing = fuelProgress.Max - fuelProgress.Value;
                var allObjects = actor.Map.GetEntities();
                var handled = new HashSet<GameObject>();
                foreach (var fuel in allObjects)
                {
                    handled.Add(fuel);
                    if (!actor.CanReach(fuel))
                        continue;
                    if (!actor.CanReserve(fuel))
                        continue;
                    if (!fuel.IsHaulable)
                        continue;
                    if (!refComp.Accepts(fuel as Entity))
                        continue;
                    if (fuel.Material?.Fuel?.Value > 0)
                    {
                        var task = new Plan(PlanDefOf.Refueling);// 
                        task.SetTarget(TaskBehaviorRefueling.DestinationIndex, new TargetArgs(actor.Map, target.Key));
                        foreach (var similar in CollectUntilFull(actor, refComp, fuel, fuelMissing, handled))
                            task.AddTarget(TaskBehaviorRefueling.SourceIndex, new TargetArgs(similar.Key), similar.Value);
                        return task;
                    }
                }
            }
            return null;
        }

        static IEnumerable<KeyValuePair<GameObject, int>> CollectUntilFull(Actor actor, BlockEntityCompRefuelable refComp, GameObject center, float fuelMissing, HashSet<GameObject> handled)
        {
            var similarNearby = center.Map.GetNearbyObjectsNew(center.Global, r => r <= 5, f => f.IsFuel);
            int stackEnduranceLimit = actor.GetHaulStackLimitFromEndurance(center);

            float currentFuelValue = 0;
            int totalAmountToCollect = 0;
            var enumerator = similarNearby.GetEnumerator();
            var fuelPerItem = center.Material.Fuel.Value;
            var maxCapacity = refComp.GetCapacityFor(center as Entity);
            var max = Math.Min(center.StackMax, maxCapacity);
            while (
                totalAmountToCollect < max &&
                totalAmountToCollect + 1 <= stackEnduranceLimit && // we're just below encumberance limit
                enumerator.MoveNext())
            {
                var fuelItem = enumerator.Current;
                if (handled.Contains(fuelItem) && fuelItem != center)
                    continue;
                handled.Add(fuelItem);
                if (!fuelItem.IsHaulable)
                    continue;
           
                if (fuelItem != center && !center.CanAbsorb(fuelItem))
                    continue;
                if (!refComp.Accepts(fuelItem as Entity))
                    continue;

                var fuelValue = fuelItem.Fuel;
                var desiredAmountToCollectByFuel = (int)(fuelMissing / fuelValue);
                var desiredAmountToCollectByWeight = (int)Math.Min(stackEnduranceLimit - totalAmountToCollect, desiredAmountToCollectByFuel);
                var actualAmountToCollect = (int)Math.Min(fuelItem.StackSize, desiredAmountToCollectByWeight);
                var fuelValueToCollect = actualAmountToCollect * fuelValue;
                currentFuelValue += fuelValueToCollect;
                totalAmountToCollect += actualAmountToCollect;
                yield return new KeyValuePair<GameObject, int>(fuelItem, actualAmountToCollect);
            }
        }
    }
}
