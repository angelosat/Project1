using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    sealed class BlockWorkstation : BlockWithEntity, IBlockWorkstation
    {
        readonly AtlasDepthNormals.Node.Token[] Orientations = Block.TexturesCounter;
        readonly Type BlockEntityType;
        public override bool IsDeconstructible => true;

        public BlockWorkstation(string name, Type blockEntityType)
            : base(name, opaque: false, solid: true)
        {
            this.HidingAdjacent = false;
            this.BlockEntityType = blockEntityType;
            this.Variations.Add(this.Orientations.First());
            this.BuildProperties.Category = ConstructionCategoryDefOf.Production;
            this.BuildProperties.Dimension = 4;
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Orientations[orientation];
        }
        public override BlockEntity CreateBlockEntity(IntVec3 originGlobal)
        {
            return Activator.CreateInstance(this.BlockEntityType, originGlobal) as BlockEntity;
        }
        protected override IEnumerable<IntVec3> GetInteractionSpotsLocal()//int orientation)
        {
            yield return Cell.FrontDefault;
        }
    }
}
