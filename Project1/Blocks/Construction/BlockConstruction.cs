using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;
using Start_a_Town_.Blocks;

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
            var entity = chunk.Map.GetBlockEntity(global) as BlockConstructionEntity;
            var block = entity.Product.Block;
            
            AtlasDepthNormals.Node.Token token;
                token = block.GetToken(variation, orientation, (int)camera.Rotation, data);

            var color = Color.White;
            return canvas.Designations.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, color, sunlight, blocklight, depth, this, global);
        }
        internal override void PreRemove(MapBase map, IntVec3 global)
        {
            var entity = map.GetBlockEntity(global) as BlockConstructionEntity;
            foreach (var mat in entity.Container)
            {
                var remaining = mat.Amount;
                while (remaining > 0)
                {
                    var obj = this.Ingredient.ItemDef.Create();
                    obj.StackSize = Math.Min(obj.StackMax, remaining);
                    remaining -= obj.StackSize;
                    map.Net.PopLoot(obj, global, Vector3.Zero);
                }
            }
        }
        internal override string GetName(MapBase map, IntVec3 global)
        {
            return map.GetBlockEntity<BlockConstructionEntity>(global).Product.Block.Name + " (Construction)";
        }
        internal override bool IsValidHaulDestination(MapBase map, IntVec3 global, GameObject obj)
        {
            var entity = map.GetBlockEntity(global) as BlockConstructionEntity;
            return entity.IsValidHaulDestination(obj.Def);
        }
        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            amount = amount < 0 ? dropped.StackSize : amount;
            var e = target.GetBlockEntity<BlockConstructionEntity>();
            e.HandleDepositedItem(dropped, amount);
        }
        public override BlockEntity CreateBlockEntity(IntVec3 originGlobal)
        {
            return new BlockConstructionEntity(originGlobal);
        }
    }
}
