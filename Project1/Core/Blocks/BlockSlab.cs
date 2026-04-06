using Project1.Core.Construction;
using Project1.Core.Graphics;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Simulation;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Blocks;

class BlockWaypoint : Block
{
    public BlockWaypoint()
        : base("Waypoint", opaque: false)
    {
        this.HidingAdjacent = false;
        var txt = Block.Atlas.Load("blocks/slab", Block.QuarterBlockMapDepth, Block.QuarterBlockMapNormal);
        this.Variations.Add(txt);
        this.Ingredient = new Ingredient(RawMaterialDefOf.Ingots, null, null, 1);
        this.BuildProperties.Category = ConstructionCategoryDefOf.Structural;
    }
    internal override (bool result, string error) IsAvailable(MapBase map)
        => map.Town.Waypoint.HasValue ? (false, "Waypoint already exists") : (true, null);
    public override float GetPathingCost(byte data)
    {
        return 0;
    }
    public override Microsoft.Xna.Framework.Color[] UV
    {
        get
        {
            return Block.BlockCoordinatesQuarter;
        }
    }
    public override MouseMap MouseMap
    {
        get
        {
            return Block.BlockQuarterMouseMap;
        }
    }
    public override float GetHeight(byte data, float x, float y)
    {
        return this.GetHeight(x, y);
    }
    public override float GetHeight(float x, float y)
    {
        return .25f;
    }
    public override IEnumerable<MaterialDef> GetEditorVariations()
    {
        yield return MaterialDefOf.Stone;
    }
}
class BlockSlab : Block
{
    public BlockSlab()
        : base("Slab", opaque: false)
    {
        this.HidingAdjacent = false;
        var txt = Block.Atlas.Load("blocks/slab", Block.QuarterBlockMapDepth, Block.QuarterBlockMapNormal);
        this.Variations.Add(txt);
        this.Ingredient = new Ingredient(RawMaterialDefOf.Ingots, null, null, 1);
        this.BuildProperties.Category = ConstructionCategoryDefOf.Structural;
    }
    public override float GetPathingCost(byte data)
    {
        return 0;
    }
    public override Microsoft.Xna.Framework.Color[] UV
    {
        get
        {
            return Block.BlockCoordinatesQuarter;
        }
    }
    public override MouseMap MouseMap
    {
        get
        {
            return Block.BlockQuarterMouseMap;
        }
    }
    public override float GetHeight(byte data, float x, float y)
    {
        return this.GetHeight(x, y);
    }
    public override float GetHeight(float x, float y)
    {
        return .25f;
    }
    public override IEnumerable<MaterialDef> GetEditorVariations()
    {
        yield return MaterialDefOf.Stone;
    }
}
