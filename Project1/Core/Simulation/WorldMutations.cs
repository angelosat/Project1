using Project1.Core.Entities;
using Project1.Core.Input;
using Project1.Core.Loot;

namespace Project1.Core.Simulation
{
    public static class WorldMutations
    {
        public static void BreakBlock(CellSelection cell)
        {
            var block = cell.Block;
            var map = cell.Map;
            var global = cell.Global;
            if (block.BlockDef.BreakProduct is Def breakProductProfile)
            {
                var entity = EntityFactory.Create(breakProductProfile, null, cell.Cell.Material);
                map.Events.Post(new LootDropEvent([entity], map, global));
            }
            MapEdit
                .Begin(map)
                .Erase([global])
                .Flush();
        }
    }
}
