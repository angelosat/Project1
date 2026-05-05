using Microsoft.Xna.Framework;
using Project1.Core.Graphics;
using Project1.Core.Graphics.Particles;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using Project1.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Project1.Core.Blocks;

class BlockGrass : Block
{
    public override bool IsMinable => true;
    public override Color DirtColor => Color.DarkOliveGreen;
    public override ParticleEmitterSphere GetEmitter()
    {
        return base.GetDirtEmitter();
    }

    readonly List<AtlasDepthNormals.Node.Token> Overlays = new(3);
    public static List<AtlasDepthNormals.Node.Token> FlowerOverlays = new();

    public static readonly double TramplingChance = 0.1f;

    public BlockGrass()
        : base("Grass", 0, 1, true, true)
    {
        this.BreakProduct = RawMaterialDefOf.Bags;

        this.LoadVariations("grass/grass1", "grass/grass2", "grass/grass3", "grass/grass4");

        foreach (var item in new AtlasDepthNormals.Node.Token[] {
            Atlas.Load("blocks/grass/grass1-overlay", BlockDepthMap, BlockMouseMap.Texture),
            Atlas.Load("blocks/grass/grass2-overlay", BlockDepthMap, BlockMouseMap.Texture),
            Atlas.Load("blocks/grass/grass3-overlay", BlockDepthMap, BlockMouseMap.Texture)})
            this.Overlays.Add(item);

        FlowerOverlays.Add(Atlas.Load("blocks/grass/flowersoverlayred", BlockDepthMap, NormalMap));
        FlowerOverlays.Add(Atlas.Load("blocks/grass/flowersoverlayyellow", BlockDepthMap, NormalMap));
        FlowerOverlays.Add(Atlas.Load("blocks/grass/flowersoverlaywhite", BlockDepthMap, NormalMap));
        FlowerOverlays.Add(Atlas.Load("blocks/grass/flowersoverlaypurple", BlockDepthMap, NormalMap));
        this.DrawMaterialColor = false;
    }

    internal override void OnPlaced(MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
    {
        var query = new MapQuery(map, global);
        var cellquery = query.CellQuery;
        cellquery.Variation = map.Random.Next(this.Variations.Count);
    }
    Random _rand = new();
    internal override void OnPlaced(CellQuery cellQuery)
    {
        cellQuery.Variation = _rand.Next(this.Variations.Count);
    }
    internal static void GrowRandomFlower(MapBase map, IntVec3 global)
    {
        var net = map.Net;
        if (net is Client)
            throw new Exception();
        byte data = (byte)(map.Random.Next(FlowerOverlays.Count) + 1);
        map.SyncSetCellData(global, data);
    }
    internal static byte GetRandomFlower(MapBase map) => (byte)(map.Random.Next(FlowerOverlays.Count) + 1);
    public override byte ParseData(string data)
    {
        return byte.Parse(data);
    }

    AtlasDepthNormals.Node.Token GetFlowerOverlay(byte data)
    {
        var flowerIndex = data - 1; //because 0 is no flowers
        return FlowerOverlays[flowerIndex];
    }
    internal bool HasFlower(byte data)
        => data > 0;
    public override IEnumerable<MaterialDef> GetEditorVariations()
    {
        yield return MaterialDefOf.Human;
    }
    
    internal override float GetFertility(Cell cell)
    {
        if (cell.BlockData > 0) // if there are flowers grown, dont grow anything else (return fertility = 0)
            return 0;
        return base.GetFertility(cell);
    }
   
    public override MyVertex[] Draw(Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
    {
        return base.Draw(chunk, global, camera, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data, mat);
    }
    public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, MapView view, Vector4 screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
    {
        base.Draw(canvas, chunk, global, view, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data, mat);
        if (data == 0)
            return null;
        var fl = this.GetFlowerOverlay(data);
        return canvas.Opaque.DrawBlock(fl.Atlas.Texture, screenBounds, fl, view.Zoom, fog, tint, Color.White, sunlight, blocklight, Vector4.Zero, depth, this, global);
    }
}
