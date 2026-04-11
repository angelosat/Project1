using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Rendering;
using Project1.Core.Simulation.Lighting;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns.Terrain;
using Project1.Core.WorldGen;
using Project1.Framework;
using Project1.Framework.Graphics;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Project1.Core.Simulation;

[Flags]
public enum Edges { None = 0x0, West = 0x1, North = 0x2, East = 0x4, South = 0x8, All = 0xF }
public record struct Light() : ISerializable
{
    public byte Sky = 15;
    public byte Block;

    public ISerializable Read(IDataReader r)
    {
        this.Sky = r.ReadByte();
        this.Block = r.ReadByte();
        return this;
    }

    public readonly void Write(IDataWriter w)
    {
        w.Write(this.Sky);
        w.Write(this.Block);
    }
}

public class Chunk : Inspectable
{
    public const int Size = 16;
    public const int SizeSquared = Size * Size;
    public static readonly int Volume = Size * Size * MapBase.MaxHeight;
    public Light[] Light = new Light[Volume];
    public readonly Dictionary<IntVec3Local, LightToken> LightCache = [];
    [InspectorHidden]
    public Cell[] Cells;
    [InspectorHidden]
    public Slice[] Slices = new Slice[128];
    public int[][] HeightMap;
    HashSet<IntVec2> DirtyHeightmapColumns = [];
    public int[] HeightMapPerCellIndex = new int[Chunk.Size * Chunk.Size];
    readonly BlockDamageSystem BlockDamageSystem;
    readonly List<ChunkController> Controllers = [];
    public readonly List<Entity> Entities = [];
    readonly Dictionary<IntVec3, BlockEntity> BlockEntitiesByPosition = [];
    public IntVec2 Start;
    public Vector2 bottomRight;
    public IEnumerable<BlockEntity> BlockEntities => this.BlockEntitiesByPosition.Values.Distinct();

    public Chunk Clone()
    {
        Chunk chunk;
        var w = new DataWriter();
        this.Write(w);
        w.BaseStream.Position = 0;
        using DataReader r = new(w.BaseStream);
        chunk = Chunk.Create(r);
        chunk.Map = this.Map;
        return chunk;
    }

    #region Initialization
    public double GetGradientAt(int localx, int localy, int localz)
    {
        throw new NotImplementedException();
    }
    public Dictionary<IntVec3, double> InitCells2(List<Terraformer> mutators)
    {
        var gradientCache = new Dictionary<IntVec3, double>();
        int n = 0; ;
        var grad = new GradientLowRes(this.World, this);
        var maxh = MapBase.MaxHeight;
        for (int z = 0; z < maxh; z++)
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                {
                    Cell cell = new();
                    double gradient = grad.GetGradient(i, j, z);
                    gradientCache.Add(new IntVec3(i, j, z), gradient);
                    this.Cells[n++] = cell;
                }
        return gradientCache;
    }
    public void InitCells3(Terraformer m, Dictionary<IntVec3, double> gradient)
    {
        var maxh = MapBase.MaxHeight;
        int n = 0;
        for (int z = 0; z < maxh; z++)
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                {
                    var cell = this.Cells[n++];
                    m.Initialize(this.Map.World, cell, this.Start.X + i, this.Start.Y + j, z, gradient[new IntVec3(i, j, z)]);
                }
        this.UpdateHeightMap();

    }
    public Chunk InitCells()
    {
        int n = 0;
        for (int z = 0; z < MapBase.MaxHeight; z++)
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    this.Cells[n++] = new();
        return this;
    }
    #endregion

    public override string ToString()
    {
        string text =
            "Local: " + this.MapCoords.ToString() +
              "\nGlobal: " + this.Start.ToString() +
               "\nObjects: " + this.Entities.Count +
            "\nCells to validate: " + this.CellsToValidate.Count;

        text += "Objects: " + this.Entities.Count.ToString() + "\n";
        return text.Remove(text.Length - 1);
    }

    IntVec3[] _RandomOrderedCells;
    IntVec3[] RandomOrderedCells
    {
        get
        {
            if (this._RandomOrderedCells is null)
            {
                var allPositions = new BoundingBox(IntVec3.Zero, new IntVec3(Chunk.Size - 1, Chunk.Size - 1, MapBase.MaxHeight - 1)).GetBoxIntVec3Lazy();
                var array = allPositions.ToArray();
                array.Shuffle(this.Map.Random);
                this._RandomOrderedCells = array;
            }
            return this._RandomOrderedCells;
        }
    }
    public IntVec3 GetRandomCellInOrder(int index)
    {
        if (index >= this.Cells.Length)
            throw new Exception();
        return this.RandomOrderedCells[index];
    }


    public void Invalidate()
    {
        foreach (var slice in this.Slices)
            slice?.Valid = false;
        this.Valid = false;
    }
    public void InvalidateMesh()
    {
        this.Valid = false;
    }

    public int X, Y;
    public int RectHeight;
    public MapBase Map;
    public WorldBase World => this.Map.World;
    public bool Valid;
    //readonly Queue<IntVec3Local> CellsToValidate = [];
    readonly HashSet<PositionQuery> CellsToValidate = [];

    public bool ChunkBoundariesUpdated = true;
    public bool LightValid = false;
    public bool EdgesValid = false;
    public void InvalidateEdges()
    {
        this.EdgesValid = false;
    }

    #region Public Properties
    [InspectorHidden]
    public Cell this[int localx, int localy, int localz]
    {
        get
        {
            if (localx < 0 || localx > Chunk.Size - 1 || localy < 0 || localy > Chunk.Size - 1 || localz < 0 || localz > MapBase.MaxHeight - 1)
                return null;

            int ind = GetCellIndex(localx, localy, localz);
            var cell = this.Cells[ind];
            return cell;
        }
    }
    [InspectorHidden]
    public Cell this[float localx, float localy, float localz]
    {
        get
        {
            if (localx < 0 || localx > Chunk.Size - 1 || localy < 0 || localy > Chunk.Size - 1 || localz < 0 || localz > MapBase.MaxHeight - 1)
                return null;

            int ind = GetCellIndex(localx, localy, localz);
            return this.Cells[ind];
        }
    }
    [InspectorHidden]
    public Cell this[IntVec3 localCoords]
    {
        get
        {
            if (!localCoords.IsWithinChunkBounds())
                return null;

            return this.Cells[GetCellIndex(localCoords)];
        }
    }
    [InspectorHidden]
    public Cell this[int cellIndex] => this.Cells[cellIndex];

