using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Construction;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Graphics.Particles;
using Project1.Core.Helpers;
using Project1.Core.Interactions;
using Project1.Core.Legacy;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Loot;
using Project1.Core.Networking;
using Project1.Core.Rendering;
using Project1.Core.Rooms;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Systems.Materials;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Graphics;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Blocks;

[EnsureStaticCtorCall]
public abstract partial class Block : Inspectable, ISlottable, ITooltippable
{
    public BlockDef BlockDef;
    public BlockComp.Spec[] BlockEntityCompSpecs;
    static Block()
    {
        Initialize();
    }
    public class Data
    {
        public byte Value { get; set; }
        public Data(byte value = 0)
        {
            this.Value = value;
        }
    }

    static readonly Dictionary<Block, GameObject> BlockObjects = new();

    public static Color[] BlockCoordinatesFull,
        BlockCoordinatesHalf,
        BlockCoordinatesQuarter;

    public static AtlasDepthNormals.Node.Token BlockShadow;
    public static AtlasDepthNormals.Node.Token BlockBlueprint;
    public static AtlasDepthNormals.Node.Token BlockHighlight;
    public static AtlasDepthNormals.Node.Token BlockHighlightBack;
    public static AtlasDepthNormals.Node.Token BlockBlueprintGrayscale;
    
    public static void Initialize()
    {
        var arraySize = Width * Height;
        BlockCoordinatesFull = new Color[arraySize];
        Game1.Instance.Content.Load<Texture2D>("Graphics/goodUV").GetData(BlockCoordinatesFull, 0, arraySize);
        BlockCoordinatesHalf = new Color[arraySize];
        Game1.Instance.Content.Load<Texture2D>("Graphics/goodUVhalf").GetData(BlockCoordinatesHalf, 0, arraySize);
        BlockCoordinatesQuarter = new Color[arraySize];
        Game1.Instance.Content.Load<Texture2D>("Graphics/goodUVquarter").GetData(BlockCoordinatesQuarter, 0, arraySize);

        BlockShadow = Atlas.Load("blocks/block shadow smaller", SliceBlockDepthMap, NormalMap);
        BlockBlueprint = Atlas.Load("blocks/blockblueprint");
        BlockBlueprintGrayscale = Atlas.Load(Game1.Instance.Content.Load<Texture2D>("Graphics/items/blocks/blockblueprint").ToGrayscale(), "blocks/blockblueprint-grayscale");

        BlockHighlight = Atlas.Load("blocks/highlightfull");
        BlockHighlightBack = Atlas.Load("blocks/highlightfullback", BlockDepthMapFar);// BlockDepthMapBack);

        Atlas.Initialize();
    }

    protected static AtlasDepthNormals.Node.Token LoadTexture(string name, string localfilepath)
    {
        return Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/" + localfilepath).ToGrayscale(), name);
    }
   
    public static readonly Texture2D MouseMapSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube");
    public static readonly Texture2D HalfBlockMouseMapTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube half");
    public static readonly Texture2D QuarterBlockMouseMapTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube quarter");

    public static readonly Texture2D NormalMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockNormalsFilled19");
    public static readonly Texture2D BlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepth09height19");
    //public static readonly Texture2D BlockDepthMapBack = Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepth09back");

    public static readonly Texture2D NormalMapFar = Game1.Instance.Content.Load<Texture2D>("Graphics/blockNormalsInner");
    public static readonly Texture2D BlockDepthMapFar = Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepthFar09");

    public static readonly Texture2D ShaderMouseMap = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap - Cube");

    public static readonly Texture2D HalfBlockNormalMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockHalfNormalsFilled");
    public static readonly Texture2D HalfBlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockHalfDepth09");

    public static readonly Texture2D QuarterBlockMapNormal = Game1.Instance.Content.Load<Texture2D>("Graphics/blockQuarterNormalsFilled");
    public static readonly Texture2D QuarterBlockMapDepth = Game1.Instance.Content.Load<Texture2D>("Graphics/blockQuarterDepth09");

    public static readonly Texture2D SliceBlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockOneDepth09");

    public static readonly Texture2D MouseMapSpriteOpposite = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube - back");

    public static readonly MouseMap BlockMouseMap = new(MouseMapSprite, MouseMapSpriteOpposite, MouseMapSprite.Bounds, true);
    public static readonly MouseMap BlockHalfMouseMap = new(HalfBlockMouseMapTexture, MouseMapSpriteOpposite, HalfBlockMouseMapTexture.Bounds, true);
    public static readonly MouseMap BlockQuarterMouseMap = new(QuarterBlockMouseMapTexture, MouseMapSpriteOpposite, QuarterBlockMouseMapTexture.Bounds, true);

    public static AtlasDepthNormals Atlas = new("Blocks") { DefaultDepthMask = BlockDepthMap, DefaultNormalMask = NormalMap };

    public static readonly AtlasDepthNormals.Node.Token[] Cracks =
    {
        Atlas.Load("blocks/cracks1", BlockDepthMap, NormalMap),
        Atlas.Load("blocks/cracks2", BlockDepthMap, NormalMap),
        Atlas.Load("blocks/cracks3", BlockDepthMap, NormalMap)
    };

