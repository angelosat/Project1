using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    class BlockConstruction : BlockWithEntity
    {
        public BlockConstruction()
            : base("Construction", solid: false, opaque: false)
        {
            this.HidingAdjacent = false;
            this.Variations.Add(Block.Atlas.Load("blocks/blockblueprint"));
        }
        public override bool IsStandableIn => false;
       
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            //var entity = chunk.Map.GetBlockEntity(global) as BlockConstructionEntity;
            //var block = entity.Product.Block;
            var block = chunk.Map.GetBlockEntityComp<BlockConstructionComp>(global).Block;

            AtlasDepthNormals.Node.Token token;
                token = block.GetToken(variation, orientation, (int)camera.Rotation, data);

            var color = Color.White;
            //var targetMesh = canvas.Designations;
            var targetMesh = canvas.Opaque;
            //var targetMesh = canvas.Transparent;
            return targetMesh.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, color, sunlight, blocklight, depth, this, global);
        }
        //internal override void PreRemove(MapBase map, IntVec3 global)
        //{
        //    var entity = map.GetBlockEntity(global) as BlockConstructionEntity;
        //    foreach (var mat in entity.Container)
        //    {
        //        var remaining = mat.Amount;
        //        while (remaining > 0)
        //        {
        //            var amount = Math.Min(this.Ingredient.ItemDef.StackCapacity, remaining);
        //            var obj = this.Ingredient.ItemDef.Create(amount: amount);
        //            remaining -= amount;
        //            map.Net.PopLoot(obj, global, Vector3.Zero);
        //        }
        //    }
        //}
        //internal override string GetName(MapBase map, IntVec3 global)
        //{
        //    return map.GetBlockEntity<BlockConstructionEntity>(global).Product.Block.Name + " (Construction)";
        //}
        //internal override bool IsValidHaulDestination(MapBase map, IntVec3 global, GameObject obj)
        //{
        //    var entity = map.GetBlockEntity(global) as BlockConstructionEntity;
        //    return entity.IsValidHaulDestination(obj.Def);
        //}
        ////public override bool TryConsume(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        ////{
        ////    throw new Exception();
        ////    amount = amount < 0 ? dropped.StackSize : amount;
        ////    var e = target.GetBlockEntity<BlockConstructionEntity>();
        ////    e.HandleDepositedItem(dropped, amount);
        ////}
        //public override BlockEntity GetBlockEntityOrNew(MapBase map, IntVec3 originGlobal, BlockEntityComp.Spec args)
        //{
        //    return new BlockConstructionEntity(originGlobal);
        //}
    }
}
