using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverPlanting : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var map = actor.Map;
            if (!actor.HasJob(JobDefOf.Farmer))
                return null;
            // TODO: iterate through all zones until one with an available seed type is found

            var zones = map.Town.ZoneManager.GetZones<GrowingZone>();
            foreach (var zone in zones)
            {
                var plant = zone.Plant;
                if (plant == null)
                    continue;
                if (!zone.Planting)
                    continue;
                var allLocs = zone.GetSowingPositions(plant.PlantingSpacing);
                if (!allLocs.Any())
                    continue;
                if (!actor.CanReach(allLocs.First()))
                    continue;

                var allSowablePositions = allLocs.Where(g => actor.CanReserve(g));
                if (!allSowablePositions.Any())
                    continue;

                var allseeds = actor.Map.GetObjectsLazy().Where(c => c.IsSeedFor(plant));// && actor.CanReserve(c));//
                                                                                                                //).OrderByReachableRegionDistance(actor);
                var encumberanceLimit = actor.MaxCarryable(ItemDefOf.Seeds);

                var (sources, destinations) = Distribute(actor, encumberanceLimit, allseeds, allSowablePositions.Select(p => new TargetArgs(actor.Map, p)), t => 1);

                if(!sources.Any() || !destinations.Any())
                    continue;

                var task = new AITask(TaskDefOf.Sowing);
                task.AddTargets(TaskBehaviorDeliverMaterials.MaterialID, sources);
                task.AddTargets(TaskBehaviorDeliverMaterials.DestinationID, destinations);

                return task;
            }
            return null;
        }
        (List<(TargetArgs source, int amount)> sources, List<(TargetArgs destination, int amount)> destinations) Distribute(Actor actor, int maxAmount, IEnumerable<GameObject> sources, IEnumerable<TargetArgs> destinations, Func<TargetArgs, int> targetAmountGetter)
        {
            List<(TargetArgs source, int amount)> sourcesAmounts = new();
            List<(TargetArgs destination, int amount)> destinationsAmounts = new();

            var enumSources = sources.GetEnumerator();
            var enumTargets = destinations.GetEnumerator();
            var remainingTotal = maxAmount;
            while (remainingTotal > 0 && enumSources.MoveNext())
            {
                var count = 0;
                var currentSource = enumSources.Current;
                var unreservedAmount = actor.GetUnreservedAmount(currentSource);
                var remainingFromCurrentStack = Math.Min(remainingTotal, unreservedAmount);// currentSource.StackSize);
                while (remainingFromCurrentStack > 0 && enumTargets.MoveNext())
                {
                    var currentDest = enumTargets.Current;
                    var idealAmountToDistribute = targetAmountGetter(currentDest);
                    var actualAmountToDistribute = Math.Min(idealAmountToDistribute, remainingFromCurrentStack);
                    remainingFromCurrentStack -= actualAmountToDistribute;
                    remainingTotal -= actualAmountToDistribute;
                    count += actualAmountToDistribute;
                    destinationsAmounts.Add((currentDest, actualAmountToDistribute));
                }
                sourcesAmounts.Add((currentSource, count));
            }
            return (sourcesAmounts, destinationsAmounts);
        }
    }
}
