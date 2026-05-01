using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Simulation;
using Project1.Core.Systems.Materials;
using Project1.Framework;

namespace Project1.Core.Blocks;

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
   
    public override BlockEntity GetBlockEntityOrNew(MapBase map, IntVec3 originGlobal, BlockComp.Spec args)
    {
        return new BlockDesignationEntity(this.BlockDef, originGlobal);
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

}