    public static readonly Dictionary<IntVec3, AtlasDepthNormals.Node.Token> FaceHighlights = new()
    {
        { IntVec3.UnitX, Atlas.Load("blocks/highlighteast", BlockDepthMap, NormalMap) },
        { IntVec3.UnitY, Atlas.Load("blocks/highlightsouth", BlockDepthMap, NormalMap) },
        { IntVec3.UnitZ, Atlas.Load("blocks/highlighttop", BlockDepthMap, NormalMap) },
        { -IntVec3.UnitX, Atlas.Load("blocks/highlightwest", BlockDepthMapFar, NormalMapFar) },
        { -IntVec3.UnitY, Atlas.Load("blocks/highlightnorth", BlockDepthMapFar, NormalMapFar) },
        { -IntVec3.UnitZ, Atlas.Load("blocks/highlightdown", BlockDepthMapFar, NormalMapFar) },
    };

    public static readonly AtlasDepthNormals.Node.Token[]
        TexturesCounter = new AtlasDepthNormals.Node.Token[4] {
            LoadTexture("counter1grayscale", "/counters/counter1"),
            LoadTexture("counter4grayscale", "/counters/counter4"),
            LoadTexture("counter3grayscale", "/counters/counter3"),
            LoadTexture("counter2grayscale", "/counters/counter2") };

    #region Interfaces
    public string GetName()
    {
        return this.LabelReadable;
    }
    public virtual Icon GetIcon()
    {
        return new Icon(this.GetDefault());
    }
    public string GetCornerText()
    {
        return "";
    }
    public Color GetSlotColor()
    {
        return Color.White;
    }
    public void GetTooltipInfo(Control tooltip)
    {
        tooltip.AddControls(this.Name.ToLabel());
    }

    public virtual void GetTooltip(Control tooltip, MapView viewport, IntVec3 global, IntVec3 face)
    {
        var map = viewport.Map;
        var cell = map.GetCell(global);
        var chunk = map.GetChunk(global);
        tooltip.Controls.Add(new Label($"Global: {global}") { Location = tooltip.Controls.BottomLeft });
        tooltip.Controls.Add(new Label($"Local: {global.ToLocal()}") { Location = tooltip.Controls.BottomLeft });
        tooltip.Controls.Add(new Label("Chunk: " + map.GetChunk(global).MapCoords.ToString()) { Location = tooltip.Controls.BottomLeft });
        if (viewport.Settings.MysteriousBlocks && map.IsUndiscovered(global))
        {
            tooltip.AddControlsBottomLeft(new Label("Undiscovered area") { Font = UIManager.FontBold, TextColor = Color.Goldenrod });
            return;
        }
        tooltip.Controls.Add(new Label(this.Name) { Location = tooltip.Controls.BottomLeft, Font = UIManager.FontBold, TextColor = Color.Goldenrod });
        if(cell.Material is MaterialDef mat)
            tooltip.Controls.Add(new Label(mat/*.ToString()*/) { TextColorFunc = () => mat.Color, Location = tooltip.Controls.BottomLeft });

        var data = cell.BlockData;
        var binary = Convert.ToString(data, 2);
        var datastring = int.Parse(binary).ToString("00000000");
        tooltip.Controls.Add(new Label("BlockData: " + datastring + " (" + data.ToString() + ")") { Location = tooltip.Controls.BottomLeft });
        tooltip.Controls.Add(new Label("Variation: " + cell.Variation.ToString()) { Location = tooltip.Controls.BottomLeft });
        tooltip.Controls.Add(new Label("Orientation: " + cell.Orientation.ToString()) { Location = tooltip.Controls.BottomLeft });
        //tooltip.Controls.Add(new Label("Face light: " + map.GetSunLight(global + face)) { Location = tooltip.Controls.BottomLeft });
        var light = map.GetLight(global + face, out var skylight, out var blocklight);
        tooltip.Controls.Add(new Label($"Face sunlight: {skylight}") { Location = tooltip.Controls.BottomLeft });
        tooltip.Controls.Add(new Label($"Face blocklight: {blocklight}") { Location = tooltip.Controls.BottomLeft });
        tooltip.Controls.Add(new Label($"Heightmap: {chunk.GetHeightMapValue(global)}") { Location = tooltip.Controls.BottomLeft });

        map.GetBlockEntity(global)?.GetTooltip(tooltip);
    }

