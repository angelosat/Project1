using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Materials;
using Project1.Core.Towns.Constructions.Categories;
using Project1.Core.Blocks;
using Project1.Core.Legacy;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Loot;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core
{
    partial class BlockCampfire : BlockWithEntity
    {
        public BlockCampfire()
            : base("Campfire", opaque: false, solid: false)
        {
            this.HidingAdjacent = false;
            this.BuildProperties = new BuildProperties(new Ingredient(item: RawMaterialDefOf.Logs), 0);
            this.Variations.Add(Block.Atlas.Load("blocks/campfire", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap));
            this.BuildProperties.Complexity = 2;
            this.BuildProperties.Category = ConstructionCategoryDefOf.Production;
            this.Ingredient = new Ingredient().SetAllow(RawMaterialDefOf.Logs, true);
        }
        public override LootTable GetLootTable(byte data)
        {
            var table =
                new LootTable(
                    new LootWrapper(a => ItemFactory.CreateFrom(RawMaterialDefOf.Logs, MaterialDefOf.Human)) // TODO
                    );
            return table;
        }
        public override BlockEntity GetBlockEntityOrNew(MapBase map, IntVec3 originGlobal, BlockEntityComp.Spec args)
        {
            return new BlockCampfireEntity(this.BlockDef, originGlobal);
        }

        internal override void OnPlaced(MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
        {
            if (!map.GetBlock(global - IntVec3.UnitZ).Opaque)
                return;
            base.OnPlaced(map, global, material, data, variation, orientation, notify);
        }
        public override bool IsRoomBorder => false;
        public override bool IsDeconstructible => true;
        protected override void OnDeconstruct(GameObject actor, Vector3 global)
        {
        }
        protected override void OnBlockBelowChanged(MapBase map, IntVec3 global)
        {
            map.GetBlock(global.Below, out var cell);
            if (cell.Block == BlockDefOf.Air.Worker)
                map.RemoveBlock(global);
        }
    }
}
