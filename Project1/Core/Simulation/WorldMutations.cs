using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Input;
using Project1.Core.Loot;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Simulation
{
    public static class WorldMutations
    {
        public static void BreakBlock(MapBase map, IntVec3 global)
        {
            var cell = map.GetCell(global);
            var block = cell.Block;
            if (block.BlockDef.BreakProduct is Def breakProductProfile)
            {
                var entity = EntityFactory.Create(breakProductProfile, null, cell.Material);
                map.Events.Post(new LootDropEvent([entity], map, global));
            }
            MapEdit
                .Begin(map)
                .Erase([global])
                .Flush();
        }
        public static void DeconstructBlock(MapBase map, IntVec3 global)
        {
            var cell = map.GetCell(global);
            var block = cell.Block;
            List<Entity> loot = [];
            if (block.BlockDef.ConstructionProfile is ConstructionProfile constrProf)
            {
                foreach (var refinement in constrProf.Refinements)
                    loot.Add(EntityFactory.Create(refinement, null, cell.Material));
                map.Events.Post(new LootDropEvent([..loot], map, global));
            }
            MapEdit
                .Begin(map)
                .Erase([global])
                .Flush();
        }
    }
}
