using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Graphics.Particles;
using Project1.Core.Helpers;
using Project1.Core.Map;
using Project1.Core.Networking;
using Project1.Core.Networking.Simulation;
using Project1.Core.Screens;
using Project1.Core.Simulation.Lighting;
using Project1.Core.Simulation.Physics;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns;
using Project1.Core.Towns.Stockpiles;
using Project1.Core.UI.Hud;
using Project1.Core.WorldGen;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Interfaces;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Project1.Core.Simulation;

public sealed class EntityQueryEnumerator() : ISaveableNewNew<EntityQueryEnumerator>
{
    int chunkIndex = 0, entityIndex = 0;
    public bool TryGetNext(MapBase map, out Entity entity)
    {
        entity = this.GetNext(map);
        return entity is not null;
    }
    public Entity GetNext(MapBase map)
    {
        if (map is null)
            return null;
        int chunksTried = 0;
        var chunksSnapshot = map.ActiveChunks.Values.ToArray();
        while (chunksTried < chunksSnapshot.Length)
        {
            var chunk = chunksSnapshot[chunkIndex];

            if (entityIndex < chunk.Entities.Count)
            {
                return chunk.Entities[entityIndex++];
            }
            else
            {
                // move to next chunk
                entityIndex = 0;
                chunkIndex = (chunkIndex + 1) % chunksSnapshot.Length;
                chunksTried++;
            }
        }

        // If all chunks are empty
        return null;
    }

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("ChunkIndex", this.chunkIndex);
        tag.Save("EntityIndex", this.chunkIndex);
        return tag;
    }

    public static EntityQueryEnumerator Create(SaveTag tag)
    {
        var chunkindex = tag.LoadInt("ChunkIndex");
        var entityindex = tag.LoadInt("EntityIndex");
        return new() { chunkIndex = chunkindex, entityIndex = entityindex };
    }
}
public abstract class MapBase : Inspectable
{
    public MapSize Size;
    public class MapSize(
        string name, int blocks) : INamed
    {
        public string Name { get; private set; } = name;
        public int Blocks { get; private set; } = blocks;
        public int Chunks { get; private set; } = blocks / Chunk.Size;

        public static readonly MapSize Micro = new("Micro", 32);
        public static readonly MapSize Tiny = new("Tiny", 64);
        public static readonly MapSize Small = new("Small", 128);
        public static readonly MapSize Normal = new("Normal", 256);
        public static readonly MapSize Huge = new("Huge", 512);

        public static readonly MapSize Default = Micro;

        public static readonly List<MapSize> AllSizes = [Micro, Tiny, Small, Normal, Huge];
    }
    public readonly EventBus Events = new();
    public override string LabelReadable => this.ToString();
    //public Camera Camera;
    public static float IconOffset = 0;
    public Biome Biome = new();
    protected Queue<IntVec3> RandomBlockUpdateQueue = new();
    public LightingEngine LightingEngine;
    public WorldBase World;
    //public Dictionary<IntVec2, Chunk> ActiveChunks = [];
    public Dictionary<int, Chunk> ActiveChunks = [];
    public MapId ID;
    public NetEndpoint Net => field ??= this.World.Net;
    public GameObject PlayerCharacter;
    public ParticleManager ParticleManager;
    public RegionManager Regions;
    //public StockpileManager Stockpiles;
    public HaulingManager Hauling;
    //public ConversationSystem Conversations;
    protected EntityTrackerPerCell EntityTracker;
    internal List<MapComponent> Comps = [];
    internal List<SimulationSystem> SimulationSystems = [];
    internal CollisionSystem Collisions;
    public float Sunlight;
    public abstract Color GetAmbientColor();
    public abstract void SetAmbientColor(Color color);
    public abstract double GetDayTimeNormal();
    public abstract Texture2D GetThumbnail();
    public abstract float LoadProgress { get; }
    public ulong CurrentTick => this.World.CurrentTick;
    public TimeSpan Clock => this.World.Clock;
    public abstract Vector2 GetOffset();
    public static Texture2D Shadow;
    internal static void Initialize()
    {
        Generator.InitGradient3();
        Shadow = Game1.Instance.Content.Load<Texture2D>("Graphics/shadow");
    }
    public Vector2 Coordinates;
    public abstract string GetName();
    public readonly float PlantDensityTarget = .1f;

    public abstract Dictionary<int, Chunk> GetActiveChunks();
    public abstract bool AddChunk(Chunk chunk);
    public abstract IEnumerable<Entity> GetObjects(IntVec3 min, IntVec3 max);
    public abstract IEnumerable<Entity> GetObjects(BoundingBox box);
    public abstract List<Entity> GetEntitiesAroundChunk(Vector3 global);
    public float Gravity => this.World.Gravity;

    public IEnumerable<BlockEntity> BlockEntities => this.ActiveChunks.Values.SelectMany(ch => ch.BlockEntities).Distinct();
    public int ChunkVolume => Chunk.Size * Chunk.Size * this.GetMaxHeight();
    public int Volume => this.ActiveChunks.Count * this.ChunkVolume;
    public Random Random => this.World.Random;
    public IEnumerable<T> GetBlockEntityComps<T>() where T : BlockComp
        => this.BlockEntities
            .Select(be => be.GetCompOrDefault<T>())
            .Where(c => c is not null);
    