    public IntVec2 MapCoords
    {
        get => new(this.X, this.Y);
        set
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Start = this.MapCoords * Size;
        }
    }
    internal void ResolveReferences()
    {
        foreach (var c in this.Controllers)
            c.ResolveReferences();
    }

    public static readonly int Width = Block.Width * Size;
    public static readonly int Height = MapBase.MaxHeight * Block.BlockHeight + Size * Block.Depth;
    public static readonly Rectangle Bounds = new(-Width / 2, -Height / 2, Width, Height);

    public Rectangle GetScreenBounds(Camera cam)
    {
        Rectangle chunkBounds = cam.GetScreenBounds(this.Start.X + Chunk.Size / 2, this.Start.Y + Chunk.Size / 2, MapBase.MaxHeight / 2, Bounds);  //chunk.Value.GetBounds(camera);
        return chunkBounds;
    }
    #endregion

    public Chunk(MapBase map, Vector2 pos)
        : this()
    {
        this.Map = map;
        this.MapCoords = pos;
        this.InitCells();
    }
    Chunk(Vector2 pos)
        : this()
    {
        this.MapCoords = pos;
    }
    Chunk()
    {
        this.Cells = new Cell[Chunk.Size * Chunk.Size * MapBase.MaxHeight];
        this.HeightMap = new int[Size][];
        for (int i = 0; i < Size; i++)
            this.HeightMap[i] = new int[Size];
        for (int i = 0; i < MapBase.MaxHeight; i++)
            this.Slices[i] = new Slice();
        this.BlockDamageSystem = new(this);
        this.Controllers.Add(new FloraController(this));
        this.Controllers.Add(new FlowerController(this));
        this.Controllers.Add(new GrassController(this));
    }
    public static Chunk Create(MapBase map, Vector2 pos)
    {
        Chunk chunk = new(pos);
        chunk.Map = map;
        return chunk;
    }
    public static Chunk Create(MapBase map, int x, int y)
    {
        Chunk chunk = new(new Vector2(x, y));
        chunk.Map = map;
        return chunk;
    }
    public static Chunk Load(MapBase map, Vector2 key, SaveTag tag)
    {
        return new Chunk(map, key).LoadFromTag(tag);
    }

    public void Add(Entity obj)
    {
        obj.Map = this.Map;
        if (this.Entities.Contains(obj))
            throw new Exception();
        this.Entities.Add(obj);
    }
    public bool Remove(Entity obj)
    {
        if (!this.Entities.Remove(obj))
            throw new Exception();
        return true;
    }

    public IEnumerable<(Cell cell, CellId index)> GetAllCellsWithIndex()
    {
        for (int i = 0; i < this.Cells.Length; i++)
        {
            yield return (this.Cells[i], i);
        }
    }

    public Cell GetLocalCell(int x, int y, int z)
        => this.Cells[GetCellIndex(x, y, z)];
    public Cell GetLocalCell(CellId cellIndex)
      => this.Cells[cellIndex];

    public Cell GetLocalCell(IntVec3Local local)
        => this.Cells[GetCellIndex(local)];

    public static int GetCellIndex(int x, int y, int z)
        => (z << 8) | (y << 4) | x;// (z * Size + y) * Size + x;
    public static int GetCellIndex(float x, float y, float z)
        => GetCellIndex((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(z));

    public static int GetCellIndex(IntVec3Local local)
        => GetCellIndex(local.X, local.Y, local.Z);

    public static IntVec3Local GetLocalFromIndex(CellId index)
        => new(index % Size, (index / Size) % Size, index / (SizeSquared));

    public CellQuery Query(IntVec3Local local)
        => new(this, local);

    public int GetHeightMapValue(IntVec3Local local)
        => this.GetHeightMapValue(local.X, local.Y);
    
    public int GetHeightMapValue(int localx, int localy)
        => this.HeightMap[localx][localy];

    public bool IsAboveHeightMap(IntVec3Local local)
        => local.Z > this.HeightMap[local.X][local.Y];
    
    public bool IsAboveHeightMap(int localx, int localy, int localz)
        => localz > this.HeightMap[localx][localy];

    public void UpdateHeightMap()
    {
        for (int j = 0; j < Size; j++)
            for (int i = 0; i < Size; i++)
                this.UpdateHeightMapColumn(i, j, false);
    }
    public void InvalidateHeightmap(int localx, int localy)
    {
        // invalidate heightmap immediately?
        // TODO: invalidate coordinates and update heightmap at the next tick, so as to prevent updating the same column multiple times in case of multiple block changes
        //this.UpdateHeightMapColumn(localx, localy);
        this.UpdateHeightMapColumnWithLightSmart(localx, localy);
    }

    /// <summary>
    /// the current ont
    /// </summary>
    /// <param name="localx"></param>
    /// <param name="localy"></param>
    public void UpdateHeightMapColumnWithLightSmart(int localx, int localy)
    {
        //this.UpdateHeightMapColumn(localx, localy);
        //return;
        int z;
        Cell cell;
        z = MapBase.MaxHeight - 1;
        bool found = false;
        bool hit = false;
        var oldValue = this.HeightMap[localx][localy];
        int minVal = 0, maxVal = this.Map.GetMaxHeight();
        while (z >= 0)
        {

            cell = this.GetLocalCell(localx, localy, z);
            if (!hit)
                if (cell.Block != BlockDefOf.Air.Block)
                {
                    hit = true;
                }
            if (cell.Opaque)
            {
                if (!found)
                {
                    found = true;
                    int newValue = z;
                    this.HeightMap[localx][localy] = newValue;

                    if (newValue > oldValue)
                    {
                        minVal = oldValue;
                        maxVal = newValue;
                    }
                    else if (newValue < oldValue)
                    {
                        minVal = newValue;
                        maxVal = oldValue;
                    }
                    else return; // new heightmap value is same as previous one so return
                }
            }

            if (found && (minVal < z && z <= maxVal)) // if a new heightmap value found, invalidate cells inbetween the old and the new one
                //this.InvalidateCell(new IntVec3(localx, localy, z)); // why did i have this commented out? it caused slice meshes not getting updated light
                this.InvalidateCell(new IntVec3Local(localx, localy, z)); // why did i have this commented out? it caused slice meshes not getting updated light

            z--;
        }

        if (!found)
            this.HeightMap[localx][localy] = 0;
    }

    public void UpdateHeightMapColumn(int localx, int localy, bool invalidate = true)
    {
        int z;
        byte light;
        Cell cell;
        light = 15;
        z = MapBase.MaxHeight - 1;
        bool hit = false;
        while (z >= 0)
        {
            cell = this.GetLocalCell(localx, localy, z);
            if (!hit)
                if (cell.Block != BlockDefOf.Air.Block)
                {
                    hit = true;
                }
            if (cell.Opaque)
            {
                if (light > 0)
                {
                    this.HeightMap[localx][localy] = z;
                    light = 0;
                }
            }
            //this.SetSkylight(localx, localy, z, light);
            if (invalidate)
                this.InvalidateCell(new IntVec3(localx, localy, z));
            z--;
        }

        if (light > 0)
            this.HeightMap[localx][localy] = z;
    }

    public void ValidateCells()
    {
        if (this.CellsToValidate.Count > 0)
            //this.Map.LightingEngine.HandleImmediate(this.CellsToValidate.Select(p=>p.Global));
            this.Map.LightingEngine.HandleImmediate(this.CellsToValidate);
        foreach (var pos in this.CellsToValidate)
        {
            //this.Map.LightingEngine.HandleImmediate([pos]);
            //this.GetLocalCell(pos.CellIndex).Valid = true;
            this.InvalidateSlice(pos.Local.Z);
            this.InvalidateMesh();
        }
        this.CellsToValidate.Clear();

        //while (this.CellsToValidate.Count > 0)
        //{
        //    var cell = this.CellsToValidate.Dequeue();
        //    this.Map.LightingEngine.HandleImmediate([cell.ToGlobal(this)]);
        //    this.GetLocalCell(cell).Valid = true;
        //    this.InvalidateSlice(cell.Z);
        //    this.InvalidateMesh();
        //}
    }

    public void InvalidateSlice(byte z)
    {
        this.Slices[z].Valid = false;
        this.InvalidateMesh();
    }

    public void InvalidateSlice(int z)
        => this.InvalidateSlice((byte)z);

    public void InvalidateCell(IntVec3Local cell)
    {
        this.BlockDamageSystem.Delete(cell);

        var pos = this.QueryPosition(cell);

        this.InvalidateLight(pos.Global);

        //if (!this.GetLocalCell(cell).Valid)
        //    return;

        this.CellsToValidate.Add(pos);
        //this.CellsToValidate.Enqueue(cell);
        //this.GetLocalCell(cell).Valid = false;
    }

    PositionQuery QueryPosition(IntVec3Local local)
    {
        var global = local.ToGlobal(this);
        var index = Chunk.GetCellIndex(local);
        return new() { Local = local, Chunk = this, Cell = this.GetLocalCell(index), CellIndex = index, Global = global, GlobalCellId = global.Id };
    }

    public byte GetBlockLight(IntVec3Local local)
        => this.GetBlockLight(local.X, local.Y, local.Z);
    
    public byte GetBlockLight(int x, int y, int z)
        => this.Light[GetCellIndex(x, y, z)].Block;

    public byte GetBlockLight(CellId cellIndex)
        => this.Light[cellIndex].Block;
    public byte GetBlockLight(PositionQuery pos)
     => this.Light[pos.CellIndex].Block;

    public byte GetSkylight(IntVec3Local local)
        => this.GetSkylight(local.X, local.Y, local.Z);

    public byte GetSkylight(CellId cellIndex)
       => this.Light[cellIndex].Sky;

    public float GetSkylightPercentage(IntVec3Local local)
        => (float)this.GetSkylight(local) / 15;

    public byte GetSkylight(int x, int y, int z)
    {
        if (z >= this.Map.GetMaxHeight())
            return 15;
        return this.Light[GetCellIndex(x, y, z)].Sky;
    }

    public void SetSkylight(IntVec3Local local, byte value)
        => this.SetSkylight(local.X, local.Y, local.Z, value);
    
    public void SetSkylight(int x, int y, int z, byte value)
    {
        this.Light[GetCellIndex(x, y, z)].Sky = value;
        var global = new IntVec3(this.Start.X + x, this.Start.Y + y, z);
        this.InvalidateLight(global);
    }
    public void SetSkylight(PositionQuery pos, byte value)
    {
        this.Light[pos.CellIndex].Sky = value;
        this.InvalidateLight(pos.Global);
    }
    public void SetSkylight(CellId index, byte value)
    {
        this.Light[index].Sky = value;
        this.InvalidateLight(Chunk.GetLocalFromIndex(index).ToGlobal(this));
    }
    public void SetBlockLight(IntVec3Local local, byte value)
    {
        var index = GetCellIndex(local);
        this.Light[index].Block = value;
        var global = local.ToGlobal(this);
        this.InvalidateLight(global);
    }
    public void SetBlockLight(CellId index, byte value)
    {
        this.Light[index].Block = value;
        var global = Chunk.GetLocalFromIndex(index).ToGlobal(this);
        this.InvalidateLight(global);
    }
    public void SetBlockLight(PositionQuery pos, byte value)
    {
        this.Light[pos.CellIndex].Block = value;
        this.InvalidateLight(pos.Global);
    }
    public bool InvalidateLight(IntVec3 global)
    {
        this.LightCache.Clear();
        if (this.Slices.Length != 0)
        {
            var z = global.Z;
            if (z > 0)
                this.InvalidateSlice(z - 1);
            this.InvalidateSlice(z);
            if (z < this.Map.GetMaxHeight() - 1)
                this.InvalidateSlice(z + 1);
        }
        return true;
    }

    public static bool TryGetFinalLight(MapBase map, IntVec3 global, out byte sky, out byte block)
        => TryGetFinalLight(map, global.X, global.Y, global.Z, out sky, out block);

    public static bool TryGetFinalLight(MapBase map, int globalX, int globalY, int globalZ, out byte sky, out byte block)
    {
        sky = 0;
        block = 0;
        if (globalZ > MapBase.MaxHeight - 1)
            return false;
        if (globalZ < 0)
            return false;

        var global = new IntVec3(globalX, globalY, globalZ);
        if (!map.TryGetChunk(global, out Chunk chunk))
        {
            // return full skylight if adjacent neighbor chunk doesn't exist?
            sky = 15;
            return false;
        }
        int lx = globalX - chunk.X * Chunk.Size;
        int ly = globalY - chunk.Y * Chunk.Size;
        byte finalsun = (byte)Math.Max(0, chunk.GetSkylight(lx, ly, globalZ) - map.GetSkyDarkness());
        sky = finalsun;
        block = chunk.GetBlockLight(lx, ly, globalZ);
        return true;
    }

    public static bool TryGetSunlight(MapBase map, IntVec3 global, out byte sunlight)
    {
        sunlight = 0;

        if (global.Z > map.GetMaxHeight() - 1)
            return false;
        if (global.Z < 0)
            return false;

        if (!map.TryGetChunk(global, out var chunk))
            return false;

        int x = global.X - chunk.Start.X;
        int y = global.Y - chunk.Start.Y;
        sunlight = chunk.GetSkylight(x, y, global.Z);
        return true;
    }

    #region Updating
    public void Update()
    {
        this.ValidateHeightmap();
        this.ValidateCells();
    }

    public void HitTestEntities(Camera camera)
    {
        foreach (var o in this.Entities)
            o.HitTest(camera);
    }

    private void ValidateHeightmap()
    {
        foreach (var pos in this.DirtyHeightmapColumns)
            //this.UpdateHeightMap(pos.X, pos.Y)
            this.UpdateHeightMapColumnWithLightSmart(pos.X, pos.Y);
        this.DirtyHeightmapColumns.Clear();
    }
    public void Tick()
    {
        this.TickEntities();
        this.TickBlockEntities();
        this.TickBlockTokens();
        this.TickControllers();
    }
    void TickControllers()
    {
        foreach (var c in this.Controllers)
            c.Tick();
    }
    void TickBlockTokens()
        => this.BlockDamageSystem.Tick();

    private void TickBlockEntities()
    {
        foreach (var blockentity in this.BlockEntitiesByPosition.ToList())
            blockentity.Value.Tick(this.Map, blockentity.Key.ToGlobal(this));
    }
    private void TickEntities()
    {
        var objectList = this.Entities.ToArray();
        var objCount = objectList.Length;
        for (int i = 0; i < objCount; i++)
        {
            var obj = objectList[i];
            if (obj.IsSpawned) // BECAUSE obj might have been despawned or disposed as a result of a previous object's tick, for example an item stack absorbing another item stack in the physicscomponent
                obj.Tick(); // make an item stack merge itself to the other stack instead of the other way around? so that i don't have to do this check
        }
    }
    #endregion

    #region Drawing
    public void DrawObjects(MySpriteBatch sb, Camera camera, Controller controller, MapBase map, SceneState scene)
    {
        foreach (var obj in this.Entities) //make a copy of the list first because currently the player character might be added while drawing
        {
            Vector3 global = obj.Global;
            if (global.Z > camera.DrawLevel + 1)// - 1)
                continue;
            var actor = map.Net.GetPlayer().ControllingEntity;
            if (camera.HideTerrainAbovePlayer && actor is not null)
                if (global.Z > actor.Transform.Global.Z + 2)// - 1)
                    continue;

            if (!map.TryGetCell(global, out Cell cell))
                continue;
            var local = obj.Cell.ToLocal();
            int x = local.X, y = local.Y, z = local.Z;
            // TODO: figure out a way to get depth from actual precise global coords instead of cell coords
            Coords.Rotate(camera, x, y, out float rx, out float ry);
            Vector3 rotated = new(rx, ry, z);

            if (!obj.TryGetComponent<SpriteComp>(out var spriteComp))
                continue;

            Sprite sprite = spriteComp.Sprite;
            Rectangle spriteBounds = sprite.GetBounds();
            Rectangle screenBounds = camera.GetScreenBounds(global, spriteBounds);
            screenBounds.X -= BordersEffect.Thickness;
            screenBounds.Y -= BordersEffect.Thickness;
            if (!camera.ViewPort.Intersects(screenBounds))
                continue;
            float cd = global.GetDrawDepth(map, camera);
            //var local = cell.LocalCoords;
            byte light = Math.Max((byte)(this.GetSkylight(local) - map.GetSkyDarkness()), this.GetBlockLight(local));
            float l = (light + 1) / 16f;
            Color color = new Color(l, l, l, 1);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

            obj.Draw(sb, new DrawObjectArgs(camera, controller, map, this, cell, spriteBounds, screenBounds, obj, color, cd));
            SpriteComp.DrawShadow(camera, spriteBounds, map, obj, cd, cd);

            if (scene.ObjectsDrawn.Contains(obj))
                throw new Exception();
            scene.ObjectsDrawn.Add(obj);
            scene.ObjectBounds.Add(obj, screenBounds);
        }
    }
    public void DrawInterface(SpriteBatch sb, Camera cam)
    {
        foreach (var obj in this.Entities)
            obj.DrawInterface(sb, cam);
        foreach (var blockentity in this.BlockEntitiesByPosition)
            blockentity.Value.DrawUI(sb, cam, blockentity.Key.ToGlobal(this));
        this.DrawBlockTokens(sb, cam);
    }
    static readonly float BlockTokenDrawThreshold = Ticks.FromSeconds(2);
    private void DrawBlockTokens(SpriteBatch sb, Camera camera)
    {
        this.BlockDamageSystem.DrawBlockTokens(sb, camera);
        //return;
        //if (camera.Zoom < 1)
        //    return;
        //foreach(var (pos, token) in this.BlockTokens)
        //    if(token.Lifetime < BlockTokenDrawThreshold)
        //        Bar.Draw(sb, camera, pos.ToGlobal(this), "Block HitPoints", token.HealthPercentage, camera.Zoom * .2f);
    }

    public void DrawHighlight(SpriteBatch sb, Rectangle bounds)
    {
        sb.Draw(UIManager.Highlight, bounds, null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.None, 0);
    }
    #endregion

    #region Saving and Loading
    public string GetDirectoryPath()
    {
        return this.Map.GetFullPath() + "/chunks/" + this.DirectoryName;
    }
    internal void SaveToFile()
    {
        Chunk copy = this.Clone();
        string filename = GetFilename(this.MapCoords);
        string newFile = "_" + filename;

        string directory = this.GetDirectoryPath();
        directory = @"/Saves/Worlds/" + this.Map.World.Name + "/" + this.Map.GetFolderName() + "/chunks/";

        string working = Directory.GetCurrentDirectory();
        string fullpath = this.Map.GetFullPath() + "/chunks/" + this.DirectoryName;

        if (!Directory.Exists(fullpath))
            Directory.CreateDirectory(fullpath);
        copy.SaveToFile(newFile);
        if (File.Exists(fullpath + filename))
            try
            {
                File.Replace(fullpath + newFile, fullpath + filename, fullpath + filename + ".bak");
                File.Delete(fullpath + filename + ".bak");
            }
            catch (IOException)
            {
                Server.Instance.ConsoleBox.Write(Color.Red, "SERVER", "Error saving Chunk " + copy.MapCoords.ToString());
                // recover back up here?
            }
        else
            File.Move(fullpath + newFile, fullpath + filename);

        Server.Instance.ConsoleBox.Write(Color.Lime, "SERVER", "Chunk " + copy.MapCoords.ToString() + " saved succesfully \"" + directory + filename + "\"");
    }
    internal string SaveToFile(string filename)
    {
        string directory = this.FullDirPath;
        DateTime now = DateTime.Now;
        SaveTag chunktag;
        using (var stream = new MemoryStream())
        {
            var writer = new BinaryWriter(stream);
            chunktag = this.SaveToTag();
            chunktag.WriteTo(writer);
            Compress(stream, directory + filename);
            stream.Close();
        }
        Console.WriteLine(filename + " saved in " + (DateTime.Now - now).ToString());
        return directory + GetFilename(this.MapCoords);
    }

    private Dictionary<BlockEntity, List<IntVec3>> GetDistinctBlockEntities()
    {
        var distinct = new Dictionary<BlockEntity, List<IntVec3>>();
        foreach (var ent in this.BlockEntitiesByPosition)
        {
            if (!distinct.TryGetValue(ent.Value, out var existing))
            {
                existing = new List<IntVec3>();
                distinct.Add(ent.Value, existing);
            }
            existing.Add(ent.Key);
        }
        return distinct;
    }

    public static Chunk Load(MapBase map, string fullpath)
    {
        string filename = fullpath.Split('\\').Last();
        string[] c = filename.Split('.');
        var coords = new Vector2(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
        var chunk = new Chunk(coords)
        {
            Map = map
        };
        using (FileStream stream = new FileStream(fullpath, FileMode.Open))
        {
            var buffer = DecompressAll(stream);
            using MemoryStream decompressedStream = new(buffer);
            using BinaryReader reader = new(decompressedStream);
            SaveTag chunktag = SaveTag.Read(reader);
            chunk.LoadFromTag(chunktag);
            reader.Close();
        }
        return chunk;
    }

    private SaveTag SaveBlockEntitiesDistinct()
    {
        var blockEntitiesTag = new SaveTag(SaveTag.Types.List, "BlockEntities", SaveTag.Types.Compound);
        var distinct = this.GetDistinctBlockEntities();
        foreach (var ent in distinct)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, "");
            var origin = ent.Key.OriginGlobal;
            origin.Save(tag, "OriginGlobal");

            if (this.Contains(origin)) // ONLY SAVE BLOCKENTITY IF THE ORIGIN IS IN THIS CHUNK
            {
                var entitysavetag = ent.Key.Save("Entity");
                if (entitysavetag is not null)
                    tag.Add(entitysavetag);
            }
            else
                tag.Add(ent.Value.Save("PositionsLocal")); // all local positions where the entity is occupying (NOT INCLUDING POSITIONS IN NEIGHBORING CHUNKS)
            blockEntitiesTag.Add(tag); // the block entity is saved ONCE in the chunk the origin is contained, and all occupied cells are saved with it (global positions)
                                       // secondary blockentity positions save only the global origin position and retrieve the blockentity on chunk load,
                                       // or if the origin chunk hasn't loaded yet, when it loads it registers the blockentity using the saved occupiedcells in the blockentity class
        }
        return blockEntitiesTag;
    }
    private void LoadBlockEntitiesDistinct(SaveTag chunktag)
    {
        if (chunktag.TryGetTag("BlockEntities", out var blentitiesjTag))
            foreach (SaveTag tag in blentitiesjTag.Value as List<SaveTag>)
            {
                var origin = tag.LoadIntVec3("OriginGlobal");

                if (this.Contains(origin))
                {
                    //var block = this[origin.ToLocal()].Block;
                    //var entity = block.BlockDef.CreateEntity(origin);
                    //tag.TryGetTag("Entity", entity.Load);
                    //entity.Def = block.BlockDef;
                    var entity = BlockEntity.Create(tag["Entity"]);

                    foreach (var global in entity.CellsOccupied)
                    {
                        if (this.Contains(global))
                            this.SetBlockEntity(entity, global.ToLocal()); // TODO add chunk in map before finishing loading??
                        else
                        {
                            if (this.Map.TryGetChunk(global, out var nchunk))
                                nchunk.SetBlockEntity(entity, global.ToLocal());
                        }
                    }
                }
                else
                {
                    var positions = tag["PositionsLocal"].LoadListVector3();

                    if (this.Map.TryGetBlockEntity(origin, out var entity))
                    {
                        foreach (var local in positions)
                            this.BlockEntitiesByPosition[local] = entity;
                    }
                }
            }
    }
    private void WriteBlockEntitiesDistinct(IDataWriter w)
    {
        var distinct = this.GetDistinctBlockEntities();
        w.Write(distinct.Count);
        foreach (var ent in distinct)
        {
            var entity = ent.Key;
            w.Write(entity.OriginGlobal);
            if (this.Contains(entity.OriginGlobal))
            {
                ent.Key.Write(w);
            }
            else
            {
                w.Write(ent.Value); // if this chunk doesnt contain the blockentity origin, only write the local cells that the blockentity appears in
            }
        }
    }
    private void ReadBlockEntitiesDistinct(IDataReader r)
    {
        int blockEntityCount = r.ReadInt32();
        for (int i = 0; i < blockEntityCount; i++)
        {
            var originGlobal = r.ReadIntVec3();
            if (this.Contains(originGlobal))
            {
                var entity = BlockEntity.Create(r);
                foreach (var global in entity.CellsOccupied)
                {
                    if (this.Contains(global))
                        this.SetBlockEntity(entity, global.ToLocal());
                    else
                    {
                        if (this.Map.TryGetChunk(global, out var nchunk))
                            nchunk.SetBlockEntity(entity, global.ToLocal());
                    }
                }
            }
            else
            {
                var positionsLocal = r.ReadListIntVec3();

                if (this.Map.TryGetBlockEntity(originGlobal, out var entity))
                    foreach (var local in positionsLocal)
                        this.BlockEntitiesByPosition[local] = entity;
            }
        }
    }

    public static void Compress(Stream stream, string filename)
    {
        using (stream)
        {
            stream.Position = 0;
            using FileStream outFile = File.Create(filename);
            using GZipStream zip = new(outFile, CompressionMode.Compress);
            stream.CopyTo(zip);
        }
    }
    public static MemoryStream Decompress(FileStream compressed)
    {
        using (compressed)
        {
            using GZipStream decompress = new(compressed, CompressionMode.Decompress);
            MemoryStream memory = new MemoryStream();
            decompress.CopyTo(memory);
            memory.Position = 0;
            return memory;
        }
    }
    public static byte[] DecompressAll(FileStream compressed)
    {
        byte[] buffer;
        using (GZipStream decompress = new(compressed, CompressionMode.Decompress))
        {
            using MemoryStream memory = new();
            decompress.CopyTo(memory);
            memory.Position = 0;
            buffer = new byte[memory.Length];
            memory.Read(buffer, 0, buffer.Length);
        }
        return buffer;
    }
    public static string GetFilename(Vector2 pos)
    {
        return pos.X.ToString() + "." + pos.Y.ToString() + ".chunk.sat";
    }
    public static string GetDirName(Vector2 pos)
    {
        return pos.X.ToString() + "." + pos.Y.ToString() + "/";
    }
    #endregion

    public void OnCameraRotated(Camera camera)
    {
        this.LightCache.Clear();
    }

    #region Serialization

    public static Chunk Create(MapBase map, IDataReader reader)
    {
        var chunk = new Chunk() { Map = map };
        chunk.Read(reader);
        return chunk;
    }
    public static Chunk Create(IDataReader reader)
    {
        Chunk chunk = new();
        chunk.Read(reader);
        return chunk;
    }
    public void Write(IDataWriter writer)
    {
        writer.Write(this.MapCoords);
        writer.Write(this.LightValid);
        writer.Write(this.EdgesValid);

        var serializer = new ChunkSerializer();
        serializer.Serialize(this, writer);

        // save only entity refids, for entities to be claimed from the world entity registry during deserialization
        var refids = this.Entities.Select(o => o.RefId).ToList();
        if (refids.Any(c => c == 0))
            throw new Exception();
        writer.Write(refids);

        this.WriteBlockEntitiesDistinct(writer);
        for (int j = 0; j < Size; j++)
            for (int i = 0; i < Size; i++)
                //for (int j = 0; j < Size; j++)
                writer.Write(this.HeightMap[i][j]);

        //writer.Write(this.Sunlight);
        //writer.Write(this.BlockLight);

        this.Light.WriteImmutable(writer);
    }
    void Read(IDataReader reader)
    {
        //this.MapCoords = reader.ReadVector2();
        this.MapCoords = reader.ReadIntVec2();

        this.LightValid = reader.ReadBoolean();
        this.EdgesValid = reader.ReadBoolean();

        // TODO: OPTIMIZE
        this.InitCells();

        var serializer = new ChunkSerializer();
        serializer.Deserialize(this, reader);

        var entityRefIds = reader.ReadListEntityRefId();
        foreach (var refId in entityRefIds)
            this.Add(this.Map.World.Get(refId));
        this.ReadBlockEntitiesDistinct(reader);
        for (int j = 0; j < Size; j++)
            for (int i = 0; i < Size; i++)
                this.HeightMap[i][j] = reader.ReadInt32();

        //this.Sunlight = reader.ReadBytes(Volume);//.ToList();
        //this.BlockLight = reader.ReadBytes(Volume);

        this.Light.ReadImmutable(reader);
    }
    #endregion

    public string FullDirPath => this.Map.GetFullPath() + "/chunks/" + this.DirectoryName;

    public string DirectoryName => (this.MapCoords.X.ToString() + "." + this.MapCoords.Y.ToString()) + "/";


    public Canvas Canvas;

    public void Build(Camera cam)
    {
        this.ValidateSlicesNew(cam);
        this.Valid = true;
    }

    public void DrawOpaqueLayers(Camera cam, Effect effect)
    {
        Coords.Iso(cam, this.MapCoords.X * Chunk.Size, this.MapCoords.Y * Chunk.Size, 0, out float x, out float y);
        Coords.Rotate(cam, this.MapCoords.X, this.MapCoords.Y, out int rotx, out int roty);
        var world = Matrix.CreateTranslation(new Vector3(x, y, ((rotx + roty) * Chunk.Size)));
        effect.Parameters["World"].SetValue(world);
        effect.CurrentTechnique.Passes["Pass1"].Apply();
        EffectParameter effectHideWalls = effect.Parameters["HideWalls"];
        effectHideWalls.SetValue(Engine.HideWalls);
        effect.CurrentTechnique.Passes["Pass1"].Apply();
        int foglvel = cam.GetFogLevel();
        for (int i = foglvel; i <= cam.MaxDrawZ; i++)
        {
            var slice = this.Slices[i];
            slice.Canvas.Opaque.Draw();
            if (i == cam.MaxDrawZ && cam.DrawTopSlice)
                slice.Cover.Opaque.Draw();
            if (!cam.HideWalls)
                slice.Canvas.WallHidable.Draw();
        }
        effectHideWalls.SetValue(false);
        effect.CurrentTechnique.Passes["Pass1"].Apply();
        for (int i = foglvel; i <= cam.MaxDrawZ; i++)
        {
            var slice = this.Slices[i];
            slice.Canvas.NonOpaque.Draw();
            if (i == cam.MaxDrawZ && cam.DrawTopSlice)
                slice.Cover.NonOpaque.Draw();
        }

        foreach (var blockentity in this.BlockEntitiesByPosition)
            blockentity.Value.Draw(cam, this.Map, blockentity.Key.ToGlobal(this));
    }
    public void DrawTransparentLayers(Camera cam, Effect effect)
    {
        Coords.Iso(cam, this.MapCoords.X * Chunk.Size, this.MapCoords.Y * Chunk.Size, 0, out float x, out float y);
        Coords.Rotate(cam, this.MapCoords.X, this.MapCoords.Y, out int rotx, out int roty);
        var world = Matrix.CreateTranslation(new Vector3(x, y, ((rotx + roty) * Chunk.Size)));
        effect.Parameters["World"].SetValue(world);
        effect.CurrentTechnique.Passes["Pass1"].Apply();
        // no need to apply pass?
        int foglvel = (int)Math.Max(0, cam.LastZTarget - Camera.FogZOffset - Camera.FogFadeLength);
        for (int i = foglvel; i <= cam.MaxDrawZ; i++)
        {
            var slice = this.Slices[i];
            slice.Canvas.Transparent.Draw();
            if (cam.DrawZones)
                slice.Canvas.Designations.Draw();
        }
        if (cam.DrawTopSlice && !cam.MysteriousBlocks)
        {
            var slice = this.Slices[cam.MaxDrawZ];
            slice.Cover.Transparent.Draw();
            if (cam.DrawZones)
                slice.Cover.Designations.Draw();
        }
    }
    internal bool Contains(Vector3 global)
        => global.GetChunkCoords() == this.MapCoords;

    public SaveTag SaveToTag()
    {
        string.Format("saving chunk {0}", this.MapCoords).ToConsole();

        var chunktag = new SaveTag(SaveTag.Types.Compound, "Chunk");

        var heightTag = new SaveTag(SaveTag.Types.List, "Heightmap", SaveTag.Types.Byte);
        var visibleCells = new SaveTag(SaveTag.Types.List, "VisibleCells", SaveTag.Types.Int);
        var lightTag = new SaveTag(SaveTag.Types.List, "Light", SaveTag.Types.Byte);

        var sw = Stopwatch.StartNew();

        var serializer = new ChunkSerializer();
        serializer.Serialize(this, chunktag);

        sw.Stop();
        string.Format("cells saved in {0} ms", sw.ElapsedMilliseconds).ToConsole();

        sw.Restart();
        int n = 0;
        foreach (Cell cell in this.Cells)
        {
            //byte light = (byte)((this.Sunlight[n] << 4) + this.BlockLight[n++]);
            byte light = (byte)((this.Light[n].Sky << 4) + this.Light[n++].Block);
            lightTag.Add(new SaveTag(SaveTag.Types.Byte, "", light));
        }
        sw.Stop();
        string.Format("light saved in {0} ms", sw.ElapsedMilliseconds).ToConsole();

        sw.Restart();
        for (int j = 0; j < Size; j++)
            for (int i = 0; i < Size; i++)
                heightTag.Add(new SaveTag(SaveTag.Types.Byte, "", (byte)this.HeightMap[i][j]));
        sw.Stop();
        string.Format("heightmap saved in {0} ms", sw.ElapsedMilliseconds).ToConsole();

        var entityRefIds = this.Entities.Select(e => e.RefId).ToList();
        var entitiestag = entityRefIds.Save("Entities");

        var blockEntitiesTag = this.SaveBlockEntitiesDistinct();

        chunktag.Add(new SaveTag(SaveTag.Types.Bool, "LightValid", this.LightValid));
        chunktag.Add(new SaveTag(SaveTag.Types.Bool, "EdgesValid", this.EdgesValid));
        chunktag.Add(lightTag);
        chunktag.Add(heightTag);
        chunktag.Add(visibleCells);
        chunktag.Add(entitiestag);
        chunktag.Add(blockEntitiesTag);
        chunktag.Add(this.RandomOrderedCells.Save("RandomOrderedCells"));
        string.Format("saved chunk {0}", this.MapCoords).ToConsole();
        return chunktag;
    }

    internal Chunk LoadFromTag(SaveTag chunktag)
    {
        this.LightValid = chunktag.TagValueOrDefault<bool>("LightValid", false);
        this.EdgesValid = chunktag.TagValueOrDefault<bool>("EdgesValid", false);

        var lightTag = chunktag["Light"].Value as List<SaveTag>;

        var serializer = new ChunkSerializer();
        serializer.Deserialize(this, chunktag);

        var n = 0;
        for (int h = 0; h < MapBase.MaxHeight; h++)
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                {
                    byte light = (byte)lightTag[n].Value;
                    var sunlight = (byte)((light & 0xF0) >> 4);
                    var blocklight = (byte)(light & 0x0F);
                    this.Light[n] = new() { Sky = sunlight, Block = blocklight };
                    n++;
                }

        var heightTag = chunktag["Heightmap"].Value as List<SaveTag>;
        n = 0;
        for (int j = 0; j < Size; j++)
            for (int i = 0; i < Size; i++)
                this.HeightMap[i][j] = (byte)heightTag[n++].Value;

        var list = chunktag.LoadListEntityRefId("Entities");
        foreach (var refId in list)
            this.Add(this.Map.World.Get(refId));

        this.LoadBlockEntitiesDistinct(chunktag);

        this._RandomOrderedCells = chunktag.LoadArrayIntVec3("RandomOrderedCells");
        return this;
    }

    internal bool IsSolid(IntVec3 local)
    {
        if (local.Z > this.Map.GetMaxHeight() - 1)
            return false;
        return this[local].IsSolid();
    }

    public void ValidateSlicesNew(Camera cam)
    {
        var frontmost = UpdateFrontmostXY(cam);
        var count = this.Slices.Length;
        for (int i = 0; i < count; i++)
        {
            var slice = this.Slices[i];
            if (slice is null)
            {
                slice = new Slice();
                this.Slices[i] = slice;
            }
            if (slice.Valid)
                continue;
            this.BuildSliceNew(slice, cam, this.Map, i, frontmost);
            slice.Valid = true;
        }
    }

    private (int x, int y) UpdateFrontmostXY(Camera cam)
    {
        int frontCellX = 0, frontCellY = 0;
        var mapSizeInChunks = this.Map.GetSizeInChunks();
        switch ((int)cam.Rotation)
        {
            case 0:
                frontCellX = frontCellY = mapSizeInChunks * Size - 1;
                break;
            case 1:
                frontCellX = mapSizeInChunks * Size - 1;
                break;
            case 2:
                break;
            case 3:
                frontCellY = mapSizeInChunks * Size - 1;
                break;
            default:
                break;
        }
        return (frontCellX, frontCellY);
    }

    public void BuildSliceNew(Slice slice, Camera camera, MapBase map, int z, (int x, int y) frontCells)
    {
        var maxCapacity = Size * Size;
        var obstructed = new List<IntVec3Local>(maxCapacity);
        var mysterious = new List<IntVec3Local>(maxCapacity);
        var visible = new List<IntVec3Local>(maxCapacity);
        var frontmost = new List<IntVec3Local>(maxCapacity);
        var frontmostMysterious = new List<IntVec3Local>(maxCapacity);

        var canvas = new Canvas(Game1.Instance.GraphicsDevice, visible.Count + frontmost.Count + frontmostMysterious.Count);

        for (int i = 0; i < Chunk.Size; i++)
            for (int j = 0; j < Chunk.Size; j++)
            {
                var local = new IntVec3(i, j, z);
                var block = this.GetBlock(local);
                var global = local.ToGlobal(this);
                var isair = block == BlockDefOf.Air.Block;// BlockDefOf.Air;
                // HACK
                if (isair && this.Map.Town.ConstructionsManager.IsDesignatedConstruction(global))
                    //if (isair && this.Map.Town.DesignationManager.IsDesignation(global, DesignationDefOf.Construct)) // HACK
                    camera.DrawBlock(canvas, BlockDefOf.Designation.Block, map, this, global);

                var isobstructed = !map.IsVisible(global);// || !(global.X == frontCellX || global.Y == frontCellY);
                var isundiscovered = map.IsUndiscovered(global);
                var ismysterious = camera.MysteriousBlocks && isundiscovered;

                if (global.X == frontCells.x || global.Y == frontCells.y)
                {
                    if (ismysterious)
                        frontmostMysterious.Add(local);
                    if (!isair)
                        frontmost.Add(local);
                }
                else
                {
                    if (ismysterious)
                        mysterious.Add(local);
                    else
                    {
                        if (!isair)
                        {
                            if (isobstructed)
                                obstructed.Add(local);
                            else
                                visible.Add(local);
                        }
                    }
                }
            }

        var topCover = new Canvas(Game1.Instance.GraphicsDevice, obstructed.Count + mysterious.Count);

        foreach (var cell in obstructed)
            camera.DrawCell(topCover, map, this, cell);//, cell.LocalCoords.ToGlobal(this));

        foreach (var cell in mysterious)
            camera.DrawUnknown(topCover, map, this, cell);//);


        foreach (var cell in visible)
            camera.DrawCell(canvas, map, this, cell);//, cell.LocalCoords.ToGlobal(this));

        foreach (var cell in frontmost)
            camera.DrawCell(canvas, map, this, cell);//, cell.LocalCoords.ToGlobal(this));

        foreach (var cell in frontmostMysterious)
            camera.DrawUnknown(canvas, map, this, cell);

        slice.Canvas = canvas;
        slice.Cover = topCover;
    }


    public class Slice
    {
        public bool Valid;
        public Canvas Canvas;
        public Canvas Cover;
        public MySpriteBatch Unknown;
    }
    public bool TryGetBlockEntity(IntVec3 local, out BlockEntity entity)
    {
        return this.BlockEntitiesByPosition.TryGetValue(local, out entity);
    }

    public void SetBlockEntity(BlockEntity entity, IntVec3 local)
    {
        entity.Map = this.Map;
        this.BlockEntitiesByPosition[local] = entity;
    }
    public bool TryRemoveBlockEntity(IntVec3 local, out BlockEntity entity)
    {
        if (this.BlockEntitiesByPosition.TryGetValue(local, out entity))
        {
            foreach (var cell in entity.CellsOccupied)
                this.BlockEntitiesByPosition.Remove(cell.ToLocal());
        }
        return entity is not null;
    }

    public IEnumerable<(IntVec3Local local, BlockEntity entity)> GetBlockEntitiesByPosition()
    {
        foreach (var be in this.BlockEntitiesByPosition)
            yield return (be.Key, be.Value);
    }
    internal IBlockHealth GetBlockToken(IntVec3Local local) => this.BlockDamageSystem.GetBlockHealth(local);

    internal void ApplyBlockWork(IntVec3Local local, int work)
    {
        var result = this.BlockDamageSystem.ApplyDamage(local, work);
        if (result != BlockHealthToken.BlockDamageResult.NoChange)
            this.InvalidateSlice(local.Z);
        switch (result)
        {
            case BlockHealthToken.BlockDamageResult.DamageLevelChanged:
                this.InvalidateSlice(local.Z);
                break;

            case BlockHealthToken.BlockDamageResult.HitPointsDepleted:
                this.Map.Events.Post(new BlockHitPointsDepletedEvent(local.ToGlobal(this)));
                break;

            default:
                break;
        }
        ;
        this.Map.Events.Post(new BlockDamagedEvent(this.Map, local.ToGlobal(this), work));
    }

    internal int GetData(CellId cellIndex) => this.Cells[cellIndex].Data.Data;
    internal void SetData(CellId cellIndex, int data)
    {
        this.Cells[cellIndex].Data = new(data);
        var local = GetLocalFromIndex(cellIndex);
        this.InvalidateFinal(local);
    }

    internal byte GetBlockData(CellId cellIndex) => this.Cells[cellIndex].BlockData;
    internal byte GetBlockData(IntVec3Local local) => this.Cells[GetCellIndex(local)].BlockData;

    internal int GetVariation(CellId cellIndex) => this.Cells[cellIndex].Variation;
    internal int GetVariation(IntVec3Local local) => this.Cells[GetCellIndex(local)].Variation;

    internal void SetBlockData(IntVec3Local local, byte blockData)
    {
        this.GetLocalCell(local).BlockData = blockData;
        this.InvalidateFinal(local);
    }
    internal void SetBlockData(CellId cellIndex, byte blockData)
    {
        this.GetLocalCell(cellIndex).BlockData = blockData;
        var local = GetLocalFromIndex(cellIndex);
        this.InvalidateFinal(local);
    }
    internal Block GetBlock(IntVec3Local local) => this.Cells[GetCellIndex(local)].Block;
    internal Block GetBlock(CellId cellIndex) => this.Cells[cellIndex].Block;
    internal void SetBlock(IntVec3Local local, Block block)
    {
        this.GetLocalCell(local).Block = block;
        block.OnPlaced(new CellQuery(this, local));
        this.InvalidateFinal(local);
    }
    internal void SetBlock(CellId cellIndex, Block block)
    {
        this.GetLocalCell(cellIndex).Block = block;
        var local = GetLocalFromIndex(cellIndex);
        this.InvalidateFinal(local);
    }
    internal MaterialDef GetMaterial(CellId cellIndex) => this.Cells[cellIndex].Material;
    internal void SetMaterial(IntVec3Local local, MaterialDef material)
    {
        this.GetLocalCell(local).Material = material;
        this.InvalidateFinal(local);
    }
    internal void SetMaterial(CellId cellIndex, MaterialDef material)
    {
        this.GetLocalCell(cellIndex).Material = material;
        var local = GetLocalFromIndex(cellIndex);
        this.InvalidateFinal(local);
    }

    internal void SetVariation(CellId cellIndex, int variation)
    {
        this.GetLocalCell(cellIndex).Variation = variation;
        var local = GetLocalFromIndex(cellIndex);
        this.InvalidateFinal(local);
    }
    private void InvalidateFinal(IntVec3Local local)
    {
        this.InvalidateCell(local);
        this.InvalidateSlice(local.Z);
        this.Map.Events.Post(new CellsInvalidatedEvent(this.Map, [local.ToGlobal(this)]));
    }

    internal void WriteCell(CellId cellIndex, BlockDef block, MaterialDef material, int? variation, byte? blockdata, int? data)
    {
        if (block is not null)
            this.SetBlock(cellIndex, block.Block);
        if (material is not null)
            this.SetMaterial(cellIndex, material);
        if (blockdata.HasValue)
            this.SetBlockData(cellIndex, blockdata.Value);
        if (variation.HasValue)
            this.SetVariation(cellIndex, variation.Value);
        if (data.HasValue)
            this.SetData(cellIndex, data.Value);
        var local = Chunk.GetLocalFromIndex(cellIndex);
        this.InvalidateCell(local);
        this.InvalidateSlice(local.Z);
        this.Map.Events.Post(new CellsInvalidatedEvent(this.Map, [local.ToGlobal(this)]));
    }
    static public IEnumerable<IntVec2> Columns
    {
        get
        {
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    yield return new IntVec2(i, j);
        }
    }
}