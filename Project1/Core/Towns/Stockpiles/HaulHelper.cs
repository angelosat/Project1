using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework;

namespace Project1.Core.Towns.Stockpiles
{
    static class HaulHelper
    {
        public static bool IsValidStorage(this TargetArgs storage, MapBase map, GameObject item)
        {
            return StockpileAIHelper.IsValidStorage(item, storage);
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
       
        
        public static int MaxCarryable(this Actor actor, ItemDef def)
        {
            return actor.GetHaulStackLimitFromEndurance(def);
        }
    }
}