    public static int MaxHeight = 128;

    public abstract int GetMaxHeight();
    public abstract int GetSizeInChunks();

    protected int[] _randomOrderedChunkIndices;
    protected int[] RandomOrderedChunkIndices
    {
        get
        {
            if (this._randomOrderedChunkIndices is null)
            {
                this._randomOrderedChunkIndices = Enumerable.Range(0, this.ActiveChunks.Count).Shuffle(this.Random).ToArray();
                // force initialization on all chunks
                foreach (var ch in this.ActiveChunks.Values)
                    _ = ch.GetRandomCellInOrder(0);
            }
            return this._randomOrderedChunkIndices;
        }
    }

    int RandomChunkIndex, RandomCellIndex;
    internal void OnCameraRotated(Renderer renderer)
    {
        foreach (var chunk in this.GetActiveChunks())
        {
            chunk.Value.OnCameraRotated(renderer);
            chunk.Value.Invalidate();
        }
        this.Town.OnCameraRotated(renderer);
    }

    public IntVec3 GetNextRandomCell()
    {
        var randomChunk = this.ActiveChunks.Values.ElementAt(this.RandomOrderedChunkIndices[this.RandomChunkIndex]);
        var randomCell = randomChunk.GetRandomCellInOrder(this.RandomCellIndex);
        this.RandomChunkIndex++;
        if (this.RandomChunkIndex >= this.ActiveChunks.Count)
        {
            this.RandomChunkIndex = 0;
            this.RandomCellIndex++;
            if (this.RandomCellIndex >= this.ChunkVolume)
                this.RandomCellIndex = 0;
        }

        return randomCell.ToGlobal(randomChunk);
    }
    internal bool IsValidBuildSpot(IntVec3 pos)
    {
        var error = "";
        return this.IsValidBuildSpot(pos, ref error);
    }
    internal bool IsValidBuildSpot(IntVec3 pos, ref string errorText)
    {
        if (this.BlockEntities.Any(e => e.ReservedInteractionCells.Contains(pos)))
        {
            errorText = "Building blocked by interaction spot";
            return false;
        }
        return true;
    }
    internal bool IsValidBuildSpot(IntVec3 pos, bool showError)
    {
        var error = "";
        var result = this.IsValidBuildSpot(pos, ref error);
        if (!result && showError)
            Log.Warning(error);
        return result;
    }
    public IEnumerable<Cell> GetAllCells()
    {
        foreach (var ch in this.ActiveChunks.Values)
            foreach (var c in ch.Cells)
                yield return c;
    }
    public IEnumerable<(Chunk chunk, Cell cell, CellId id)> GetAllCellsWithIndex()
    {
        foreach (var ch in this.ActiveChunks.Values)
            foreach (var c in ch.GetAllCellsWithIndex())
                yield return (ch, c.cell, c.index);
    }
    internal void ResolveReferences()
    {
        this.World.ResolveReferences();
        this.Town.ResolveReferences();
        //this.Stockpiles.ResolveReferences();
        //this.Hauling.ResolveReferences();
        //this.EntityTracker.ResolveReferences();
        foreach (var comp in this.Comps)
            comp.ResolveReferences();
        foreach (var chunk in this.ActiveChunks.Values)
        {
            chunk.ResolveReferences();
        }
        foreach (var be in this.BlockEntities)
        {
            foreach (var comp in this.Comps)
                comp.Scan(be);
            this.Town.Scan(be);
        }
        foreach (var entity in this.Entities)
        {
            foreach (var comp in this.Comps)
                comp.Scan(entity);
            this.Town.Scan(entity);
        }
        foreach (var index in this.GetAllCellsWithIndex())
        {
            this.Town.Scan(index);
        }
    }
    internal void SyncSetCellData(IntVec3 global, byte data)
    {
        PacketsMap.SyncSetCellData(this, global, data);
    }
    internal void SetCellData(Vector3 global, byte v)
    {
        this.GetCell(global).BlockData = v;
        this.InvalidateCell(global);
    }
    public void RemoveBlock(IntVec3 vec, bool notify = true)
    {
        var global = Cell.GetOrigin(this, vec);
        var cell = this.GetCell(global);
        var block = cell.Block;
       
        var blockentity = this.GetBlockEntity(global);

        var parts = cell.GetParts(global);

        this.GetBlock(global).PreRemove(this, global); // preremove only center part or all parts?
        if (blockentity != null)
        {
            this.RemoveBlockEntity(global);
            blockentity.OnRemoved(this, global);
            blockentity.Dispose();
        }
        else
            foreach (var p in parts)
            {
                this.SetBlock(p, BlockDefOf.Air.Block, MaterialDefOf.Air, 0, 0, 0, notify);
                this.SetBlockLuminance(p, 0);
                // reenable physics of entities resting on block
                foreach (var entity in this.GetObjects(p - new IntVec3(1, 1, 0), p + new IntVec3(1, 1, 2)))
                    entity.Physics.Enable();
                    //PhysicsComponent.Enable(entity);

                var above = p.Above;
                this.GetBlock(above)?.BlockBelowChanged(this, above);
            }
    }
    /// <summary>
    /// starts and returns an async task handling map generation
    /// </summary>
    /// <returns></returns>
    public abstract Task Generate(bool showDialog);

