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
        public static bool IsValidStorage(this InteractionTarget storage, MapBase map, GameObject item)
        {
            return StockpileAIHelper.IsValidStorage(item, storage);
        }
        public static bool IsValidHaulDestinationNew(this InteractionTarget destination, MapBase map, GameObject item)
        {
            var pos = (IntVec3)destination.Global;
            return
                map.Town.GetZoneAt(destination.Global)?.Accepts(item as Entity, pos) ?? false ||
                map.GetBlock(destination.Global).IsValidHaulDestination(map, pos, item);

        }
    }
}