    public virtual void DrawUI(SpriteBatch sb, Vector2 pos)
    {
        var token = this.GetDefault();
        sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, this.BlockState.GetTint(0));
    }

    public virtual AtlasDepthNormals.Node.Token GetDefault()
    {
        var token = this.GetToken(0, 0, 0, 0);
        return token;
    }

    public virtual void DrawUI(SpriteBatch sb, Vector2 pos, byte state)
    {
        var token = this.GetToken(0, 0, 0, 0);
        var c = this.BlockState.GetTint(state);
        c.A = 255;
        sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, c);
    }
    #endregion

    public override string ToString()
    {
        return "Block:" + this.LabelReadable;
    }

    public virtual string Name => this.LabelReadable;
    public override string LabelReadable { get; }
    //static int BaseIDSequence = 1;
    //public readonly int BaseID;
    public static readonly int Width = 32, Depth = 16, Height = 40, BlockHeight = 20;
    public static readonly Vector2 OriginCenter = new(Width / 2f, Height - Depth / 2f);

    internal virtual void PreRemove(MapBase mapBase, IntVec3 p)
    {
    }

    public static Rectangle Bounds = new(-(int)OriginCenter.X, -(int)OriginCenter.Y, Width, Height);
    public virtual BoundingBox GetBoundingBox(MapBase map, IntVec3 global) => new(new(global.X - .5f, global.Y - .5f, global.Z), new(global.X + .5f, global.Y + .5f, global.Z + 1));

    public virtual Color[] UV => BlockCoordinatesFull;
    public virtual MouseMap MouseMap => BlockMouseMap;
    // TODO find a way to make this method required for blocks tha have entity
    
    public virtual BlockEntity GetBlockEntityOrNew(MapBase map, IntVec3 originGlobal, BlockComp.Spec args)
    { return null; }
    internal virtual BlockEntity TryLinkToAdjacentBlockEntity(MapBase map, IntVec3 global) => null;
    internal virtual BlockEntity TryCreateNewBlockEntity(MapBase map, IntVec3 global, int orientation)
    {
        var entity = this.BlockDef.CreateEntity();
        if (entity is null)
            return null;
        return entity.SetFootprint(this.GetFootprint(map, global, orientation).Select(c=>c.global));
    }
    public IEnumerable<IntVec3> GetChildren(MapBase map, IntVec3 originGlobal)
    {
        return this.GetChildren(map.GetCell(originGlobal).Orientation).Select(l => originGlobal + l);
    }
    public IEnumerable<IntVec3> GetChildren(int ori)
    {
        return this.GetChildrenWithSource(ori).Select(i => i.local);
    }
    public IEnumerable<(IntVec3 global, IntVec3 source)> GetChildrenWithSource(IntVec3 originGlobal, int ori)
    {
        return this.GetChildrenWithSource(ori).Select(l => (originGlobal + l.local, l.source));
    }
    public IEnumerable<(IntVec3 local, IntVec3 source)> GetChildrenWithSource(int ori)
    {
        for (int i = 0; i < this.Size.X; i++)
        {
            for (int j = 0; j < this.Size.Y; j++)
            {
                for (int k = 0; k < this.Size.Z; k++)
                {
                    var local = new IntVec3(i, j, k);
                    var parent = new IntVec3(i == 0 ? 0 : -1, j == 0 ? 0 : -1, k == 0 ? 0 : -1); // slaves can only extend up to (1,1,1), that's why they have to point at most back to (-1,-1,-1)
                    yield return (Coords.Rotate(local, ori), Coords.Rotate(parent, ori));
                }
            }
        }
    }
    public Dictionary<IntVec3, IntVec3> GetChildrensWithSourceDic(IntVec3 originGlobal, int ori)
    {
        return this.GetChildrenWithSource(originGlobal, ori).ToDictionary(i => i.global, i => i.source);
    }

    public IntVec3 Size = IntVec3.One;
    //public ConstructionProfile ConstructionProfile;
    public BuildProperties BuildProperties = new();
    public FurnitureDef Furniture;
    /// <summary>
    /// The index to the block's variation is stored within a cell. Add variations to this list so they can be picked at random whenever a block is placed.
    /// </summary>
    public List<AtlasDepthNormals.Node.Token> Variations = new();
    public List<Sprite> Sprites = new();

    /// <summary>
    /// Defines wether the block fully obscures adjacent block faces.
    /// </summary>
    public bool HidingAdjacent = true;

    /// <summary>
    /// Defines the alpha channel of the block's sprite during drawing.
    /// </summary>
    public readonly float Transparency;

    /// <summary>
    /// Defines how fast the block can be dug?
    /// </summary>
    public readonly float Density;

    /// <summary>
    /// Defines how much light the block lets pass through it.
    /// </summary>
    public readonly bool Opaque;

    /// <summary>
    /// Defines whether the block can be walked upon (maybe combine this with density to make things like moving sand?)
    /// </summary>
    public readonly bool Solid;

    protected Block(string name, float transparency = 0f, float density = 1f, bool opaque = true, bool solid = true)
    {
        this.LabelReadable = name;
        this.Transparency = transparency;
        this.Density = density;
        this.Opaque = opaque;
        this.Solid = solid;
        this.LootTable = new LootTable();
    }

    /// <summary>
    /// Determines wether the block will be considered as a pathfinding finish node for the AI to stand IN
    /// </summary>
    public virtual bool IsStandableIn => !this.Solid;
    /// <summary>
    /// Determines wether the block will be considered as a pathfinding finish node for the AI to stand ON
    /// </summary>
    public virtual bool IsStandableOn => this.Solid;
    public virtual byte Luminance { get; }
    public virtual bool IsMinable => false;
    public bool IsDeconstructible => this.BlockDef.ConstructionProfile?.IsDeconstructible ?? false;
    public virtual bool IsRoomBorder => this.Opaque;
    public virtual bool Multi => false;
    public virtual Color DirtColor => Color.White;
    public LootTable LootTable { get; set; }

    public virtual FurnitureDef GetFurnitureRole(MapBase map, IntVec3 global) => null;

    public virtual bool IsTargetable(Vector3 global)
        =>  true;
    
    protected virtual ParticleEmitterSphere GetDustEmitter()
    {
        var emitter = new ParticleEmitterSphere()
        {
            Lifetime = Ticks.PerSecond / 2f,
            Offset = Vector3.Zero,
            Rate = 0,
            ParticleWeight = 0f,//1f,
            ColorEnd = Color.White * .5f,
            ColorBegin = Color.White,
            SizeEnd = 1,
            SizeBegin = 3,
            Force = .01f,
            Friction = 0f
        };
        return emitter;
    }

    
    protected virtual ParticleEmitterSphere GetDirtEmitter()
    {
        var dustcolor = this.DirtColor;
        var emitter = new ParticleEmitterSphere()
        {
            Lifetime = Ticks.PerSecond / 2f,
            Offset = Vector3.Zero,
            Rate = 0,
            ParticleWeight = 1f,//1f,
            ColorEnd = dustcolor,// * .5f,//Color.SaddleBrown * .5f,
            ColorBegin = dustcolor,// Color.SaddleBrown,
            SizeEnd = 2,
            SizeBegin = 2,
            Force = .05f
        };
        return emitter;
    }
    public virtual ParticleEmitterSphere GetEmitter() => this.GetDirtEmitter();// this.GetDustEmitter();
    protected virtual Rectangle ParticleTextureRect => this.Variations[0].Rectangle;
    public List<Rectangle> GetParticleRects(int count)
    {
        var list = new List<Rectangle>(count);
        var sqrt = (int)Math.Sqrt(count);
        var rect = this.ParticleTextureRect;// this.Variations[0].Rectangle;
        var w = rect.Width / sqrt;
        var h = rect.Height / sqrt;
        for (int i = 0; i < sqrt; i++)
            for (int j = 0; j < sqrt; j++)
                list.Add(new Rectangle(rect.X + i * w, rect.Y + j * h, w, h));

        return list;
    }


    public virtual byte ParseData(string data)
    {
        return 0;
    }
  
    public Ingredient Ingredient
    {
        get => this.BuildProperties.Ingredient;
        set => this.BuildProperties.Ingredient = value;
    }
    public int BuildComplexity => this.BuildProperties.Complexity;
    public ConstructionCategoryDef ConstructionCategory => this.BuildProperties.Category;
    public readonly bool HasData;
 
    /// <summary>
    /// TODO: maybe pass position of neighbor that changed?
    /// </summary>
    /// <param name="net"></param>
    /// <param name="source"></param>
    public virtual void OnNeighborChanged(MapBase map, IntVec3 current, IntVec3 source) 
    {
        map.GetBlockEntity(current)?.OnNeighborChanged(map, source);
    }

    public void Deconstruct(GameObject actor, Vector3 global)
    {
        this.OnDeconstruct(actor, global);
        actor.Map.GetBlockEntity(global)?.Deconstruct(actor, global);
        actor.Map.RemoveBlock(global);
    }
    protected virtual void OnDeconstruct(GameObject actor, Vector3 global)
    {
        var map = actor.Map;
        var cell = map.GetCell(global);
        var material = cell.Material;
        var scraps = RawMaterialDefOf.Scraps;
        var materialQuantity = this.Ingredient.Amount;
        actor.Net.PopLoot(new LootTable(new LootWrapper(a => scraps.CreateFrom(material), 1, materialQuantity, scraps.StackCapacity / 2, scraps.StackCapacity)), global, Vector3.Zero);
    }

    public virtual bool IsValidPosition(MapBase map, IntVec3 global, int orientation) { return true; }

    [Obsolete($"use {nameof(MapBase.SetBlock)}")]
    public static void Place(Block block, MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
    {
        block.OnPlaced(map, global, material, data, variation, orientation, false);// notify);
        var children = block.GetChildrenWithSource(global, orientation);
        foreach (var (child, source) in children)
            map.GetCell(child).Origin = source;

        if (notify)
            map.NotifyBlocksChanged(children.Select(c => c.global));
    }
    
    internal virtual void GetPlacementPlan(MapBase map, IntVec3 global, int orientation, List<(IntVec3 global, byte data)> cellMutations, out BlockEntity entity)
    {
        entity = null;
        cellMutations.Add((global, 0));
    }
    internal virtual IEnumerable<(IntVec3 global, byte data)> GetFootprint(MapBase map, IntVec3 global, int orientation)
    {
        yield return (global, 0);
    }
    internal virtual void OnPlaced(MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
    {
        //map.SetBlock(global, this, material, data, variation, orientation, notify);
        //var entity = this.GetBlockEntityOrNew(map, global, null);
        //if (entity != null)
        //{
        //    map.AddBlockEntity(global, entity);
        //    map.EventOccured(Message.Types.BlockEntityAdded, entity, global);
        //}
        //map.SetBlockLuminance(global, this.Luminance);

        //var children = this.GetChildrenWithSource(global, orientation);
        //foreach (var (child, source) in children)
        //    map.GetCell(child).Origin = source;

        //if (notify)
        //    map.NotifyBlocksChanged(children.Select(c => c.global));
    }

    public void BlockBelowChanged(MapBase map, IntVec3 global)
    {
        map.GetBlockEntity(global)?.OnBlockBelowChanged(map, global);
        this.OnBlockBelowChanged(map, global);
    }
    protected virtual void OnBlockBelowChanged(MapBase map, IntVec3 global) { }
    public virtual LootTable GetLootTable(byte data)
    {
        return this.LootTable;
    }
    public ItemDef BreakProduct;
    void BreakNew(MapBase map, IntVec3 global)
    {
        var net = map.Net;
        var item = this.BreakProduct;
        if(item is not null)
            net.PopLoot(ItemFactory.CreateFrom(item, map.GetCell(global).Material).SetStackSize(item.StackCapacity), global, Vector3.Zero);
        map.RemoveBlock(global);
    }
    public virtual void Break(MapBase map, IntVec3 global)
    {
        //var net = map.Net;
        //net.PopLoot(this.GetLootTable(net.Map.GetBlockData(global)), global, Vector3.Zero);
        this.BreakNew(map, global);
        map.RemoveBlock(global);
    }
    public virtual bool IsSolid(MapBase map, Vector3 global, byte data)
    {
        var offset = global + 0.5f * new Vector3(1, 1, 0);
        offset -= offset.RoundXY();
        var h = this.GetHeight(data, offset.X, offset.Y);
        return this.Solid && offset.Z < h;
    }
    public virtual float GetDensity(byte data, Vector3 global)
    {
        var offset = global.ToBlock();
        var h = this.GetHeight(data, offset.X, offset.Y);
        return offset.Z < h ? this.Density : 0;
    }

    public virtual bool IsSolid(Cell cell)
    {
        return this.Solid;
    }
    public virtual bool IsSolid(Cell cell, Vector3 withinBlock)
    {
        var h = this.GetHeight(cell.BlockData, withinBlock.X, withinBlock.Y);
        return this.Solid && withinBlock.Z < h;
    }
    public virtual bool IsOpaque(Cell cell) => this.Opaque;
    public virtual void RandomBlockUpdate(INetEndpoint net, IntVec3 global, Cell cell) { }
    protected void LoadVariations(params string[] assetNames)
    {
        foreach (string name in assetNames)
        {
            var token = Atlas.Load("blocks/" + name.Trim(), BlockDepthMap, NormalMap);
            this.Variations.Add(token);
        }
    }
    public virtual void Draw(MySpriteBatch sb, Rectangle screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
    {
        if (this == BlockDefOf.Air.Block)
            return;

        sb.DrawBlock(Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], zoom, fog, tint, sunlight, blocklight, depth);
    }
  
    public virtual void Draw(MySpriteBatch sb, Vector4 screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
    {
        if (this == BlockDefOf.Air.Block)
            return;

        sb.DrawBlock(Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], zoom, fog, tint, sunlight, blocklight, depth, this);
    }
    public virtual MyVertex[] Draw(MySpriteBatch sb, Vector3 blockCoordinates, float zoom, int rotation, Vector4 screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
    {
        if (this == BlockDefOf.Air.Block)
            return null;

        var material = this.DrawMaterialColor ? mat.ColorVector : DefaultColorVector;// this.GetColorVector(data);

        var token = this.GetToken(variation, orientation, rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
        return sb.DrawBlock(Atlas.Texture, screenBounds,
            token,
            zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, this, blockCoordinates);

    }
    public virtual MyVertex[] Draw(MySpriteBatch sb, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
    {
        if (this == BlockDefOf.Air.Block)
            return null;

        var material = this.DrawMaterialColor ? mat.ColorVector : DefaultColorVector;// this.GetColorVector(data);

        var token = this.GetToken(variation, orientation, (int)camera.Rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
        return sb.DrawBlock(Atlas.Texture, screenBounds,
            token,
            camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, this, blockCoordinates);

    }
    public virtual MyVertex[] Draw(Chunk chunk, IntVec3 blockCoordinates, Camera camera, Vector4 screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
    {
        if (this == BlockDefOf.Air.Block)
            return null;

        //var material = this.GetColorVector(data);
        var material = this.DrawMaterialColor ? mat.ColorVector : DefaultColorVector;// this.GetColorVector(data);

        var token = this.GetToken(variation, orientation, (int)camera.Rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
        return chunk.Canvas.Opaque.DrawBlock(Atlas.Texture, screenBounds,
            token,
            camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, this, blockCoordinates);
    }
    internal void Draw(Canvas canvas, Chunk chunk, IntVec3 vector3, MapView view, Vector4 screenBoundsVector4, Vector4 sun, Vector4 block, Color finalFogColor, Color tint, int depth, Cell cell)
    {
        this.Draw(canvas, chunk, vector3, view, screenBoundsVector4, sun, block, finalFogColor, tint, depth, cell.Variation, cell.Orientation, cell.BlockData, cell.Material);
        if (cell.Damage > 0)
            DrawCracks(canvas.Opaque, vector3, view, screenBoundsVector4, sun, block, finalFogColor, tint, depth, cell.Damage);
    }
    internal void Draw(Canvas canvas, Chunk chunk, IntVec3 global, MapView view, Vector4 screenBoundsVector4, Vector4 sun, Vector4 block, Color finalFogColor, Color tint, int depth)
    {
        var cell = chunk.GetLocalCell(global);
        this.Draw(canvas, chunk, global, view, screenBoundsVector4, sun, block, finalFogColor, tint, depth, cell.Variation, cell.Orientation, cell.BlockData, cell.Material);
        if (cell.Damage > 0)
            DrawCracks(canvas.Opaque, global, view, screenBoundsVector4, sun, block, finalFogColor, tint, depth, cell.Damage);
    }
    public virtual MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, MapView view, Vector4 screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
    {
        if (this == BlockDefOf.Air.Block)
            return null;

        //var material = this.GetColorVector(data);
        var material = this.DrawMaterialColor ? mat.ColorVector : DefaultColorVector;// this.GetColorVector(data);

        var mesh = this.Opaque ? canvas.Opaque : canvas.NonOpaque;

        if (this.IsRoomBorder && 
            chunk.Map.Town.RoomManager.GetRoomBorderAt(global) is Room room && 
            room.IsWallHidable(global, view.Camera.RotationReverse))
            mesh = canvas.WallHidable;

        var token = this.GetToken(variation, orientation, (int)view.Rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
        return mesh.DrawBlock(Atlas.Texture, screenBounds,
            token,
            view.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, this, global);
    }
    public virtual void Draw(MySpriteBatch sb, Vector2 screenPos, Vector4 sunlight, Vector4 blocklight, Color tint, float zoom, float depth, Cell cell)
    {
        if (this == BlockDefOf.Air.Block)
            return;

        sb.DrawBlock(Atlas.Texture, screenPos, this.Variations[cell.Variation], zoom, tint, sunlight, blocklight, depth);
    }
    public virtual void DrawPreview(MySpriteBatch sb, IntVec3 global, MapView view, byte data, MaterialDef mat, int variation = 0, int orientation = 0)
    {
        this.DrawPreview(sb, global, view, Color.White * .5f, data, mat, variation, orientation);
    }
    public virtual void DrawPreview(MySpriteBatch sb, IntVec3 global, MapView view, Color tint, byte data, MaterialDef mat, int variation = 0, int orientation = 0)
    {
        //var depth = global.GetDrawDepth(view);
        var depth = view.GetDrawDepth(global);
        var materialcolor = this.DrawMaterialColor ? mat.Color : Color.White;// this.GetColor(data);
        var token = this.GetPreviewToken(variation, orientation, (int)view.Rotation, data); // change the method to accept double so i don't have to cast the camera rotation to int?
        var bounds = view.GetScreenBoundsVector4(global.X, global.Y, global.Z, Bounds, Vector2.Zero);
        sb.DrawBlock(Atlas.Texture, bounds, token, view.Zoom, Color.Transparent, tint, materialcolor, Vector4.One, Vector4.One, Vector4.Zero, depth, this);
    }
    protected static void DrawShadow(MySpriteBatch nonopaquemesh, IntVec3 blockCoordinates, MapView view, Vector4 screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float depth)
    {
        nonopaquemesh.DrawBlock(Atlas.Texture, screenBounds, BlockShadow, view.Zoom, fog, tint, Color.White, sunlight, blocklight, Vector4.Zero, depth, null, blockCoordinates);
    }
    protected static void DrawCracks(MySpriteBatch opaquemesh, IntVec3 blockCoordinates, MapView view, Vector4 screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int damage)
    {
        var cracksTexture = Cracks[damage - 1];
        opaquemesh.DrawBlock(Atlas.Texture, screenBounds, cracksTexture, view.Zoom, fog, Color.White, Color.White, sunlight, blocklight, Vector4.Zero, depth, null, blockCoordinates);
    }
    public virtual AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte blockdata)
    {
        return this.Variations[Math.Min(variation, this.Variations.Count - 1)];
    }
    public virtual AtlasDepthNormals.Node.Token GetToken(int cameraRotation, Cell cell)
    {
        return this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)];
    }
    public virtual AtlasDepthNormals.Node.Token GetPreviewToken(int variation, int orientation, int cameraRotation, byte data)
    {
        return this.GetToken(variation, orientation, cameraRotation, data);
    }

    public virtual IBlockState BlockState => new DefaultState();

    static Vector4 DefaultColorVector = Vector4.One;

    public string GetName(byte p)
    {
        var statename = this.BlockState.GetName(p);
        return this.GetName() + (string.IsNullOrWhiteSpace(statename) ? "" : ":" + statename);
    }
    internal virtual ContextAction GetContextRB(GameObject player, IntVec3 global)
    {
        throw new NotImplementedException();
    }
    internal virtual ContextAction GetContextActivate(GameObject player, IntVec3 global)
    {
        return null;
    }

    public virtual List<Interaction> GetAvailableTasks(MapBase map, IntVec3 global)
    {
        return new List<Interaction>();
    }

    public virtual void GetContextActions(GameObject player, IntVec3 global, ContextArgs a)
    {
        throw new NotImplementedException();
    }

    public static float GetPathingCost(MapBase map, IntVec3 global)
    {
        var cell = map.GetCell(global);
        return cell.Block.GetPathingCost(cell.BlockData);
    }
    public virtual float GetPathingCost(byte data)
    {
        return 1;
    }

    public bool RequiresConstruction = true;
    internal MaterialDef DefaultMaterial;
    protected bool DrawMaterialColor = true;

    public virtual IEnumerable<MaterialDef> GetEditorVariations()
    {
        if(this.Ingredient is not null)
            foreach (var i in this.Ingredient.GetAllValidMaterials())
                yield return i;
        if(this.BlockDef.ConstructionProfile is not null)
            foreach (var i in this.BlockDef.ConstructionProfile.Refinements)
                foreach (var m in MaterialSystem.MaterialsByType[i.MaterialType])
                    yield return m;
        //return this.Ingredient?.GetAllValidMaterials() ?? Enumerable.Empty<MaterialDef>();//.Select(m => (byte)m.ID);
    }
    internal IEnumerable<ItemMaterialAmount> GetAllValidConstructionMaterialsNew()
    {
        return this.Ingredient?.GetItemMaterialAmounts(this) ?? Enumerable.Empty<ItemMaterialAmount>();
    }
   
    //public virtual void OnSteppedOn(GameObject actor, IntVec3 global) { }

    public virtual bool TryConsume(GameObject actor, GameObject dropped, IntVec3 global, int amount = -1)
    {
        return false;
    }

    public static MaterialDef GetBlockMaterial(MapBase map, IntVec3 global)
    {
        var cell = map.GetCell(global);
        return cell.Material;
    }
    public static float GetBlockHeight(MapBase map, Vector3 global)
    {
        /// check if cell exists?
        var offset = global.ToBlock();
        var cell = map.GetCell(global);
        var h = cell.Block.GetHeight(cell.BlockData, offset.X, offset.Y);
        return h;
    }
    public virtual Vector3 GetVelocityTransform(byte data, Vector3 blockcoords) { return Vector3.Zero; }

    public virtual float GetHeight(byte data, float x, float y)
    {
        return this.GetHeight(x, y);
    }
    public virtual float GetHeight(float x, float y) { return this.Solid ? 1f : 0f; }
    public float GetHeight(byte data, Vector3 blockcoords) { return this.GetHeight(data, blockcoords.X, blockcoords.Y); }

    public static readonly AtlasDepthNormals.Node.Token ParticlePixel = Atlas.Load(UIManager.Highlight, "particle");

    static readonly AtlasDepthNormals.Node.Token Token = Atlas.Load("blocks/blockunknown", BlockDepthMap, NormalMap);

    public static MyVertex[] DrawUnknown(MySpriteBatch opaquemesh, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Vector4 sunlight, Vector4 blocklight, Color fog, Color tint, float depth)
    {
        return opaquemesh.DrawBlock(Atlas.Texture, screenBounds,
            Token,
            camera.Zoom, fog, tint, Vector4.One, sunlight, blocklight, Vector4.Zero, depth, null, blockCoordinates);
    }

    internal static bool IsBlockSolid(MapBase map, IntVec3 global)
    {
        var cell = map.GetCell(global);
        return cell == null || cell.Block.IsSolid(map, global, cell.BlockData);
    }

    internal virtual bool IsPathable(Cell cell, IntVec3 blockCoords)
    {
        return !this.IsSolid(cell, blockCoords);
    }

    public virtual void ShowUI(IntVec3 global)
    {

    }

    public void PaintIcon(int width, int height, byte data, MaterialDef mat)
    {
        var gd = Game1.Instance.GraphicsDevice;
        var token = this.GetDefault();
        var fx = Renderer.Effect;// Game1.Instance.Content.Load<Effect>("blur");
        var mysb = new MySpriteBatch(gd);
        fx.CurrentTechnique = fx.Techniques["Combined"];
        fx.Parameters["Viewport"].SetValue(new Vector2(width, height));
        fx.Parameters["s"].SetValue(Atlas.Texture);
        fx.Parameters["s1"].SetValue(Atlas.DepthTexture);
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        var bounds = new Vector4((width - Width) / 2, (height - Height) / 2, token.Texture.Bounds.Width, token.Texture.Bounds.Height);
        //var cam = new Camera
        //{
        //    SpriteBatch = mysb
        //};
        this.Draw(mysb, Vector3.Zero, 1, 0, bounds, Vector4.One, Vector4.One, Color.Transparent, Color.White, 0.5f, 0, 0, data, mat ?? MaterialDefOf.Air);
        mysb.Flush();
    }
    public RenderTarget2D PaintIcon(byte data, MaterialDef mat)
    {
        var gd = Game1.Instance.GraphicsDevice;
        var token = this.GetDefault();
        var w = token.Texture.Bounds.Width;
        var h = token.Texture.Bounds.Height;
        var renderTarget = new RenderTarget2D(gd, w, h);
        gd.SetRenderTarget(renderTarget);
        gd.Clear(Color.Transparent);
        var fx = Renderer.Effect;// Game1.Instance.Content.Load<Effect>("blur");
        var mysb = new MySpriteBatch(gd);
        fx.CurrentTechnique = fx.Techniques["Combined"];
        fx.Parameters["Viewport"].SetValue(new Vector2(w, h));
        fx.Parameters["s"].SetValue(Atlas.Texture);
        fx.Parameters["s1"].SetValue(Atlas.DepthTexture);
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        var bounds = new Vector4(0, 0, w, h);
        //var cam = new Camera
        //{
        //    SpriteBatch = mysb
        //};
        this.Draw(mysb, Vector3.Zero, 1, 0, bounds, Vector4.One, Vector4.One, Color.Transparent, Color.White, 0.5f, 0, 0, data, mat);
        mysb.Flush();
        gd.SetRenderTarget(null);
        return renderTarget;
    }
    protected virtual IEnumerable<IntVec3> GetInteractionSpotsLocal() { yield break; }
    protected virtual IEnumerable<IntVec3> GetInteractionSpotsLocal(MapBase map, IntVec3 global) { yield break; }
    protected virtual bool HasInteractionSpots => false;

    public Color? TooltipColor => null;

    internal IEnumerable<IntVec3> GetInteractionSpotsLocal(MapBase map, IntVec3 global, int orientation)
    {
        // main entry
        return this.GetInteractionSpotsLocal(map, global).Select(s => Coords.Rotate(s, orientation));   
    }
    internal IEnumerable<IntVec3> GetInteractionSpots(IntVec3 global, int orientation)
    {
        return this.GetInteractionSpotsLocal().Select(s => global + Coords.Rotate(s, orientation));
    }
    internal IEnumerable<IntVec3> GetInteractionSpots(Cell cell, MapBase map, IntVec3 global)
    {
        foreach (var p in this.GetInteractionSpotsLocal(map, global, cell.Orientation))
            yield return p + global;
    }
    internal IEnumerable<IntVec3> GetInteractionSpots(MapBase map, IntVec3 global)
    {
        //var cell = map.GetCell(global);
        if (!map.Contains(global))
            yield break;
        global = Cell.GetOrigin(map, global);
        var cell = map.GetCell(global);
        foreach (var p in this.GetInteractionSpotsLocal(map, global, cell.Orientation))
            yield return p + global;
    }
    internal IEnumerable<IntVec3> GetReservedInteractionCells(IntVec3 global, int orientation) => this.GetInteractionSpots(global, orientation).SelectMany(ActorDefOf.Npc.OccupyingCellsStanding);
    internal IEnumerable<IntVec3> GetReservedInteractionCells(MapBase map, IntVec3 global) => this.GetInteractionSpots(map, global).SelectMany(ActorDefOf.Npc.OccupyingCellsStanding);

    public virtual IEnumerable<(string label, Type type)> GetSelectionTabs() { yield break; }
    internal virtual void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
    {
        map.GetBlockEntity(vector3)?.GetSelectionInfo(info, map, vector3);
    }

   
    internal void GetQuickButtons(SelectionManager uISelectedInfo, MapBase map, IntVec3 vector3)
    {
        //var e = map.GetBlockEntity(vector3);
        //e?.GetQuickButtons(uISelectedInfo, map, vector3);
        if (this.Furniture is not null)
            uISelectedInfo.AddTabAction("Room", () => { });
        this.GetQuickButtonsEx(uISelectedInfo, map, vector3);
    }

    protected virtual void GetQuickButtonsEx(SelectionManager info, MapBase map, IntVec3 vector3) { }
    internal virtual bool IsValidHaulDestination(MapBase map, IntVec3 global, GameObject obj)
    {
        return false;
    }
    internal virtual string GetName(MapBase map, IntVec3 global)
    {
        return this.Name;
    }
    internal virtual float GetFertility(Cell cell)
    {
        return cell.Material == MaterialDefOf.Soil ? 1f : 0;
        //return this.GetMaterial(cell.BlockData) == MaterialDefOf.Soil ? 1f : 0;
    }

    internal virtual void OnDrawSelected(MySpriteBatch sb, RenderContext ctx, IntVec3 global)
    {

    }
    internal void DrawSelected(MySpriteBatch sb, RenderContext ctx, IntVec3 global)
    {
        ctx.Map.GetBlockEntity(global)?.DrawSelected(sb, ctx, global);
        this.DrawInteractionSpots(sb, ctx, global);
        this.OnDrawSelected(sb, ctx, global);
    }

    internal void DrawInteractionSpots(MySpriteBatch sb, RenderContext ctx, IntVec3 global)
    {
        var map = ctx.Map;
        var interactionSpots = this.GetInteractionSpots(map, global);
        var col = Color.Lime; // color red if interaction spots are obstructed?
        ctx.Renderer.DrawCellHighlights(sb, FaceHighlights[-IntVec3.UnitZ], interactionSpots, col * .5f);
    }
    internal void DrawInteractionCells(MySpriteBatch sb, RenderContext ctx, IntVec3 global, int orientation)
    {
        var map = ctx.Map;
        var interactionCells = this.GetReservedInteractionCells(global, orientation);
        var col = interactionCells.All(c => map.Contains(c) && !map.IsSolid(c)) ? Color.Lime : Color.Red;
        ctx.Renderer.DrawCellHighlights(sb, BlockHighlight, interactionCells, col * .5f);
    }

    public IEnumerable<ConstructionDesignationArgs> GetConstructionOptions()
    {
        var profile = this.BlockDef.ConstructionProfile;
        foreach (var refinement in profile.Refinements)
        {
            var validMats = profile.Materials ?? Def.Get<MaterialDef>().Where(m => refinement.MaterialType == m.Type);
            foreach (var mat in validMats)
                yield return new ConstructionDesignationArgs(this.BlockDef, refinement, mat, ItemDefOf.Ingredient.StackCapacity); // HACK
        }
    }

    internal virtual (bool result, string error) IsAvailable(MapBase map) => (true, null);

    internal virtual void OnPlaced(CellQuery cellQuery) { }

    public IEnumerable<Control> GetTooltipControls()
    {
        var box = new GroupBox();
        this.GetTooltipInfo(box);
        yield return box;
    }

    public class DefaultState : IBlockState
    {
        public void Apply(MapBase map, Vector3 global)
        { }
        public void Apply(ref byte data)
        {
        }
        public void Apply(Block.Data data)
        {
        }
        public void FromCraftingReagent(GameObject item) { }
        public Color GetTint(byte d)
        { return Color.White; }
        public string GetName(byte d)
        {
            return "";
        }

        
    }
}