    internal void RemoveBlocks(IEnumerable<IntVec3> positions, bool notify = true)
    {
        var nonAirPositions = positions.Where(vec => this.GetBlock(vec) != BlockDefOf.Air.Block).ToList();
        foreach (var global in nonAirPositions)
            this.RemoveBlock(global, false);
        if (notify)
            this.NotifyBlocksChanged(nonAirPositions);
    }
    public Block GetBlock(IntVec3 global)
    {
        if (!this.TryGetCell(global, out var cell))
            return null;
        return cell.Block;
    }
    public Block GetBlock(IntVec3 global, out Cell cell)
    {
        if (!this.TryGetCell(global, out cell))
            return null;
        return cell.Block;
    }

    public BlockEntity RemoveBlockEntity(IntVec3 global)
    {
        var chunk = this.GetChunk(global);
        var local = global.ToLocal();

        if (chunk.TryRemoveBlockEntity(local, out var entity))
        {
            foreach (var cell in entity.CellsOccupied)
                this.SetBlock(cell, BlockDefOf.Air);
            entity.Map = null;
            this.Events.Post(new BlockEntityRemovedEvent(entity));
            return entity;
        }
        return null;
        throw new Exception(); // for debugging
    }

    public void AddBlockEntity(BlockEntity entity)
    {
        foreach (var cell in entity.CellsOccupied)
        {
            var chunk = this.GetChunk(cell);
            var local = cell.ToLocal();
            chunk.SetBlockEntity(entity, local);
        }
        entity.OnSpawned(this);
        this.Events.Post(new BlockEntityAddedEvent(entity));
    }

    public void AttachCellToEntity(IntVec3 global, BlockEntity entity)
    {
        entity.CellsOccupied.Add(global);
        Chunk chunk = this.GetChunk(global);
        var local = global.ToLocal();
        chunk.SetBlockEntity(entity, local);
    }

    internal IntVec3 GetFrontOfBlock(IntVec3 global)
    {
        var cell = this.GetCell(global);
        return global + cell.Front;
    }

    internal IntVec3 GetBehindOfBlock(IntVec3 global)
    {
        var cell = this.GetCell(global);
        return global + cell.Back;
    }
    
    public bool TryGetBlockEntity(IntVec3 global, out BlockEntity entity)
    {
        entity = null;
        if (this.GetChunk(global) is not Chunk chunk)
            return false;
        return chunk.TryGetBlockEntity(global.ToLocal(), out entity);
    }
    
    public BlockEntity GetBlockEntity(IntVec3 global)
    {
        var chunk = this.GetChunk(global);
        chunk.TryGetBlockEntity(global.ToLocal(), out var entity);
        return entity;
    }

    public T GetBlockComp<T>(IntVec3 global) where T: BlockComp
    {
        return this.GetBlockEntity(global).GetComp<T>();
    }
    public bool TryGetBlockEntityComp<T>(IntVec3 global, out T comp) where T : BlockComp
    {
        if (!this.TryGetBlockEntity(global, out var entity))
        {
            comp = null;
            return false; 
        }
        comp = entity.GetComp<T>();
        return comp is not null;
    }
    public T GetBlockEntity<T>(IntVec3 global) where T : BlockEntity
    {
        var chunk = this.GetChunk(global);
        chunk.TryGetBlockEntity(global.ToLocal(), out var entity);

        return entity as T;
    }
    public virtual int GetHeightmapValue(int x, int y)
    {
        var global = new IntVec3(x, y, 0);
        var ch = this.GetChunk(global);
        if (ch == null)
            return int.MinValue;
        return ch.GetHeightMapValue(global);
    }
    public virtual int GetHeightmapValue(IntVec3 global)
    {
        var ch = this.GetChunk(global);
        if (ch == null)
            return int.MinValue;
        return ch.GetHeightMapValue(global.ToLocal());
    }

