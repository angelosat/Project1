using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Reservations;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Designations;
using Project1.Core.Towns.Duties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Plants
{
    class PlannerPlanting : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var map = actor.Map;
            //if (!actor.HasDuty(DutyDefOf.Farmer))
            //    return null;
            var manager = map.Town.GrowingManager;
            var zones = manager.AllGrowingZones;
            var mapItems = actor.Map.Entities;
            if (actor.Hauled is Entity carried && carried.Def == ItemDefOf.Seeds)
            {
                var batches = GetReachableBatches(actor, manager.GetSowingTargetsAll(carried));
                if (!batches.Any())
                    return null;
                foreach (var (targets, zone) in batches)
                {
                    var reservableTargets = targets.Where(actor.CanReserve).ToList();
                    var totalReservable = reservableTargets.Count;

                    // find more seeds to top off carried seeds
                    var remaining = totalReservable - carried.StackSize;
                    if (remaining > carried.StackSize)
                    {
                        var candidates = mapItems.Where(c => 
                            c.Def == ItemDefOf.Seeds && 
                            c.Profile == carried.Profile && 
                            actor.CanReachAndReserve(c));
                        if (candidates.FirstOrDefault() is Entity target)
                        {
                            var toTake = Math.Min(target.StackSize, remaining);
                            return new Plan(PlanDefOf.GoHaul, target, toTake);
                        }
                    }
                    var pos = reservableTargets[0];
                    //return new Plan(PlanDefOf.GoPlace, map, pos, 1);
                    return new Plan(PlanDefOf.Plant, map, pos, 1) { Zone = zone};
                }
            }
            var allValidEntities = mapItems.Where(c => 
                c.Def == ItemDefOf.Seeds && 
                c.Profile is PlantSpeciesDef && 
                actor.CanReachAndReserve(c));
            var seedGroups = allValidEntities.GroupBy(c => c.Profile as PlantSpeciesDef);
            foreach(var species in seedGroups)
            {
                var batches = GetReachableBatches(actor, manager.GetSowingTargetsAll(species.Key));
                if (!batches.Any())
                    continue;
                foreach (var (targets, zone) in batches)
                {
                    foreach(var pos in targets)
                    {
                        if (!actor.CanReserve(pos))
                            continue;
                        var item = species.First();
                        var total = Math.Min(item.StackSize, targets.Count()); // if the actor can reach one position,
                                                                               // it implies he can reach all because zone cells are adjacent
                        return new Plan(PlanDefOf.GoHaul, item, total);
                    }
                }
            }
            return null;
        }
        static IEnumerable<SowingBatch> GetReachableBatches(Actor actor, IEnumerable<SowingBatch> batches)
        {
            foreach (var batch in batches)
            {
                if (!actor.CanReach(batch.Zone))
                    continue;
                yield return batch;
            }
        }
        //protected Plan TryPlanOld(Actor actor)
        //{
        //    var map = actor.Map;
        //    if (!actor.HasJob(JobDefOf.Farmer))
        //        return null;
        //    // TODO: iterate through all zones until one with an available seed type is found

        //    var zones = map.Town.ZoneManager.GetZones<GrowingZone>();
        //    foreach (var zone in zones)
        //    {
        //        var plant = zone.Plant;
        //        if (plant == null)
        //            continue;
        //        if (!zone.Planting)
        //            continue;
        //        var allLocs = zone.GetSowingPositions(plant.PlantingSpacing);
        //        if (!allLocs.Any())
        //            continue;
        //        if (!actor.CanReach(allLocs.First()))
        //            continue;

        //        var allSowablePositions = allLocs.Where(g => actor.CanReserve(g));
        //        if (!allSowablePositions.Any())
        //            continue;

        //        var allRelevantSeeds = actor.Map.GetEntities().Where(c => c.Profile == plant);// && actor.CanReserve(c));//).OrderByReachableRegionDistance(actor);

        //        var encumberanceLimit = actor.MaxCarryable(ItemDefOf.Seeds);

        //        var (sources, destinations) = Distribute(actor, encumberanceLimit, allRelevantSeeds, allSowablePositions.Select(p => new TargetArgs(actor.Map, p)), t => 1);

        //        if(sources.Count == 0 || destinations.Count == 0)
        //            continue;

        //        var task = new Plan(PlanDefOf.Sowing);
        //        task.AddTargets(TaskBehaviorDeliverMaterials.MaterialID, sources);
        //        task.AddTargets(TaskBehaviorDeliverMaterials.DestinationID, destinations);

        //        return task;
        //    }
        //    return null;
        //}
        //(List<(TargetArgs source, int amount)> sources, List<(TargetArgs destination, int amount)> destinations) Distribute(Actor actor, int maxAmount, IEnumerable<GameObject> sources, IEnumerable<TargetArgs> destinations, Func<TargetArgs, int> targetAmountGetter)
        //{
        //    List<(TargetArgs source, int amount)> sourcesAmounts = [];
        //    List<(TargetArgs destination, int amount)> destinationsAmounts = [];

        //    var enumSources = sources.GetEnumerator();
        //    var enumTargets = destinations.GetEnumerator();
        //    var remainingTotal = maxAmount;
        //    while (remainingTotal > 0 && enumSources.MoveNext())
        //    {
        //        var count = 0;
        //        var currentSource = enumSources.Current;
        //        var unreservedAmount = actor.GetUnreservedAmount(currentSource);
        //        var remainingFromCurrentStack = Math.Min(remainingTotal, unreservedAmount);// currentSource.StackSize);
        //        while (remainingFromCurrentStack > 0 && enumTargets.MoveNext())
        //        {
        //            var currentDest = enumTargets.Current;
        //            var idealAmountToDistribute = targetAmountGetter(currentDest);
        //            var actualAmountToDistribute = Math.Min(idealAmountToDistribute, remainingFromCurrentStack);
        //            remainingFromCurrentStack -= actualAmountToDistribute;
        //            remainingTotal -= actualAmountToDistribute;
        //            count += actualAmountToDistribute;
        //            destinationsAmounts.Add((currentDest, actualAmountToDistribute));
        //        }
        //        sourcesAmounts.Add((currentSource, count));
        //    }
        //    return (sourcesAmounts, destinationsAmounts);
        //}
    }
}
