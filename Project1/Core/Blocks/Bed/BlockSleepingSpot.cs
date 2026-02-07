using Project1.Core.Rooms;
using Project1.Core.Towns;
using Project1.Core.Towns.Constructions.Categories;
using Project1.Core.Base;
using Project1.Core.Blocks;
using Project1.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this.UtilitiesProvided.Add(Utility.Types.Sleeping);
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
        public override BlockEntity GetBlockEntityOrNew(MapBase map, IntVec3 originGlobal, BlockEntityComp.Spec args)
        {
            return new BlockBedEntity(this.BlockDef, originGlobal);
        }
    }
}