    internal bool IsAdjacentToSolid(Vector3 global)
    {
        foreach (var adj in VectorHelper.Adjacent)
        {
            var n = global + adj;
            if (this.Town.Map.IsSolid(n))
                return true;
        }
        return false;
    }
    internal double GetGradient(IntVec3 pos)
    {
        var x = pos.X;
        var y = pos.Y;
        var z = pos.Z;
        var chunk = this.GetChunk(x, y);
        var g = chunk.GetGradientAt(x - chunk.Start.X, y - chunk.Start.Y, z);
        return g;
    }
    public Cell GetCell(int x, int y, int z)
    {
        var chunk = this.GetChunk(x, y);
        var cell = chunk[x - chunk.Start.X, y - chunk.Start.Y, z];
        return cell;
    }
    public virtual Cell GetCell(Vector3 global)
    {
        var cell = global.ToCell();
        if (this.TryGetChunk(cell, out var chunk))
            return chunk[cell.X - chunk.Start.X, cell.Y - chunk.Start.Y, cell.Z];
        return null;
    }
    public Chunk GetChunk(Vector3 global)
    {
        if (this.TryGetChunk(global, out var chunk))
            return chunk;
        return null;
    }
    public Chunk GetChunk(IntVec3 global)
    {
        if (this.TryGetChunk(global.X, global.Y, global.Z, out var chunk))
            return chunk;
        return null;
    }
    public Chunk GetChunk(int x, int y)
    {
        int chunkX = x / Chunk.Size;
        int chunkY = y / Chunk.Size;
        return this.ActiveChunks[GetChunkKey(chunkX, chunkY)];
    }
    public List<Chunk> GetChunks(Vector2 pos, int radius = 1)
    {
        List<Chunk> list = [];
        int x = (int)pos.X, y = (int)pos.Y;
        for (int i = x - radius; i <= x + radius; i++)
            for (int j = y - radius; j <= y + radius; j++)
                if (this.ActiveChunks.TryGetValue(GetChunkKey(i, j), out Chunk ch))
                    list.Add(ch);
        return list;
    }
    public bool TryGetCell(Vector3 global, out Cell cell)
    {
        return this.TryGetAll(global, out _, out cell);
    }
    public bool TryGetChunk(Vector3 global, out Chunk chunk)
    {
        if (global.Z < 0 || global.Z >= MaxHeight)
        {
            chunk = null;
            return false;
        }
        var cell = global.ToCell();
        //int chunkX = (int)Math.Floor((float)cell.X / Chunk.Size);
        //int chunkY = (int)Math.Floor((float)cell.Y / Chunk.Size);
        int chunkX = cell.X >> 4;
        int chunkY = cell.Y >> 4;
        return this.ActiveChunks.TryGetValue(GetChunkKey(chunkX, chunkY), out chunk);
    }
    public bool TryGetChunk(int x, int y, int z, out Chunk chunk)
    {
        if (z < 0 || z >= MaxHeight)
        {
            chunk = null;
            return false;
        }
        //int chunkX = (int)Math.Floor((float)cell.X / Chunk.Size);
        //int chunkY = (int)Math.Floor((float)cell.Y / Chunk.Size);
        int chunkX = x >> 4;
        int chunkY = y >> 4;
        return this.ActiveChunks.TryGetValue(GetChunkKey(chunkX, chunkY), out chunk);
    }
    public static int GetChunkKey(int chunkX, int chunkY)
        => (chunkX << 16) | (chunkY & 0xFFFF);
    public static int GetChunkKey(IntVec2 chunkXY)
        => GetChunkKey(chunkXY.X, chunkXY.Y);
    //public override bool TryGetAll(int gx, int gy, int gz, out Chunk chunk, out Cell cell, out int lx, out int ly)
    //{
    //    if (gz > MaxHeight - 1 || gz < 0)
    //    {
    //        lx = 0;
    //        ly = 0;
    //        chunk = null;
    //        cell = null;
    //        return false;
    //    }
    //    if (this.TryGetChunk(gx, gy, out chunk))
    //    {
    //        lx = gx - (int)chunk.Start.X;
    //        ly = gy - (int)chunk.Start.Y;
    //        cell = chunk[Chunk.GetCellIndex(lx, ly, gz)];
    //        return true;
    //    }
    //    lx = 0;
    //    ly = 0;
    //    chunk = null;
    //    cell = null;
    //    return false;
    //}
    //public abstract bool TryGetAll(int gx, int gy, int gz, out Chunk chunk, out Cell cell, out int lx, out int ly);
    public bool TryQueryPosition(IntVec3 global, out PositionQuery query)
    {
        if (global.Z < 0 || global.Z >= MaxHeight)
        {
            query = default;
            return false;
        }

        int chunkX = global.X >> 4;
        int chunkY = global.Y >> 4;

        if (!ActiveChunks.TryGetValue(GetChunkKey(chunkX, chunkY), out var chunk))
        {
            query = default;
            return false;
        }

        int localX = global.X & 15;
        int localY = global.Y & 15;
        int localZ = global.Z;

        int index = (localZ << 8) | (localY << 4) | localX;

        var cell = chunk.Cells[index];

        query = new PositionQuery
        {
            Chunk = chunk,
            CellIndex = index,
            Global = global,
            Cell = cell,
            Local = new(localX, localY, localZ)
        };

        return true;
    }
    public bool TryQueryPositionOld(IntVec3 global, out PositionQuery query)
    {
        var chunk = this.GetChunk(global);
        if(chunk is null)
        {
            query = default;
            return false;
        }
        var local = global.ToLocal();
        var index = Chunk.GetCellIndex(local);
        var cell = chunk.GetLocalCell(index);
        query = new() { Chunk = chunk, CellIndex = index, Global = global, Local = local, Cell = cell };//, GlobalCellId = global.Id };
        return true;
    }
    public PositionQuery QueryPosition(IntVec3 global)
    {
        var chunk = this.GetChunk(global);
        var local = global.ToLocal();
        var index = Chunk.GetCellIndex(local);
        var cell = chunk.GetLocalCell(index);
        return new() { Chunk = chunk, CellIndex = index, Global = global, Local = local, Cell = cell, GlobalCellId = global.Id };
    }
    public bool TryGetAll(IntVec3 global, out Chunk chunk, out Cell cell, out IntVec3Local local)
    {
        //var z = global.Z;
        //if (z > MaxHeight - 1 || z < 0)
        //{
        //    local = default;
        //    chunk = null;
        //    cell = null;
        //    return false;
        //}
        if (this.TryGetChunk(global, out chunk))
        {
            //lx = gx - (int)chunk.Start.X;
            //ly = gy - (int)chunk.Start.Y;
            local = global.ToLocal();
            //cell = chunk[Chunk.GetCellIndex(lx, ly, gz)];
            cell = chunk.GetLocalCell(global);//
            return true;
        }
        local = default;
        chunk = null;
        cell = null;
        return false;
    }
    public bool TryGetAll(Vector3 global, out Chunk chunk, out Cell cell)
    {
        cell = null;
        chunk = null;
        var rounded = global.ToCell();
        if (rounded.Z < 0 || rounded.Z > this.World.MaxHeight - 1)
            return false;
        int chunkX = (int)Math.Floor((float)rounded.X / Chunk.Size);
        int chunkY = (int)Math.Floor((float)rounded.Y / Chunk.Size);
        if (this.ActiveChunks.TryGetValue(GetChunkKey(chunkX, chunkY), out chunk))
        {
            cell = chunk[(int)(rounded.X - chunk.Start.X), (int)(rounded.Y - chunk.Start.Y), (int)rounded.Z];
            return true;
        }
        return false;
    }
    internal bool IsStandableIn(Vector3 global)
    {
        var belowBlockStandableOn = this.GetBlock(global.Below()).IsStandableOn;
        var curblockStandableIn = this.GetBlock(global)?.IsStandableIn ?? true;
        return curblockStandableIn && belowBlockStandableOn;
    }
    internal bool IsStandableOn(Vector3 global)
    {
        var above = global.Above();
        if (!this.Contains(above))
            /// are entities allowed to stand on topmost blocks of a map?
            return true;// false;
        return this.GetBlock(global).IsStandableOn && this.GetBlock(above).IsStandableIn;
    }
    
