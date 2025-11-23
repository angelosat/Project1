using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    static class HaulHelper
    {
        public static bool IsValidStorage(this TargetArgs storage, MapBase map, GameObject item)
        {
            return StockpileAIHelper.IsValidStorage(item, storage);
        }

        public static bool IsValidHaulDestination(this TargetArgs destination, MapBase map, GameObject item)
        {
            if (StockpileAIHelper.IsValidStorage(item, destination))
                return true;
            if (destination.Type == TargetType.Position &&
                (map.Town.ZoneManager.GetZoneAt<GrowingZone>(destination.Global)?.IsValidSeed(item) ?? false))
                return true;
            var block = map.GetBlock(destination.Global);
            if (block.IsValidHaulDestination(map, destination.Global, item))
                return true;
            return false;
        }
        public static bool IsValidHaulDestinationNew(this TargetArgs destination, MapBase map, GameObject item)
        {
            var pos = (IntVec3)destination.Global;
            return
                map.Town.GetZoneAt(destination.Global)?.Accepts(item as Entity, pos) ?? false ||
                map.GetBlock(destination.Global).IsValidHaulDestination(map, pos, item);

        }
        public static bool TryFindNearbyPlace(Actor actor, GameObject item, Vector3 center, out TargetArgs target)
        {
            var map = actor.Map;
            var actorCell = actor.Cell;
            var places = actorCell.GetRadial();
            foreach (var pl in places)
            {
                var global = pl;
                var above = global.Above();
                var existingItems = map.GetObjects(above);
                var toCombine = existingItems.FirstOrDefault(i => i != item && i.CanAbsorb(item));
                if (toCombine != null)
                {
                    target = new TargetArgs(toCombine);
                    return true;
                }

                var block = map.GetBlock(global);
                if (block.IsStandableOn &&
                    map.IsSolid(global) &&
                    map.IsEmpty(above))
                {
                    target = new TargetArgs(map, above);
                    return true;
                }
            }
            target = null;
            return false;
        }
        public static bool TryFindNearbyPlace(Actor actor, GameObject item, out TargetArgs target)
        {
            var map = actor.Map;
            var itemCell = item.Global.ToCell();
            var places = itemCell.GetRadial();
            foreach (var pl in places)
            {
                var global = pl;
                if (actor.Map.IsDesignation(global))
                    continue;
                var above = global.Above();
                var existingItems = map.GetObjects(above);
                var toCombine = existingItems.FirstOrDefault(i => i != item && i.CanAbsorb(item));
                if (toCombine != null)
                {
                    target = new TargetArgs(toCombine);
                    return true;
                }

                var block = map.GetBlock(global);
                if (block.IsStandableOn &&
                    map.IsSolid(global) &&
                    map.IsEmpty(above))
                {
                    target = new TargetArgs(map, above);
                    return true;
                }
            }
            target = null;
            return false;
        }
        public static IEnumerable<Entity> GetPotentialItemsNew(Actor actor, bool ignoreOtherReservations = false)
        {
            var objs = actor.Map.GetObjectsLazy();
            foreach (var obj in objs)
            {
                var item = obj as Entity;
                if (item == null)
                    continue;
                if (item.Physics.Size == ObjectSize.Immovable)
                    continue;
                if (!item.IsStockpilable())
                    continue;
                if (!actor.CanReach(item))
                    continue;
                if (!actor.CanReserve(item))
                    continue;
                if (StockpileAIHelper.IsItemAtBestStockpile(item))
                    continue;
                yield return item;
            }
        }

        public static int MaxCarryable(this Actor actor, Entity item)
        {
            return actor.MaxCarryable(item.Def);
        }
        public static int MaxCarryable(this Actor actor, ItemDef def)
        {
            return actor.GetHaulStackLimitFromEndurance(def);
        }
    }
}
