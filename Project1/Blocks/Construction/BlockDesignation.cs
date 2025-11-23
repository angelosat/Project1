using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    partial class BlockDesignation : Block
    {
        public BlockDesignation()
            : base("Designation", 1, 0, false, false)
        {
            this.HidingAdjacent = false;
            this.Variations.Add(Atlas.Load("blocks/blockblueprint"));
            this.DrawMaterialColor = false;
        }

        public override bool IsStandableIn => true;
       
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            var token = this.Variations[0];
            var color = Color.White;
            return canvas.Designations.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, color, sunlight, blocklight, depth, this, global);
        }
       
        public override BlockEntity CreateBlockEntity(IntVec3 originGlobal)
        {
            return new BlockDesignationEntity(originGlobal);
        }
        
        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            var des = actor.Map.GetBlockEntity(target.Global) as BlockDesignationEntity;
            var product = des.Product;
            var global = target.Global;
            amount = amount == -1 ? dropped.StackSize : amount;
            BlockConstructionEntity constr = new(product, global, dropped, amount);
            constr.Children = des.Children;
            var map = actor.Map;
            var positions = des.CellsOccupied;
            bool requiresConstruction = product.Block.RequiresConstruction;
            var isReady = constr.IsReadyToBuild(out _, out _, out _);

            // TODO instead of doing this here, use a function in BlockConstructionEntity
            if (requiresConstruction)
            {
                foreach (var p in positions)
                {
                    map.AddBlockEntity(p, constr);
                    var pcell = map.GetCell(p);
                    map.SetBlock(p, BlockDefOf.Construction, product.Material, pcell.BlockData, pcell.Variation, pcell.Orientation, false);
                }
                map.NotifyBlocksChanged(positions);
            }
            else if(isReady)
            {
                foreach (var p in positions)
                    map.RemoveBlock(p, false);
                var block = product.Block;
                var cell = map.GetCell(global);
                Block.Place(block, map, global, product.Material, product.Data, 0, cell.Orientation, true);
                map.GetBlockEntity(global)?.IsMadeFrom(new ItemMaterialAmount[] { product.Requirement });
            }
           
            if (amount == -1)
                throw new Exception();
            dropped.StackSize -= amount;
        }
        
        internal override bool IsValidHaulDestination(MapBase map, IntVec3 global, GameObject obj)
        {
            var entity = map.GetBlockEntity(global) as BlockDesignationEntity;
            return entity.IsValidHaulDestination(obj.Def);
        }
       
        internal override string GetName(MapBase map, IntVec3 global)
        {
            var e = map.GetBlockEntity<BlockDesignationEntity>(global);
            return $"{e.Product.Block.Name} (Designation)";
        }

        public static void Place(MapBase map, IntVec3 global, byte data, int variation, int orientation, ProductMaterialPair product)
        {
            var entity = new BlockDesignation.BlockDesignationEntity(product, global);
            bool ismulti = product.Block.Multi;

            // LATEST DECISION: add the same entity to all occupied cells
            // NOT FOR BLOCKDESIGNATION because i add every entity and child entities should have their origin field set
            if (ismulti)
            {
                //var parts = product.Block.GetParts(global, orientation);
                var parts = product.Block.GetChildrenWithSource(orientation).Select(c => (global + c.local, c.source));
                foreach (var p in parts)
                {
                    var pos = p.Item1;
                    var source = p.Item2;
                    map.AddBlockEntity(pos, entity);// DIDNT I DECIDE THAT BLOCKENTITIES WILL BE PLACE ONLY IN THE ORIGIN CELL???
                    entity.Children.Add(pos);
                    map.SetBlock(pos, BlockDefOf.Designation, MaterialDefOf.Air, 0, source, variation, orientation, false);
                }
            }
            else
            {
                map.AddBlockEntity(global, entity);// DIDNT I DECIDE THAT BLOCKENTITIES WILL BE PLACE ONLY IN THE ORIGIN CELL???
                entity.Children.Add(global);
                map.SetBlock(global, BlockDefOf.Designation, MaterialDefOf.Air, data, variation, orientation, false); // i put this last because there are blockchanged event handlers that look up the block entity which hadn't beeen added yet when I set the block beforehand
            }
        }
    }
}
