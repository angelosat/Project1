using Project1.Core.Blocks;
using Project1.Core.Blocks.Construction;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Loot;
using Project1.Core.Materials;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

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
            //MapEdit
            //    .Begin(map, MapEditContext.Simulation)
            //    .Erase([global])
            //    .Flush();
            MapEdit.Paint(MapEditContext.Simulation, map, [global], BlockDefOf.Air.Block, MaterialDefOf.Air, 0, 0, 0);
        }
        public static void DeconstructBlock(MapBase map, IntVec3 global)
        {
            var query = map.Query(global);
            var cell = query.Cell;
            var block = cell.Block;
            List<Entity> loot = [];
            //if (block.BlockDef.ConstructionProfile is ConstructionProfile constrProf)
            //{
            //    //foreach (var refinement in constrProf.Refinements)
            //    //    loot.Add(EntityFactory.Create(refinement, null, cell.Material));
            //    loot.Add(RawMaterialSystem.Create(cell.Material));
            //    map.Events.Post(new LootDropEvent([.. loot], map, global));
            //}
            if (query.BlockEntity?.GetComp<BlockBuildingComp>() is BlockBuildingComp comp)
            {
                if (comp.IngredientUsed is MaterialRefinementDef matRef)
                {
                    var refund = RawMaterialSystem.Create(matRef, cell.Material);
                    map.Events.Post(new LootDropEvent([refund], map, global));
                }
            }
            else if (block.BlockDef.ConstructionProfile is ConstructionProfile constrProf)
            {
                //foreach (var refinement in constrProf.Refinements)
                //    loot.Add(EntityFactory.Create(refinement, null, cell.Material));
                loot.Add(RawMaterialSystem.Create(constrProf.Refinements.Single(), cell.Material));
                map.Events.Post(new LootDropEvent([.. loot], map, global));
            }
            //MapEdit
            //    .Begin(map, MapEditContext.Simulation)
            //    .Erase([global])
            //    .Flush();
            MapEdit.Paint(MapEditContext.Simulation, map, [global], BlockDefOf.Air.Block, MaterialDefOf.Air, 0, 0, 0);

        }
    }
}
