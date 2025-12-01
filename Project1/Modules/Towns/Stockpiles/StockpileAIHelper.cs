using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class StockpileAIHelper
    {
        public static AITask FindBestTask(Actor actor, IEnumerable<Entity> items)
        {
            foreach (var i in items)
                if (TryHaulNew(actor, i) is AITask task)
                    return task;
            return null;
        }

        public static AITask TryHaulNew(Actor actor, Entity item, bool ignoreOtherReservations = false)
        {
            var unreservedAmount = actor.GetUnreservedAmount(item);
            if (unreservedAmount == 0)
                return null;
            var targets = StockpileAIHelper.GetAllValidStoragePlacesNoReserveCheckLazy(actor, item);
            if (!targets.Any())
                return null;
            var enumerator = targets.GetEnumerator();
            enumerator.MoveNext();
            TargetArgs bestTarget = enumerator.Current;
            int bestTargetUnreservedAmount = actor.GetUnreservedAmount(bestTarget.Global);
            while (bestTargetUnreservedAmount == 0 && enumerator.MoveNext())
            {
                bestTarget = enumerator.Current;
                bestTargetUnreservedAmount = actor.GetUnreservedAmount(bestTarget);
            }
            if (bestTarget == null)
                return null;
            if (bestTargetUnreservedAmount == 0)
                return null;
            var maxweight = Math.Min(item.StackMax, actor.GetHaulStackLimitFromEndurance(item));
            var maxamount = Math.Min(maxweight, unreservedAmount);
            var task = new AITask(TaskDefOf.Hauling, new TargetArgs(item), bestTarget) { Count = maxamount };
            if (bestTarget.HasObject)
                task.Count = Math.Min(task.Count, bestTarget.Object.StackAvailableSpace);

            if (task.Count < 0)
                throw new Exception();
            while (enumerator.MoveNext() && 0 < task.Count && task.Count < item.StackMax)
            {
                var current = enumerator.Current;
                if (current.HasObject && current.Object != item)
                {
                    task.Count = Math.Min(item.StackMax, task.Count + item.StackMax - current.Object.StackSize);
                    if (task.Count < 0)
                        throw new Exception();
                }
                else
                    task.Count = item.StackMax;
            }

            if (task.Count < 0)
                throw new Exception();
            if (task.Count == 0)
                return null;
            return task;

        }

        public static Dictionary<TargetArgs, int> GetAllValidHaulDestinations(Actor actor, GameObject item, out int maxamount)
        {
            var all = new Dictionary<TargetArgs, int>();
            var stockpiles = GetStoragesByPriority(item.Map);
            maxamount = 0;

            foreach (var s in stockpiles)
            {
                if (s.Settings.Accepts(item as Entity))
                    foreach (var spot in s.GetPotentialHaulTargets(actor, item, out maxamount))
                        all.Add(spot.Key, spot.Value);
            }
            return all;
        }
        public static TargetArgs GetBestHaulDestination(Actor actor, GameObject item)
        {
            var all = GetAllValidHaulDestinations(actor, item, out int maxAmount).Keys.Select(k => new TargetArgs(actor.Map, k.Global + Vector3.UnitZ));
            var closest = all.OrderBy(t => Vector3.DistanceSquared(t.Global, actor.Global)).FirstOrDefault();
            return closest ?? TargetArgs.Null;
        }

        public static bool IsItemAtBestStockpile(GameObject item)
        {
            var stockpiles = item.Map.Town.ZoneManager.GetZones<Stockpile>();
            var currentStockpile = stockpiles.FirstOrDefault(s => s.Contains(item));
            if (currentStockpile == null)
                return false;
            var betterStockpile = stockpiles
                .Where(s =>
                    s != currentStockpile &&
                    s.CanAccept(item) &&
                    s.Priority > currentStockpile.Priority)
                .OrderByDescending(s => s.Priority)
                .FirstOrDefault();
            return betterStockpile == null && currentStockpile.Accepts(item);
        }

        public static IEnumerable<IStorageNew> GetStoragesByPriority(MapBase map)
        {
            IEnumerable<Stockpile> stockpiles = GetStockpiles(map);
            return stockpiles.OrderByDescending(i => i.Priority);
        }

        public static IEnumerable<Stockpile> GetStockpiles(MapBase map)
        {
            return map.Town.ZoneManager.GetZones<Stockpile>();
        }

        public static IEnumerable<TargetArgs> GetAllValidStoragePlacesNoReserveCheckLazy(Actor actor, Entity item)
        {
            var storages = GetStoragesByPriority(actor.Map);
            foreach (var s in storages)
            {
                if (s.Settings.Accepts(item))
                    foreach (var spot in s.GetPotentialHaulTargets(actor, item))
                        yield return spot;
            }
        }

        public static bool IsValidStorage(GameObject item, TargetArgs destination)
        {
            if (destination.HasObject && (destination.Object == null || !destination.Object.IsSpawned || destination.Object.IsStackFull))
                return false;
            var global = (IntVec3)destination.Global;
            var below = global.Below;
            var targetStockpile = destination.Map.Town.ZoneManager.GetZoneAt<Stockpile>(below);
            if (targetStockpile == null)
                return false;
            return targetStockpile.IsValidStorage(item as Entity, below.At(item.Map), item.StackSize);
        }
    }
}
