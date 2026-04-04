using Project1.Core.Rooms;
using Project1.Core.Towns;
using Project1.Core.Blocks;
using Project1.Core.Simulation;
using System.Collections.Generic;
using Project1.Framework;
using Project1.Core.Construction;

namespace Project1.Core
{
    class BlockSleepingSpot : Block
    {
        public BlockSleepingSpot()
            : base("SleepingSpot", transparency: 1, density: 0, opaque: false, solid: false)
        {
            this.HidingAdjacent = false;
            this.Furniture = FurnitureDefOf.Bed;
            this.BuildProperties.Category = ConstructionCategoryDefOf.Furniture;
            this.Variations.Add(Block.FaceHighlights[-IntVec3.UnitZ]);
            this.RequiresConstruction = false;
            this.DrawMaterialColor = false;
        }

        protected override IEnumerable<IntVec3> GetInteractionSpotsLocal()
        {
            yield return IntVec3.Zero;
        }
        public override bool IsRoomBorder => false;
        public override bool IsStandableOn => false;
        public override float GetHeight(byte data, float x, float y)
        {
            return 0;
        }
        public override float GetHeight(float x, float y)
        {
            return 0;
        }
        public override BlockEntity GetBlockEntityOrNew(MapBase map, IntVec3 originGlobal, BlockComp.Spec args)
        {
            return new BlockBedEntity(this.BlockDef, originGlobal);
        }
    }
}
