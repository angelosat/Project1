using Project1.Core.Blocks;
using Project1.Core.Construction;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core
{
    sealed class BlockWorkstation : Block//WithEntity
    {
        readonly AtlasDepthNormals.Node.Token[] Orientations = Block.TexturesCounter;
        public BlockWorkstation() : base("Workstation", opaque: false, solid: true)
        {
            this.HidingAdjacent = false;
            this.Variations.Add(this.Orientations.First());
            this.BuildProperties.Category = ConstructionCategoryDefOf.Production;
            this.BuildProperties.Dimension = 4;
        }
        public BlockWorkstation(string name)
            : base(name, opaque: false, solid: true)
        {
            this.HidingAdjacent = false;
            this.Variations.Add(this.Orientations.First());
            this.BuildProperties.Category = ConstructionCategoryDefOf.Production;
            this.BuildProperties.Dimension = 4;
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Orientations[orientation];
        }
        internal override BlockEntity TryLinkToAdjacentBlockEntity(MapBase map, IntVec3 global)
        {
            var neighbors = new List<BlockEntity>();
            var workstationType = this.BlockDef.BlockEntityCompSpecs.OfType<BlockWorkstationComp.Spec>().SingleOrDefault()?.WorkstationType;
            
            foreach (var dir in IntVec3.AdjacentIntVec3)
            {
                var neighborPos = global + dir;
                if (map.TryGetBlockEntity(neighborPos, out var neighbor))
                {
                    if (neighbor.GetComp<BlockWorkstationComp>()?.WorkstationType == workstationType)
                        neighbors.Add(neighbor);
                }
            }

            //if (neighbors.Count == 0)
            //    return null;
            //else
            //{
            // find the first neighbor that hasn't reach max number of modules
            var maxModules = this.BlockDef.GetSpec<BlockWorkstationComp.Spec>().WorkstationType.MaxModules;

            foreach (var n in neighbors)
                {
                    var moduleCount = n.CellsOccupied.Count;
                    if (moduleCount < maxModules)
                    {
                        n.Attach(global);
                        return n;
                    }
                }

                //// Neighbor(s) exist: expand the first neighbor's entity
                //var primaryEntity = neighbors[0];   // pick one neighbor as the authoritative entity
                //primaryEntity.Attach(global);
                //return primaryEntity;// return primaryEntity;
            //}
            return null;
        }
        public override BlockEntity GetBlockEntityOrNew(MapBase map, IntVec3 originGlobal, BlockComp.Spec args)
        {
            // Find all adjacent existing workstation block entities
            var neighbors = new List<BlockEntity>();
            var typedArgs = (BlockWorkstationComp.Spec)args ?? new BlockWorkstationComp.Spec(WorkstationDefOf.Smeltery); // HACK
            foreach (var dir in IntVec3.AdjacentIntVec3)
            {
                var neighborPos = originGlobal + dir;
                if (map.TryGetBlockEntity(neighborPos, out var neighbor))
                {
                    if(neighbor.GetComp<BlockWorkstationComp>()?.WorkstationType == typedArgs.WorkstationType)
                        neighbors.Add(neighbor);
                }
            }

            if (neighbors.Count == 0)
            {
                // No neighbors: create a new block entity for this block
                var entity = this.BlockDef.CreateEntity(originGlobal);
                return entity;
            }
            else
            {
                // Neighbor(s) exist: expand the first neighbor's entity
                var primaryEntity = neighbors[0];   // pick one neighbor as the authoritative entity
                primaryEntity.CellsOccupied.Add(originGlobal); // add this new cell to its linked modules
                return primaryEntity;
            }
        }
        protected override IEnumerable<IntVec3> GetInteractionSpotsLocal()
        {
            yield return Cell.FrontDefault;
        }
        protected override IEnumerable<IntVec3> GetInteractionSpotsLocal(MapBase map, IntVec3 global)
        {
            var entity = map.GetBlockEntity(global);
            if(entity.OriginGlobal == global)
                yield return Cell.FrontDefault;
            yield break;
        }
        
    }
}