    public virtual bool IsSolid(Vector3 global)
    {
        if (!this.TryGetCell(global, out Cell cell))
            return true; // return true to prevent crashing by trying to add object to missing chunk
        //return false; // return false to let entity attempt to enter unloaded chunk so we can handle the event of that


        var offset = global + new Vector3(0.5f, 0.5f, 0);
        var blockCoords = offset - offset.FloorXY();

        var issolid = cell.Block.IsSolid(cell, blockCoords);
        return issolid;
    }
    public virtual bool IsPathable(Vector3 global)
    {
        if (this.IsInBounds(global))
        {
            var cell = this.GetCell(global);
            return cell.Block.IsPathable(cell, global.ToBlock());
        }
        return false;
    }

    public bool Despawn(Entity obj)
    {
        if (obj.Map != this)
            return false;
        obj.OnDespawn(this);
        if (!this.Remove(obj)) // TODO: move this to map.despawn
            throw new Exception();
        obj.Map = null;
        this.Events.Post(new EntityDespawnedEvent(obj));
        this.EntityTracker.OnEntityDespawned(obj);
        return true;
    }
    internal bool Remove(Entity obj)
    {
        return this.GetChunk(obj.Global).Remove(obj);
    }
    internal void Add(Entity obj)
    {
        this.GetChunk(obj.Global).Add(obj);
    }
    public IEnumerable<GameObject> GetObjects(Vector3 global)
    {
        var ch = this.GetChunk(global);
        var objects = ch.Entities;
        var count = objects.Count;
        var globalIntVec3 = global.ToCell();
        for (int i = 0; i < count; i++)
        {
            var e = objects[i];
            if (e.Global.ToCell() == globalIntVec3)
                yield return e;
        }
    }
    

    internal virtual IReadOnlySet<Entity> GetEntitiesAt(IntVec3 pos)
        => this.EntityTracker.GetEntitiesAt(pos);
    //internal virtual IReadOnlySet<Entity> GetEntitiesAtNew(IntVec3 pos)
    //   => this.EntityTracker.GetEntitiesAt(pos);
    public bool IsCellEmpty(IntVec3 cell)// => !this.GetEntitiesAt(cell).Any();
        => this.GetEntitiesAt(cell).Count == 0;
    public abstract bool IsInBounds(Vector3 global);
    public abstract void SetSkyLight(IntVec3 global, byte value);
    public abstract void SetBlockLight(IntVec3 global, byte value);
    
