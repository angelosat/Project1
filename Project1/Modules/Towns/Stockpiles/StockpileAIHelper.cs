using Project1.Framework.Base;

namespace Start_a_Town_
{
    class StockpileAIHelper
    {
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
