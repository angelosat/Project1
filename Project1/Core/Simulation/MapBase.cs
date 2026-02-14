using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Blocks;
using Project1.Core.Components;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Graphics.Particles;
using Project1.Core.Helpers;
using Project1.Core.Helpers.Structs;
using Project1.Core.Map;
using Project1.Core.Materials;
using Project1.Core.Networking;
using Project1.Core.Networking.Simulation;
using Project1.Core.Rooms;
using Project1.Core.Screens;
using Project1.Core.Towns;
using Project1.Core.Towns.Stockpiles;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Core.WorldGen;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1.Core.Simulation
{
    public abstract class MapBase : Inspectable
    {
        public override string LabelReadable => this.ToString();
        public Camera Camera;
        public static float IconOffset = 0;
        public Biome Biome = new();
        protected Queue<IntVec3> RandomBlockUpdateQueue = new();
        public LightingEngine LightingEngine;
        public WorldBase World;
        public Dictionary<IntVec2, Chunk> ActiveChunks = [];
        NetEndpoint _net;
        public readonly int ID;
        public NetEndpoint Net => this._net ??= this.World.Net;
        public GameObject PlayerCharacter;
        public ParticleManager ParticleManager;
        public RegionManager Regions;
        public StockpileManager Stockpiles;
        internal EntityLifecycleManager EntityLifecycleManager;
        internal List<SimulationSystem> SimulationSystems = [];

        protected Dictionary<IntVec3, BlockEntity> CachedBlockEntities = new();
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
        public float Gravity => this.World.Gravity;
        public readonly float PlantDensityTarget = .1f;
        public int ChunkVolume => Chunk.Size * Chunk.Size * this.GetMaxHeight();

        public int Area => this.ActiveChunks.Count * Chunk.Size * Chunk.Size;

        internal Room GetRoomAt(IntVec3 global)
        {
            return this.Town.RoomManager.GetRoomAt(global);
        }

        public int Volume => this.ActiveChunks.Count * this.ChunkVolume;

        public Random Random => this.World.Random;
        public abstract Dictionary<IntVec2, Chunk> GetActiveChunks();
        public abstract bool AddChunk(Chunk chunk);
        //public abstract IEnumerable<GameObject> GetObjects();
        public abstract IEnumerable<GameObject> GetObjects(Vector3 min, Vector3 max);
        public abstract IEnumerable<GameObject> GetObjects(BoundingBox box);
        public IEnumerable<BlockEntity> BlockEntities => this.ActiveChunks.Values.SelectMany(ch => ch.BlockEntities).Distinct();

        public IEnumerable<T> GetBlockEntities<T>() where T : BlockEntity
        {
            return this.BlockEntities.OfType<T>();
        }
        
        public static int MaxHeight = 128;

        internal bool IsDesignation(IntVec3 global)
        {
            return this.Town.DesignationManager.IsDesignation(new TargetArgs(this, global));
        }

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

        internal void OnCameraRotated(Camera camera)
        {
            foreach (var chunk in this.GetActiveChunks())
            {
                chunk.Value.OnCameraRotated(camera);
                chunk.Value.Invalidate();
            }
            this.Town.OnCameraRotated(camera);
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
            //return !this.BlockEntities.Any(e => e.ReservedInteractionCells.Contains(pos));
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

        internal void ResolveReferences()
        {
            this.World.ResolveReferences();
            this.Town.ResolveReferences();
            this.Stockpiles.ResolveReferences();
            foreach (var chunk in this.ActiveChunks.Values)
                chunk.ResolveReferences();
        }

        internal void ReplaceBlock(Vector3 global, Block block, MaterialDef material, byte data, int variation, int orientation, bool raiseEvent = true)
        {
            this.RemoveBlock(global);

            var blockentity = this.RemoveBlockEntity(global);
            if (blockentity != null)
            {
                blockentity.OnRemoved(this, global);
                blockentity.Dispose();
                this.Net.EventOccured((int)Message.Types.BlockEntityRemoved, blockentity, global);
            }

            // reenable physics of entities resting on block
            foreach (var entity in this.GetObjects(global.Above()))
                entity.Physics.Enable();

            this.SetBlock(global, block, material, data, variation, orientation, raiseEvent);
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
        private void DestroyBlock(IntVec3 global)
        {
            var block = this.GetBlock(global);
            this.RemoveBlock(global);
            //this.Events.Post(new BlockDestroyedEvent(block, this, global));
        }
        public void RemoveBlock(IntVec3 vec, bool notify = true)
        {
            var global = Cell.GetOrigin(this, vec);
            var cell = this.GetCell(global);
            var block = cell.Block;
            foreach (var u in block.UtilitiesProvided)
                this.Town.RemoveUtility(u, global);
            var blockentity = this.GetBlockEntity(global);

            var parts = cell.GetParts(global);

            this.GetBlock(global).PreRemove(this, global); // preremove only center part or all parts?
            if (blockentity != null)
            {
                this.RemoveBlockEntity(global);
                blockentity.OnRemoved(this, global);
                blockentity.Dispose();
                if (notify)
                    this.Net.EventOccured((int)Message.Types.BlockEntityRemoved, blockentity, global);
            }
            else
                foreach (var p in parts)
                {
                    this.SetBlock(p, BlockDefOf.Air.Worker, MaterialDefOf.Air, 0, 0, 0, notify);
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
            var nonAirPositions = positions.Where(vec => this.GetBlock(vec) != BlockDefOf.Air.Worker).ToList();
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

        public void RemoveBlockEntity(BlockEntity entity)
        {
            //foreach (var cell in entity.CellsOccupied)
            //    this.RemoveBlockEntity(cell);
            this.RemoveBlockEntity(entity.OriginGlobal);
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
            //entity.OnSpawned(this, global);
            //this.Events.Post(new BlockEntityAddedEvent(entity));

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

        public Dictionary<IntVec3, BlockEntity> GetBlockEntitiesCache()
        {
            return this.CachedBlockEntities;
        }
        public bool TryGetBlockEntity(IntVec3 global, out BlockEntity entity)
        {
            entity = null;
            if (this.GetChunk(global) is not Chunk chunk)
                return false;
            return chunk.TryGetBlockEntity(global.ToLocal(), out entity);
        }
        public bool TryGetBlockEntity<T>(IntVec3 global, out T entity) where T : BlockEntity
        {
            BlockEntity e = null;
            entity = null;
            if (this.GetChunk(global) is not Chunk chunk)
                return false;
            chunk.TryGetBlockEntity(global.ToLocal(), out e);
            entity = e as T;
            return entity is not null;
        }
        public BlockEntity GetBlockEntity(IntVec3 global)
        {
            Chunk chunk = this.GetChunk(global);
            chunk.TryGetBlockEntity(global.ToLocal(), out var entity);
            return entity;
        }
        public T GetBlockEntityComp<T>(IntVec3 global) where T: BlockComp
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
            Chunk chunk = this.GetChunk(global);
            chunk.TryGetBlockEntity(global.ToLocal(), out var entity);

            return entity as T;
        }

        public virtual int GetHeightmapValue(int x, int y)
        {
            var global = new Vector3(x, y, 0);
            var ch = this.GetChunk(global);
            if (ch == null)
                return int.MinValue;
            return ch.GetHeightMapValue(global.ToLocal());
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
            //throw new Exception();
            var globalRound = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), (int)Math.Floor(global.Z));
            if (this.TryGetChunk(globalRound, out var chunk))
                return chunk[globalRound.X - chunk.Start.X, globalRound.Y - chunk.Start.Y, globalRound.Z];
            return null;
        }

        public Chunk GetChunk(Vector3 global)
        {
            if (this.TryGetChunk(global, out var chunk))
                return chunk;
            return null;
        }
        public Chunk GetChunk(int x, int y)
        {
            int chunkX = x / Chunk.Size;
            int chunkY = y / Chunk.Size;
            return this.ActiveChunks[new Vector2(chunkX, chunkY)];
        }

        public List<Chunk> GetChunks(Vector2 pos, int radius = 1)
        {
            List<Chunk> list = new List<Chunk>();
            int x = (int)pos.X, y = (int)pos.Y;
            for (int i = x - radius; i <= x + radius; i++)
                for (int j = y - radius; j <= y + radius; j++)
                    if (this.ActiveChunks.TryGetValue(new Vector2(i, j), out Chunk ch))
                        list.Add(ch);
            return list;
        }
        public bool TryGetCell(Vector3 global, out Cell cell)
        {
            return this.TryGetAll(global, out Chunk chunk, out cell);
        }
        public bool TryGetChunk(Vector3 global, out Chunk chunk)
        {
            if (global.Z < 0 || global.Z >= MaxHeight)
            {
                chunk = null;
                return false;
            }
            var x = Math.Round(global.X);
            var y = Math.Round(global.Y);
            int chunkX = (int)Math.Floor(x / Chunk.Size);
            int chunkY = (int)Math.Floor(y / Chunk.Size);
            return this.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk);
        }
        public bool TryGetChunk(int globalx, int globaly, out Chunk chunk)
        {
            float chunkX = (float)Math.Floor((float)globalx / Chunk.Size);
            float chunkY = (float)Math.Floor((float)globaly / Chunk.Size);

            return this.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk);
        }
        public bool TryGetAll(Vector3 global, out Chunk chunk, out Cell cell)
        {
            cell = null;
            chunk = null;
            Vector3 rounded = global.RoundXY();
            if (rounded.Z < 0 || rounded.Z > this.World.MaxHeight - 1)
                return false;
            int chunkX = (int)Math.Floor(rounded.X / Chunk.Size);
            int chunkY = (int)Math.Floor(rounded.Y / Chunk.Size);
            if (this.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk))
            {
                cell = chunk[(int)(rounded.X - chunk.Start.X), (int)(rounded.Y - chunk.Start.Y), (int)rounded.Z];
                return true;
            }
            return false;
        }

        internal bool IsStandableIn(Vector3 global)
        {
            //if (!this.Contains(global))
            //    return false;
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

        public abstract bool TryGetAll(int gx, int gy, int gz, out Chunk chunk, out Cell cell, out int lx, out int ly);

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

        public virtual bool IsEmpty(Vector3 global)
        {
            global = global.ToRounded();
            if (this.GetBlock(global) != BlockDefOf.Air.Worker)
                return false;
            var blockbox = new BoundingBox(global - (Vector3.UnitX + Vector3.UnitY) * .5f, global + Vector3.UnitZ + (Vector3.UnitX + Vector3.UnitY) * .5f);
            var entities = this.GetObjectsAtChunk(global);
            foreach (var entity in entities)
            {
                var entitybox = new BoundingBox(entity.Transform.Global - (Vector3.UnitX + Vector3.UnitY) * .2f, entity.Transform.Global + Vector3.UnitZ * entity.Physics.Height + (Vector3.UnitX + Vector3.UnitY) * .2f);
                if (blockbox.Intersects(entitybox))
                    return false;
            }
            return true;
        }

        public abstract List<GameObject> GetObjectsAtChunk(Vector3 global);
        public List<GameObject> GetObjectsIntersectingBlock(Vector3 global)
        {
            var entities = this.GetObjectsAtChunk(global);
            var list = new List<GameObject>();
            var blockbox = new BoundingBox(global - new Vector3(.5f, .5f, 0), global + new Vector3(.5f, .5f, 1));
            foreach (var entity in entities)
            {
                var size = .5f;// .2f;
                var entitybox = new BoundingBox(entity.Global - new Vector3(size, size, 0), entity.Global + new Vector3(size, size, entity.Physics.Height));
                if (blockbox.Intersects(entitybox))
                    list.Add(entity);
            }
            return list;
        }
        public bool Despawn(EntityRefId entityRefId)
        {
            return this.Despawn(this.World.GetEntity(entityRefId));
        }
        public bool Despawn(GameObject obj)
        {
            if (obj.Map != this)
                return false;
            obj.OnDespawn(this);
            if (!this.Remove(obj)) // TODO: move this to map.despawn
                throw new Exception();
            obj.Map = null;
            this.Events.Post(new EntityDespawnedEvent(obj as Entity));
            return true;
        }
        public void DespawnAndSync(Entity entity)
        {
            this.Despawn(entity);
        }
        internal bool Remove(GameObject obj)
        {
            return this.GetChunk(obj.Global).Remove(obj);
        }
        internal void Add(GameObject obj)
        {
            this.GetChunk(obj.Global).Add(obj);
        }
        public IEnumerable<GameObject> GetObjects(Vector3 global)
        {
            var ch = this.GetChunk(global);
            var objects = ch.Objects;
            var count = objects.Count;
            var globalIntVec3 = global.ToCell();
            for (int i = 0; i < count; i++)
            {
                var e = objects[i];
                if (e.Global.ToCell() == globalIntVec3)
                    yield return e;
            }
        }
        public IEnumerable<GameObject> GetObjectsOccupyingCell(IntVec3 global)
        {
            var ch = this.GetChunk(global);
            var objects = ch.Objects;
            var count = objects.Count;
            for (int i = 0; i < count; i++)
            {
                var e = objects[i];
                if (e.GetOccupyingCells().Contains(global))
                    yield return e;
            }
        }
        public bool IsCellEmptyNew(IntVec3 global)
        {
            return !this.GetObjects(global).Any();
        }

        [Obsolete($"use {nameof(GetEntitiesAt)} instead")]
        internal virtual IEnumerable<GameObject> GetObjects(IEnumerable<Vector3> positions)
        {
            var chunks = new HashSet<Chunk>();
            foreach (var pos in positions)
                chunks.Add(this.GetChunk(pos));
            IEnumerable<GameObject> objects = chunks.SelectMany(ch => ch.GetObjects());
            return objects.Where(obj => positions.Contains(obj.Global.ToCell()));
        }

        internal virtual IEnumerable<Entity> GetEntitiesAt(IntVec3 pos)
        {
            foreach (var entity in this.GetObjectsAtChunk(pos).Where(e => (IntVec3)e.Global == pos))
                yield return (Entity)entity;
        }
        public abstract bool IsInBounds(Vector3 global);

        public abstract void SetSkyLight(IntVec3 global, byte value);
        public abstract void SetBlockLight(IntVec3 global, byte value);

        public abstract void AddSkyLightChanges(Dictionary<IntVec3, byte> List);
        public abstract void AddBlockLightChanges(Dictionary<IntVec3, byte> List);
        public abstract void ApplyLightChanges();

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

        public abstract bool InvalidateCell(IntVec3 global);
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

        public abstract void DrawBlocks(MySpriteBatch sb, Camera cam, EngineArgs a);
        public abstract void DrawObjects(MySpriteBatch sb, Camera cam, SceneState scene);
        public abstract void DrawInterface(SpriteBatch sb, Camera cam);
        public abstract void DrawWorld(MySpriteBatch sb, Camera cam);
        public abstract void DrawBeforeWorld(MySpriteBatch sb, Camera cam);

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

                chunk.InvalidateCell(cell);

                heightMapChanges.Add((global.X, global.Y));
            }

            foreach(var (x, y) in heightMapChanges)
            {
                var chunk = this.GetChunk(x, y);
                chunk.InvalidateHeightmap(x % Chunk.Size, y % Chunk.Size);
            }

        }
        public void SetBlockInternal(IntVec3 global, Block block, MaterialDef material, byte data, IntVec3 source, int variation = 0, int orientation = 0)
        {
            var cell = this.GetCell(global);
            cell.Block = block;
            cell.Material = material;
            cell.Variation = (byte)variation;
            cell.BlockData = data;
            cell.Orientation = orientation;
            cell.Origin = source;
        }
        public virtual PlaceBlockResult SetBlock(SetBlockArgs args)
        {
            return this.SetBlock(args.Global, args.Block, args.Material, args.Data, args.Source, orientation: args.Orientation);
        }
        public PlaceBlockResult SetBlock(IntVec3 global, BlockDef block)
        {
            return this.SetBlock(global, block.Worker, block.DefaultMaterial, 0);
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

            //if (cell.Block == BlockDefOf.Air && cell.Block == block) // if the cell is already air, dont do anything, ESPECIALLY DONT call notifyblockchanged
            if (cell.Block == BlockDefOf.Air.Worker && cell.Block == block) // if the cell is already air, dont do anything, ESPECIALLY DONT call notifyblockchanged
                return new PlaceBlockResult(null, cell, false);

            cell.Block = block;
            cell.Material = material;
            cell.Variation = (byte)variation;
            cell.BlockData = data;
            cell.Orientation = orientation;
            cell.Origin = source;
            //var entity = block.CreateEntity(global);
            //var entity = block.BlockDef.CreateEntity(global);
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
            //foreach (var (child, parent) in children)
            //    this.GetCell(child).Origin = parent;

            if (raiseEvent)
                this.NotifyBlocksChanged(children.Select(c => c.global));

            chunk.InvalidateHeightmap(cell.X, cell.Y);

            // maybe i can refresh cell edges here on the spot?
            this.InvalidateCell(global); // do i need to invalidate the cell even after invalidating the heightmap in the line above?
            var neighbors = global.GetAdjacentCubeLazy(); // changed this to only get adjacent cells to get all cells (even diagonals) around a cell, in order to let workstations update their operatingpositionunreachable property

            foreach (var n in neighbors)
            {
                var nblock = this.GetBlock(n);
                if (nblock != BlockDefOf.Air.Worker)
                    this.InvalidateCell(n);

                nblock?.OnNeighborChanged(this, global, n);
            }
            if (raiseEvent)
                this.NotifyBlockChanged(global);
            var setblockargs = new SetBlockArgs(global, block, material, data, orientation, source);
            this.Events.Post(new BlockSetEvent(setblockargs));
            return new PlaceBlockResult(entity, cell, true);
        }
        public struct PlaceBlockResult(BlockEntity entity, Cell cell, bool success = true)
        {
            public BlockEntity Entity = entity;
            public Cell Cell = cell;
            public bool Success = success;
        }
        void SetBlockInternal(IntVec3 global, Block block, MaterialDef material, byte data, int variation = 0, int orientation = 0)
        {
            this.TryGetAll(global, out var chunk, out var cell);
            cell.Block = block;
            cell.Material = material;
            cell.BlockData = data;
            cell.Variation = variation;
            cell.Orientation = orientation;
            chunk.InvalidateHeightmap(cell.X, cell.Y);
            chunk.InvalidateCell(cell); // do i need to invalidate the cell even after invalidating the heightmap in the line above?
        }
        public void NotifyBlocksChanged(IEnumerable<IntVec3> positions)
        {
            //this.Net.EventOccured((int)Components.Message.Types.BlocksChanged, this, positions);
            this.Events.Post(new CellsInvalidatedEvent(this, positions));
            this.Town.OnBlocksChanged(positions);
        }
        public void NotifyBlockChanged(IntVec3 pos)
        {
            this.NotifyBlocksChanged(new[] { pos });
        }

        public abstract bool SetBlockLuminance(IntVec3 global, byte luminance);
        [Obsolete]
        internal bool IsTraversable2Height(Vector3 source, Vector3 target)
        {
            var globalsource = source;
            var globaltarget = target;
            if (globalsource.Z == globaltarget.Z)
                return true;
            var lower = Math.Min(globalsource.Z, globaltarget.Z) == globalsource.Z ? globalsource : globaltarget;
            var above1 = lower.Above();
            var above2 = above1.Above();
            var above3 = above2.Above();
            var above3block = this.GetBlock(above3);
            if (above3block.Solid) // no need to check for doors because they are defined as non-solid
            {
                return false;
            }
            return true;
        }
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
        public void EventOccured(Message.Types type, params object[] p)
        {
            this.Net?.EventOccured((int)type, p);
        }

        public virtual void OnGameEvent(GameEvent e)
        {
            this.ParticleManager.OnGameEvent(e);
            foreach (var obj in this.GetEntities())
                obj.OnGameEvent(e);
        }
        public float GetSolidObjectHeight(Vector3 global)
        {
            var cell = this.GetCell(global);
            if (cell.Block != BlockDefOf.Air.Worker)
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
        internal void DrawParticles(Camera camera)
        {
            if (this.Net is Server)
                return;
            this.ParticleManager.Draw(camera);
            foreach (var ch in this.ActiveChunks.Values)
                foreach (var (local, entity) in ch.GetBlockEntitiesByPosition())
                    entity.Draw(camera, this, local.ToGlobal(ch));
        }
        internal IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            yield break;
        }
        internal void OnTargetSelected(IUISelection info, ISelectable selected)
        {
            this.World.OnTargetSelected(info, selected);
            this.Town.OnTargetSelected(info, selected);
        }
        internal void OnTargetSelected(SelectionManager info, ISelectable selected)
        {
            this.World.OnTargetSelected(info, selected);
            this.Town.OnTargetSelected(info, selected);
        }
        public IEnumerable<GameObject> GetNearbyObjectsNew(Vector3 global, Func<float, bool> range, Func<GameObject, bool> filter = null, Action<GameObject> action = null)
        {
            var a = action ?? ((obj) => { });
            var f = filter ?? ((obj) => { return true; });
            Chunk chunk = this.GetChunk(global);

            foreach (Chunk ch in this.GetChunks(chunk.MapCoords))
                foreach (GameObject obj in ch.GetObjects())
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

        internal MaterialDef GetMaterial(IntVec3 global)
        {
            return this.GetCell(global).Material;
            //return Block.GetBlockMaterial(this, global);
        }

        internal Region GetRegionAt(Vector3 north)
        {
            return this.Regions.GetRegionAt(north);
        }
        internal RegionNode GetNodeAt(Vector3 vector3)
        {
            return this.Regions.GetNodeAt(vector3);
        }

        internal bool CanReach(GameObject actor, Vector3 global)
        {
            return this.Regions.CanReach(actor, global);
        }

        internal int GetRegionDistance(Vector3 source, Vector3 target, Actor actor)
        {
            return this.Regions.GetRegionDistance(source, target, actor);
        }
        internal bool Contains(Vector3 global)
        {
            return this.GetChunk(global) != null;
        }

        internal bool IsAir(Vector3 global)
        {
            return this.GetBlock(global) == BlockDefOf.Air.Worker;
        }

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

        public bool IsActive => Ingame.CurrentMap == this;
        public bool IsAboveHeightMap(IntVec3 global)
        {
            return this.IsAboveHeightMap((Vector3)global);
        }
        internal bool IsAboveHeightMap(Vector3 global)
        {
            var chunk = this.GetChunk(global);
            return chunk.IsAboveHeightMap(global.ToLocal());
        }

        internal virtual bool IsUndiscovered(Vector3 global)
        {
            return false;
        }

        internal virtual void AreaDiscovered(HashSet<Vector3> hashSet)
        {
        }

        //internal void Draw(ToolManager toolManager, UIManager windowManager, SceneState scene)
        //{
        //    this.Camera.DrawMap(this, toolManager, windowManager, scene);
        //}
        internal virtual void CameraRecenter()
        {

        }
        public IEnumerable<Entity> Haulables => this.ActiveChunks.Values.SelectMany(c => c.Objects.Where(e => e.Def.IsHaulable)).Cast<Entity>();

        public IEnumerable<Entity> Entities => this.ActiveChunks.Values.SelectMany(c => c.Objects).Cast<Entity>();
        public IEnumerable<GameObject> GetEntities()
        {
            var chunks = this.ActiveChunks.Values;
            foreach (var chunk in chunks)
            {
                var entities = chunk.Objects;
                foreach (var e in entities)
                    if (e.Exists)
                        yield return e;
            }
        }
        public IEnumerable<T> GetEntities<T>() where T : Entity
        {
            var chunks = this.ActiveChunks.Values;
            foreach (var chunk in chunks)
            {
                var entities = chunk.Objects;
                foreach (var e in entities.OfType<T>())
                    //if (e.Exists) ///why wouldn't it exist if it's in the map/chunk???
                        yield return e;
            }
        }
        //public IEnumerable<GameObject> GetObjectsLazy()
        //{
        //    var count = this.CachedObjects.Count;
        //    for (int i = 0; i < count; i++)
        //    {
        //        var obj = this.CachedObjects[i];
        //        if (obj.Exists)
        //            yield return obj;
        //    }
        //}
        public IEnumerable<Entity> Find(Func<Entity, bool> filter)
        {
            foreach (Entity o in this.GetEntities())
                if (filter(o))
                    yield return o;
        }
        public IEnumerable<T> Find<T>(Func<T, bool> filter) where T : Entity
        {
            foreach (T o in this.GetEntities<T>())
                if (filter(o))
                    yield return o;
        }
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

        internal IEnumerable<KeyValuePair<IntVec3, BlockEntity>> GetBlockEntitiesWithComp<T>() where T : BlockComp
        {
            var entities = this.GetBlockEntitiesCache();
            var count = entities.Count;
            for (int i = 0; i < count; i++)
            {
                var kv = entities.ElementAt(i);
                if (kv.Value.HasComp<T>())
                    yield return kv;
            }
        }
        [Obsolete("use map.spawnandsync()")]
        internal void SyncSpawn(GameObject obj, Vector3 global, Vector3 velocity)
        {
            obj.Global = global;
            obj.Velocity = velocity;
            this.SyncSpawn(obj);
        }
        internal void SyncSpawn(GameObject obj)
        {
            //obj.Spawn(this);
            this.Spawn(obj as Entity);
            PacketsMap.SendSpawnEntity(this.Net, obj, this, obj.Global, obj.Velocity);
        }
        
        internal virtual void OnHudCreated(Hud hud)
        {
        }
        [Obsolete("use spawn(entity, position, velocity) instead")]
        internal void Spawn(Entity entity)
        {
            throw new Exception();
            this.Spawn(entity, entity.Global, entity.Velocity);
        }
        public void Spawn(EntityRefId entityRefId, Vector3 position, Vector3 velocity)
        {
            this.Spawn(this.World.GetEntity(entityRefId), position, velocity);
        }

        public void Spawn(Entity entity, Vector3 position, Vector3 velocity, bool immediate = false)
        {
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

            //entity.Slot?.Object = null;
            entity.Slot?.Assign(null);
            entity.Net = this.Net;
            entity.Map = this;
            //entity.SetGlobal(position);
            entity.Global = position;
            entity.Velocity = velocity;
            this.Add(entity);
            entity.OnSpawn(this);
            this.Events.Post(new EntitySpawnedEvent(entity, immediate));
        }
       

        internal void ApplyBlockWork(IntVec3 global, int workAmount)
        {
            if (this.TryGetChunk(global, out var chunk))
            {
                var block = this.GetBlock(global);
                if (block.BlockDef == BlockDefOf.Air)
                    throw new Exception();
                var local = global.ToLocal();
                chunk.ApplyBlockWork(local, workAmount);
                this.Events.Post(new BlockHitEvent(block, this, global, workAmount));
                if (this.GetCell(global).HitPoints == 0)
                    this.RemoveBlock(global);
            }
        }

        public IBlockToken GetBlockToken(IntVec3 global)
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
                .Where(offset => this.IsCellEmptyNew(current.Above + offset));
            foreach (var offset in potential)
                yield return current.Above + offset;
            yield return current.Above;
        }

        internal void GetQuickButtons(Action<string, Type> register, IntVec3 global)
        {
            this.Town.GetQuickButtons(register, global);
        }

        static MapBase()
        {

        }

        public readonly EventBus Events = new();
    }
}