    /// <summary>
    /// Vector must be rounded!!!
    /// </summary>
    /// <param name="global">must be rounded!!!</param>
    /// <param name="sun"></param>
    /// <param name="block"></param>
    /// <returns></returns>
    public virtual bool GetLight(Vector3 global, out byte sky, out byte block)
    {
        int x = (int)Math.Round(global.X);
        int y = (int)Math.Round(global.Y);
        int z = (int)Math.Floor(global.Z);
        return Chunk.TryGetFinalLight(this, x, y, z, out sky, out block);
    }
    public virtual bool GetLight(int x, int y, int z, out byte sky, out byte block)
    {
        return Chunk.TryGetFinalLight(this, x, y, z, out sky, out block);
    }
    public abstract byte GetSkyDarkness();
    public abstract byte GetSunLight(IntVec3 global);
    public abstract byte GetBlockData(IntVec3 global);
    public abstract byte SetBlockData(IntVec3 global, byte data = 0);
    public abstract void Validate();
    public virtual void Tick() { }
    public abstract SaveTag Save();
    public abstract void InvalidateCell(IntVec3 global);
    public abstract void GenerateThumbnails();
    public abstract void GenerateThumbnails(string fullpath);
    public abstract void LoadThumbnails();
    public abstract MapThumb GetThumb();
    /// <summary>
    /// TODO remove from mapbase class
    /// </summary>
    public Town Town;
    public abstract void WriteData(IDataWriter w);
    public abstract string GetFolderName();
    public abstract string GetFullPath();
    public abstract void UpdateLight(IEnumerable<IntVec3> positions);
    public abstract void DrawBlocks(MySpriteBatch sb, RenderContext ctx, EngineArgs a);
    public abstract void DrawObjects(MySpriteBatch sb, RenderContext ctx, SceneState scene);
    public abstract void DrawInterface(SpriteBatch sb, MapViewport viewport);
    public abstract void DrawWorld(MySpriteBatch sb, MapViewport viewport);
    public abstract void DrawBeforeWorld(MySpriteBatch sb, RenderContext ctx);
    public abstract void GetTooltipInfo(Control tooltip);
    internal void AddBlockEntityInternal(BlockEntity entity)
    {
        foreach(var global in entity.CellsOccupied)
        {
            var chunk = this.GetChunk(global);
            var local = global.ToLocal();
            chunk.SetBlockEntity(entity, local);
        }
        entity.OnSpawned(this);
    }
    internal void RemoveBlockEntityInternal(IntVec3 originGlobal)
    {
        var entity = this.GetBlockEntity(originGlobal);
        if (entity.OriginGlobal != originGlobal)
            throw new Exception();
        this.RemoveBlockEntityInternal(entity);
    }
    internal void RemoveBlockEntityInternal(BlockEntity entity)
    {
        foreach (var global in entity.CellsOccupied)
        {
            var chunk = this.GetChunk(global);
            var local = global.ToLocal();

            if (chunk.TryRemoveBlockEntity(local, out var found))
            {
                if (found != entity)
                    throw new Exception();
                entity.Map = null;
            }
        }
        entity.OnDespawned(this);
    }
    internal void SetBlockInternal(Dictionary<IntVec3, SetBlockArgs> changes)
    {
        HashSet<(int x, int y)> heightMapChanges = [];
        foreach(var (global, args) in changes)
        {
            if (global.Z == 0)
                throw new Exception();

            this.TryGetAll(global, out var chunk, out var cell);
            //var cell = this.GetCell(global);
            cell.Block = args.Block;
            cell.Material = args.Material;
            cell.Variation = 0;
            cell.BlockData = args.Data;
            cell.Orientation = args.Orientation;
            cell.Origin = args.Source;
            cell.Damage = 0; // reset damage when block changes?

            cell.Block.OnPlaced(new CellQuery(this, global));

            chunk.InvalidateCell(global);
            chunk.InvalidateSlice(global.Z);

            heightMapChanges.Add((global.X, global.Y));
        }

        foreach(var (x, y) in heightMapChanges)
        {
            var chunk = this.GetChunk(x, y);
            chunk.InvalidateHeightmap(x % Chunk.Size, y % Chunk.Size);
        }

    }
    public virtual PlaceBlockResult SetBlock(SetBlockArgs args)
    {
        return this.SetBlock(args.Global, args.Block, args.Material, args.Data, args.Source, orientation: args.Orientation);
    }
    public PlaceBlockResult SetBlock(IntVec3 global, BlockDef block)
    {
        return this.SetBlock(global, block.Block, block.DefaultMaterial, 0);
    }
    public virtual PlaceBlockResult SetBlock(IntVec3 global, Block block, MaterialDef material, byte data, int variation = 0, int orientation = 0, bool raiseEvent = true)
    {
        return this.SetBlock(global, block, material, data, IntVec3.Zero, variation, orientation, raiseEvent);
    }
    public virtual PlaceBlockResult SetBlock(IntVec3 global, Block block, MaterialDef material, byte data, IntVec3 source, int variation = 0, int orientation = 0, bool raiseEvent = true)
    {
        if (global.Z == 0)
            return new PlaceBlockResult(null, null, false);
        var cell = this.GetCell(global);
     
        if (cell is null)
            return new PlaceBlockResult(null, null, false);

        var chunk = this.GetChunk(global);

        if (cell.Block == BlockDefOf.Air.Block && cell.Block == block) // if the cell is already air, dont do anything, ESPECIALLY DONT call notifyblockchanged
            return new PlaceBlockResult(null, cell, false);

        cell.Block = block;
        cell.Material = material;
        cell.Variation = (byte)variation;
        cell.BlockData = data;
        cell.Orientation = orientation;
        cell.Origin = source;
        if (block.TryLinkToAdjacentBlockEntity(this, global) is not BlockEntity entity)
        {
            entity = block.BlockDef.CreateEntity(global);
            if (entity is not null)
                this.AddBlockEntity(entity);
        }
        // todo: query block for multi-cell footprint
        block.OnPlaced(this, global, material, data, variation, orientation);

        this.SetBlockLuminance(global, block.Luminance);

        var children = block.GetChildrenWithSource(global, orientation);

        if (raiseEvent)
            this.NotifyBlocksChanged(children.Select(c => c.global));

        var local = global.ToLocal();
        chunk.InvalidateHeightmap(local.X, local.Y);

        // maybe i can refresh cell edges here on the spot?
        this.InvalidateCell(global); // do i need to invalidate the cell even after invalidating the heightmap in the line above?
        var neighbors = global.GetAdjacentCubeLazy(); // changed this to only get adjacent cells to get all cells (even diagonals) around a cell, in order to let workstations update their operatingpositionunreachable property

        foreach (var n in neighbors)
        {
            var nblock = this.GetBlock(n);
            if (nblock != BlockDefOf.Air.Block)
                this.InvalidateCell(n);

            nblock?.OnNeighborChanged(this, global, n);
        }
        if (raiseEvent)
            this.NotifyBlockChanged(global);
        var setblockargs = new SetBlockArgs(this.ID, global, block, material, data, orientation, source);
        this.Events.Post(new BlockSetEvent(setblockargs));
        return new PlaceBlockResult(entity, cell, true);
    }
    public struct PlaceBlockResult(BlockEntity entity, Cell cell, bool success = true)
    {
        public BlockEntity Entity = entity;
        public Cell Cell = cell;
        public bool Success = success;
    }
    public void NotifyBlocksChanged(IEnumerable<IntVec3> positions)
    {
        this.Events.Post(new CellsInvalidatedEvent(this, positions));
        this.Town.OnBlocksChanged(positions);
    }
    public void NotifyBlockChanged(IntVec3 pos)
    {
        this.NotifyBlocksChanged(new[] { pos });
    }
    public abstract bool SetBlockLuminance(IntVec3 global, byte luminance);
    internal bool IsTraversable(Vector3 source, Vector3 target)
    {
        var globalsource = source;
        var globaltarget = target;
        if (globalsource.Z == globaltarget.Z)
            return true;
        var lower = Math.Min(globalsource.Z, globaltarget.Z) == globalsource.Z ? globalsource : globaltarget;
        var above1 = lower.Above();
        var above2 = above1.Above();
        return !this.GetBlock(above2).Solid;
    }
    
