using SharpDX.MediaFoundation;
using Start_a_Town_.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public override BlockEntity CreateBlockEntity(MapBase map, IntVec3 originGlobal)
        {
            // Find all adjacent existing workstation block entities
            var neighbors = new List<BlockWorkbenchEntity>();
            foreach (var dir in IntVec3.AdjacentIntVec3)
            {
                var neighborPos = originGlobal + dir;
                if (map.TryGetBlockEntity<BlockWorkbenchEntity>(neighborPos, out var neighbor))
                    neighbors.Add(neighbor);
            }

            if (neighbors.Count == 0)
            {
                // No neighbors: create a new block entity for this block
                var entity = Activator.CreateInstance(this.BlockEntityType, originGlobal) as BlockWorkbenchEntity;
                entity.CellsOccupied.Add(originGlobal); // register this cell
                return entity;
            }
            else
            {
                // Neighbor(s) exist: expand the first neighbor's entity
                var primaryEntity = neighbors[0];   // pick one neighbor as the authoritative entity
                primaryEntity.CellsOccupied.Add(originGlobal); // add this new cell to its linked modules
                return primaryEntity;// return primaryEntity;
            }
        }
        protected override IEnumerable<IntVec3> GetInteractionSpotsLocal()//int orientation)
        {
            yield return Cell.FrontDefault;
        }
        protected override IEnumerable<IntVec3> GetInteractionSpotsLocal(MapBase map, IntVec3 global)//int orientation)
        {
            //var masterCell = map.GetBlockEntityComp<BlockEntityCompWorkstation>(global).MasterCell;
            //if(masterCell == IntVec3.Zero)
            //    yield return Cell.FrontDefault;
            var entity = map.GetBlockEntity<BlockWorkbenchEntity>(global);
            if(entity.OriginGlobal == global)
                yield return Cell.FrontDefault;

            //if (global != masterCell)
            yield break;
        }
        
    }
}