    public float GetSolidObjectHeight(Vector3 global)
    {
        var cell = this.GetCell(global);
        if (cell.Block != BlockDefOf.Air.Block)
            return cell.Block.GetHeight(cell.BlockData, global.ToBlock());

        var entities = this.GetObjects(global - new Vector3(5), global + new Vector3(5));
        foreach (var entity in entities)
        {
            if (!entity.Physics.Solid)
                continue;
            BoundingBox box = new BoundingBox(entity.Global - new Vector3(0.5f, 0.5f, 0), entity.Global + new Vector3(0.5f, 0.5f, entity.Physics.Height));
            var cont = box.Contains(global);
            if (cont == ContainmentType.Contains)
            {
                if (Vector3.Distance(global * new Vector3(1, 1, 0), entity.Global * new Vector3(1, 1, 0)) < 0.5f)
                    return entity.Physics.Height;
            }
        }
        return 0;
    }
    public void InvalidateChunks()
    {
        foreach (var chunk in this.ActiveChunks)
            chunk.Value.Invalidate();
    }

    internal void UpdateParticles()
    {
        this.ParticleManager.Update();
    }
    internal void DrawParticles(RenderContext ctx)
    {
        if (this.Net is Server)
            return;
        this.ParticleManager.Draw(ctx);
        var renderer = ctx.Renderer;
        var camera = ctx.Camera;
        foreach (var ch in this.ActiveChunks.Values)
            foreach (var (local, entity) in ch.GetBlockEntitiesByPosition())
                entity.Draw(renderer, camera, local.ToGlobal(ch));
    }
    internal IEnumerable<(string label, Type type)> GetSelectionTabs()
    {
        yield break;
    }
   
    public IEnumerable<GameObject> GetNearbyObjectsNew(Vector3 global, Func<float, bool> range, Func<GameObject, bool> filter = null, Action<GameObject> action = null)
    {
        var a = action ?? ((obj) => { });
        var f = filter ?? ((obj) => { return true; });
        Chunk chunk = this.GetChunk(global);

        foreach (Chunk ch in this.GetChunks(chunk.MapCoords))
            foreach (var obj in ch.Entities)
            {
                if (!range(Vector3.Distance(obj.Global, global)))
                    continue;
                if (!f(obj))
                    continue;
                a(obj);
                yield return obj;
            }
    }
    public bool LineOfSight(Vector3 a, Vector3 b)
    {
        var x0 = (int)a.X;
        var y0 = (int)a.Y;
        var z0 = (int)a.Z;
        var x1 = (int)b.X;
        var y1 = (int)b.Y;
        var z1 = (int)b.Z;
        var los = LineHelper.LineOfSight(x0, y0, z0, x1, y1, z1, this.IsSolid);
        return los;
    }

    internal Region GetRegionAt(Vector3 north)
        => this.Regions.GetRegionAt(north);
    
    internal RegionNode GetNodeAt(Vector3 vector3)
        => this.Regions.GetNodeAt(vector3);
    
    internal int GetRegionDistance(Vector3 source, Vector3 target, Actor actor)
        => this.Regions.GetRegionDistance(source, target, actor);

    internal bool Contains(Vector3 global)
        => this.GetChunk(global) != null;

    internal bool IsAir(Vector3 global)
        => this.GetBlock(global) == BlockDefOf.Air.Block;

    internal void RandomBlockUpdate(IntVec3 global)
    {
        var cell = this.GetCell(global);
        if (cell is not null)
            cell.Block.RandomBlockUpdate(this.Net, global, cell);
        else
            this.RandomBlockUpdateQueue.Enqueue(global);
    }
    public bool AreChunksLoaded
    {
        get
        {
            var size = this.GetSizeInChunks();
            var chunkcount = size * size;
            if (this.ActiveChunks.Count != chunkcount)
                return false;
            if (this.ActiveChunks.Values.Any(c => c == null))
                return false;
            return true;
        }
    }

    internal bool IsAboveHeightMap(IntVec3 global)
        => this.GetChunk(global).IsAboveHeightMap(global);

    internal virtual bool IsUndiscovered(Vector3 global)
        => false;

    internal virtual void AreaDiscovered(HashSet<Vector3> hashSet) { }

    //internal virtual void CameraRecenter() { }

    //public IEnumerable<Entity> Haulables => this.ActiveChunks.Values.SelectMany(c => c.Entities.Where(e => e.Def.IsHaulable)).Cast<Entity>();
    public IEnumerable<Entity> Haulables => this.Entities.Where(e => e.Def.IsHaulable);

    public IEnumerable<Entity> Entities => this.ActiveChunks.Values.SelectMany(c => c.Entities);
   
    internal bool IsVisible(IntVec3 global)
    {
        if (global.Z == MaxHeight - 1)
            return true;
        var count = VectorHelper.Adjacent.Length;
        for (int i = 0; i < count; i++)
        {
            var n = global + IntVec3.AdjacentIntVec3[i];
            var ncell = this.GetCell(n);
            //if (ncell is not null && !ncell.Opaque)
            if (ncell is not null && !ncell.Block.HidingAdjacent)
                return true;
        }
        return false;
    }
    
    internal virtual void OnHudCreated(Hud hud) { }
  
    public void Spawn(Entity entity, Vector3 position, Vector3 velocity, bool immediate = false)
    {
        if (!entity.IsRegistered)
            this.World.Register(entity, immediate);
        var entitiesAtCell = this.GetEntitiesAt(position);
        if(entitiesAtCell.FirstOrDefault(e => e.CanAbsorb(entity)) is Entity absorbingEntity)
        {
            if (entity.StackSize <= absorbingEntity.StackAvailableSpace)
            {
                absorbingEntity.Add(entity.StackSize);
                entity.Consume(entity.StackSize);
                return;
            }
        }
        entity.Container?.Remove(entity);
        if(entity.IsSpawned) entity.Map.Despawn(entity);
        if(entity is Actor actor) (this.World as StaticWorld).Space.Exit(actor);

        entity.Slot?.Assign(null);
        entity.Detach();
        entity.Net = this.Net;
        entity.Map = this;
        entity.Global = position;
        entity.Velocity = velocity;
        this.Add(entity);
        entity.OnSpawn(this);
        this.Events.Post(new EntitySpawnedEvent(this, entity, immediate));
        this.EntityTracker.OnEntitySpawned(entity);
    }
    internal void ApplyBlockDamage(IntVec3 global, int workAmount)
    {
        if (this.TryGetChunk(global, out var chunk))
            chunk.ApplyBlockWork(global, workAmount);
    }
    public IBlockHealth GetBlockHealth(IntVec3 global)
    {
        if (this.TryGetChunk(global, out var chunk))
        {
            var local = global.ToLocal();
            return chunk.GetBlockToken(local);
        }
        return null;
    }
    internal IEnumerable<IntVec3> FindNearestEmptyCellsOrCurrent(IntVec3 current, int reach)
    {
        var potential = this.Regions.GetSurroundingNodesOffset(current, reach)
            .Where(offset => this.IsCellEmpty(current.Above + offset));
        foreach (var offset in potential)
            yield return current.Above + offset;
        yield return current.Above;
    }
   
    internal List<InteractionTarget> Select(IntVec3 begin, IntVec3 end)
    {
        var cube = IntVec3Helper.GetBox(begin, end);
        List<InteractionTarget> allTargets = [];
        HashSet<IntVec3> excluded = [];
        foreach (var cell in cube)
            if (this.TryGetBlockEntity(cell, out var entity))
            {
                allTargets.Add(new InteractionTarget(entity));
                foreach (var c in entity.CellsOccupied)
                    excluded.Add(c);
            }
        var cellTargets = cube.Except(excluded).Select(c => new InteractionTarget(this, c));
        return [.. allTargets.Union(cellTargets)];
    }

    public MapQuerySnapshot Query(IntVec3 global)
    {
        var query = new MapQuery(this, global);
        return query.ToSnapshot();
    }
    
    internal void EntityChangedCell(Entity entity, IntVec3 lastCell, IntVec3 nextCell)
        => this.EntityTracker.OnEntityMoved(entity, lastCell, nextCell);

    internal byte GetSunlight(IntVec3 pos)
        => this.GetChunk(pos).GetSkylight(pos);

    internal byte GetBlockLight(IntVec3 pos)
        => this.GetChunk(pos).GetBlockLight(pos);
}

public record struct PositionQuery
{
    public Chunk Chunk;
    public Cell Cell;
    public CellId CellIndex;
    public IntVec3 Global;
    public IntVec3Local Local;
    public GlobalCellId GlobalCellId;
}
