using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Base;
using Project1.Core.Input;
using Project1.Core.Net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Project1.Core.Rendering;
using Project1.Core.Materials;
using Project1.Core.Helpers;
using Project1.Core.Graphics;
using Project1.Core.Simulation.Lighting;
using Project1.Core.Entities;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Framework.Graphics;
using Project1.Framework;
using Project1.Core.WorldGen;

namespace Project1.Core.Simulation
{
    [Flags]
    public enum Edges { None = 0x0, West = 0x1, North = 0x2, East = 0x4, South = 0x8, All = 0xF }
    public class Chunk : Inspectable
    {
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
            //return this.GradientCache[GetCellIndex(localx, localy, localz)];
        }
        //double[] GradientCache;
        public Dictionary<IntVec3, double> InitCells2(List<Terraformer> mutators)
        {
            //this.GradientCache = new double[this.Cells.Length];
            var gradientCache = new Dictionary<IntVec3, double>();
            int n = 0; ;
            var grad = new GradientLowRes(this.World, this);
            var maxh = MapBase.MaxHeight;
            for (int z = 0; z < maxh; z++)
                for (int j = 0; j < Size; j++)
                    for (int i = 0; i < Size; i++)
                    {
                        Cell cell = new(i, j, z);
                        double gradient = grad.GetGradient(i, j, z);
                        //this.GradientCache[n] = gradient;
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
                    {
                        Cell cell = new(i, j, z);
                        this.Cells[n++] = cell;
                    }
            return this;
        }
        #endregion

        public override string ToString()
        {
            string text =
                "Local: " + this.MapCoords.ToString() +
                  "\nGlobal: " + this.Start.ToString() +
                   "\nObjects: " + this.Objects.Count +
                "\nCells to validate: " + this.CellsToValidate.Count;

            text += "Objects: " + this.Objects.Count.ToString() + "\n";
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

        [InspectorHidden]
        public Cell[] Cells;


        public List<GameObject> Objects;
        readonly Dictionary<IntVec3, BlockEntity> BlockEntitiesByPosition = new();
        public IEnumerable<BlockEntity> BlockEntities => this.BlockEntitiesByPosition.Values.Distinct();

        public bool IsQueuedForLight;
        public const int Size = 16;
        public IntVec2 Start;
        public Vector2 bottomRight;
        public void Invalidate()
        {
            foreach (var slice in this.Slices)
                if (slice != null)
                    slice.Valid = false;
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
        readonly Queue<Cell> CellsToValidate = new Queue<Cell>();

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
            get => new IntVec2(this.X, this.Y);
            set
            {
                this.X = value.X;
                this.Y = value.Y;
                this.Start = this.MapCoords * Size;
            }
        }
        internal void ResolveReferences()
        {
            
            //foreach (var obj in this.Objects)
            //    obj.Resolve();
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
            this.Objects = new List<GameObject>();
            this.HeightMap = new int[Size][];
            for (int i = 0; i < Size; i++)
                this.HeightMap[i] = new int[Size];
            this.ResetCellLight();
            for (int i = 0; i < MapBase.MaxHeight; i++)
                this.Slices[i] = new Slice();
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

        public void Add(GameObject obj)
        {
            obj.Map = this.Map;
            if (this.Objects.Contains(obj))
                throw new Exception();
            this.Objects.Add(obj);
        }
        public bool Remove(GameObject obj)
        {
            if (!this.Objects.Remove(obj))
                throw new Exception();
            return true;
        }

        #region Dunno
        public Cell GetLocalCell(int x, int y, int z)
        {
            return this.Cells[GetCellIndex(x, y, z)];
        }
        public Cell GetLocalCell(IntVec3 local)
        {
            return this.Cells[GetCellIndex(local)];
        }
        public static int GetCellIndex(int x, int y, int z) => (z * Size + y) * Size + x;
        public static int GetCellIndex(float x, float y, float z)
        {
            return GetCellIndex((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(z));
        }
        public static int GetCellIndex(IntVec3 local)
        {
            return GetCellIndex(local.X, local.Y, local.Z);
        }
        public static int Volume = Size * Size * MapBase.MaxHeight;
        public byte[] BlockLight = new byte[Volume];
        public byte[] Sunlight = new byte[Volume];

        void ResetCellLight()
        {
            this.BlockLight = new byte[Volume];
            for (int i = 0; i < Volume; i++)
                this.Sunlight[i] = 15;
        }

        public int[][] HeightMap;

        public int GetHeightMapValue(Vector3 local)
        {
            return this.GetHeightMapValue((int)local.X, (int)local.Y);
        }
        public int GetHeightMapValue(int localx, int localy)
        {
            return this.HeightMap[localx][localy];
        }
        public bool IsAboveHeightMap(Vector3 local)
        {
            return local.Z > this.HeightMap[(int)local.X][(int)local.Y];
        }
        public bool IsAboveHeightMap(int localx, int localy, int localz)
        {
            return localz > this.HeightMap[localx][localy];
        }

        Queue<IntVec3> LightChanges = new Queue<IntVec3>();

        /// <summary>
        /// Recalculates the skylight of a chunk and returns a list of cells whose skylight that changed.
        /// </summary>
        /// <returns>A list of cells whose skylight has changed</returns>
        public Queue<IntVec3> ResetHeightMap()
        {
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    foreach (var pos in this.ResetHeightMapColumn(i, j))
                        this.LightChanges.Enqueue(pos);
            var toReturn = new Queue<IntVec3>(this.LightChanges);
            this.LightChanges = new Queue<IntVec3>();
            return toReturn;
        }

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
            this.UpdateHeightMapColumnWithLightSmart(localx, localy);
        }
        HashSet<IntVec2> HeightMapUpdates = new();

        /// <summary>
        /// the current ont
        /// </summary>
        /// <param name="localx"></param>
        /// <param name="localy"></param>
        public void UpdateHeightMapColumnWithLightSmart(int localx, int localy)
        {
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
                    if (cell.Block != BlockDefOf.Air.Worker)
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
                    this.InvalidateCell(cell); // why did i have this commented out? it caused slice meshes not getting updated light

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
                    if (cell.Block != BlockDefOf.Air.Worker)
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
                this.SetSunlight(localx, localy, z, light);
                if (invalidate)
                    this.InvalidateCell(cell);
                z--;
            }

            if (light > 0)
                this.HeightMap[localx][localy] = z;
        }

        public Queue<Vector3> ResetHeightMapColumn(int localx, int localy)
        {
            Queue<Vector3> lightsourcesToHandle = new Queue<Vector3>();
            int z;
            byte light;
            Cell cell;
            light = 15;
            z = this.Map.GetMaxHeight() - 1;
            int firstContact = z;
            bool hit = false;
            while (z >= 0)
            {
                cell = this.GetLocalCell(localx, localy, z);
                if (!hit)
                    if (cell.Block != BlockDefOf.Air.Worker)
                    {
                        hit = true;
                        firstContact = z;
                    }
                if (cell.Opaque)
                {
                    if (light > 0)
                    {
                        this.HeightMap[localx][localy] = z;
                        light = 0;
                    }
                }

                this.SetSunlight(localx, localy, z, light);
                if (z <= firstContact)
                    //lightsourcesToHandle.Enqueue(cell.Local.ToGlobal(this));
                    //lightsourcesToHandle.Enqueue(cell.Local);
                    lightsourcesToHandle.Enqueue(cell.GetGlobalCoords(this));

                    z--;
            }

            if (light > 0)
                this.HeightMap[localx][localy] = z;
            return lightsourcesToHandle;
        }
        #endregion

        public void ValidateCells()
        {
            if (this.CellsToValidate.Any())
            {
                while (this.CellsToValidate.Count > 0)
                {
                    Cell cell = this.CellsToValidate.Dequeue();
                    this.Map.LightingEngine.HandleImmediate(new IntVec3[] { cell.GetGlobalCoords(this) });
                    cell.Valid = true;
                    this.InvalidateSlice(cell.Z);
                    this.InvalidateMesh();
                }
            }
        }

        public void InvalidateSlice(byte z)
        {
            this.Slices[z].Valid = false;
            this.InvalidateMesh();
        }
        public void InvalidateSlice(int z)
        {
            this.InvalidateSlice((byte)z);
        }

        public bool InvalidateCell(Cell cell)
        {
            this.BlockTokens.Remove(cell.LocalCoords);
            if (cell is null)
                throw new Exception();
            this.InvalidateLight(cell);

            if (!cell.Valid)
                return false;

            this.CellsToValidate.Enqueue(cell);
            cell.Valid = false;
            return true;
        }

        public byte GetBlockLight(IntVec3 local)
        {
            return this.GetBlockLight(local.X, local.Y, local.Z);
        }
        public byte GetBlockLight(int x, int y, int z)
        {
            return this.BlockLight[GetCellIndex(x, y, z)];
        }

        public byte GetSunlight(IntVec3 local)
        {
            return this.GetSunlight(local.X, local.Y, local.Z);
        }
        public byte GetSunlight(int x, int y, int z)
        {
            if (z >= this.Map.GetMaxHeight())
                return 15;
            return this.Sunlight[GetCellIndex(x, y, z)];
        }

        public void SetSunlight(IntVec3 local, byte value)
        {
            this.SetSunlight(local.X, local.Y, local.Z, value);
        }
        public void SetSunlight(int x, int y, int z, byte value)
        {
            this.Sunlight[GetCellIndex(x, y, z)] = value;
            var global = new IntVec3(this.Start.X + x, this.Start.Y + y, z);
            this.InvalidateLight(global);
        }

        public void SetBlockLight(IntVec3 local, byte value)
        {
            this.BlockLight[GetCellIndex(local)] = value;
            var global = local + new IntVec3(this.Start.X, this.Start.Y, 0);
            this.InvalidateLight(global);
        }

        /// <summary>
        /// TODO: optimize: convert to dictionary for speed
        /// </summary>
        public Dictionary<IntVec3, LightToken> LightCache = [];
        internal Dictionary<IntVec3, BlockHealthToken> BlockTokens = [];
        public static bool InvalidateLight(MapBase map, IntVec3 global)
        {
            if (map.TryGetAll(global.X, global.Y, global.Z, out Chunk chunk, out Cell cell, out int lx, out int ly))
            {
                return chunk.LightCache.Remove(global);
            }
            return false;
        }
        public bool InvalidateLight(Cell cell)
        {
            return this.InvalidateLight(cell.GetGlobalCoords(this));
        }
        public bool InvalidateLight(IntVec3 global)
        {
            this.LightCache.Clear();
            if (this.Slices.Any())
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
            byte finalsun = (byte)Math.Max(0, chunk.GetSunlight(lx, ly, globalZ) - map.GetSkyDarkness());
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
            sunlight = chunk.GetSunlight(x, y, global.Z);
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
            foreach (var o in this.Objects)
                o.HitTest(camera);
        }

        private void ValidateHeightmap()
        {
            if (this.HeightMapUpdates.Any())
            {
                foreach (var pos in this.HeightMapUpdates)
                    this.UpdateHeightMapColumnWithLightSmart(pos.X, pos.Y);
                this.HeightMapUpdates = new();
            }
        }
        public void Tick()
        {
            this.TickEntities();
            this.TickBlockEntities();
            this.TickBlockTokens();
        }
        void TickBlockTokens()
        {
            var keysToRemove = new List<IntVec3>(this.BlockTokens.Count);
            foreach (var (pos, token) in this.BlockTokens)
            {
                token.Tick();
                if (token.HasExpired)
                    keysToRemove.Add(pos);
            }
            foreach (var k in keysToRemove)
                this.BlockTokens.Remove(k);
        }
        private void TickBlockEntities()
        {
            foreach (var blockentity in this.BlockEntitiesByPosition.ToList())
                blockentity.Value.Tick(this.Map, blockentity.Key.ToGlobal(this));
        }
        private void TickEntities()
        {
            var objectList = this.Objects.ToArray();
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
            foreach (GameObject obj in this.Objects) //make a copy of the list first because currently the player character might be added while drawing
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
                float x = cell.X, y = cell.Y, z = global.Z;
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
                var local = cell.LocalCoords;
                byte light = Math.Max((byte)(this.GetSunlight(local) - map.GetSkyDarkness()), this.GetBlockLight(local));
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
            foreach (var obj in this.Objects)
                obj.DrawInterface(sb, cam);
            foreach (var blockentity in this.BlockEntitiesByPosition)
                blockentity.Value.DrawUI(sb, cam, blockentity.Key.ToGlobal(this));
            this.DrawBlockTokens(sb, cam);
        }
        static readonly float BlockTokenDrawThreshold = Ticks.FromSeconds(2);
        private void DrawBlockTokens(SpriteBatch sb, Camera camera)
        {
            if (camera.Zoom < 1)
                return;
            foreach(var (pos, token) in this.BlockTokens)
                if(token.Lifetime < BlockTokenDrawThreshold)
                    Bar.Draw(sb, camera, pos.ToGlobal(this), "Block HitPoints", token.HealthPercentage, camera.Zoom * .2f);
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
        private void SaveCellsToTagCompressedOptimized(SaveTag chunktag)
        {
            // --- Run tracking ---
            int airRunStart = -1;
            int airRunCount = 0;
            bool airRunDiscovered = false;
            List<(int start, int count, bool discovered)> airRuns = new();

            int solidRunStart = -1;
            List<BitVector32> solidRunData = new();
            List<(int start, List<BitVector32> data)> solidRuns = new();

            // Run-based block/material tracking
            Dictionary<BlockDef, List<(int start, int count)>> blockRuns = new();
            Dictionary<MaterialDef, List<(int start, int count)>> materialRuns = new();

            BlockDef? currentBlock = null;
            MaterialDef? currentMaterial = null;
            int currentBlockRunStart = 0;
            int currentMaterialRunStart = 0;

            for (int i = 0; i < Cells.Length; i++)
            {
                var cell = Cells[i];

                // --- Air run handling ---
                if (cell.Block == BlockDefOf.Air.Worker)
                {
                    if (airRunStart == -1)
                    {
                        // start new air run
                        airRunStart = i;
                        airRunCount = 1;
                        airRunDiscovered = cell.Discovered;
                    }
                    else
                    {
                        airRunCount++;
                    }

                    // flush solid run if it exists
                    if (solidRunStart != -1)
                    {
                        solidRuns.Add((solidRunStart, solidRunData));
                        solidRunData = new List<BitVector32>();
                        solidRunStart = -1;
                    }

                    // flush block/material runs if needed
                    if (currentBlock != null)
                    {
                        var runs = blockRuns[currentBlock];
                        runs.Add((currentBlockRunStart, i - currentBlockRunStart));
                        currentBlock = null;
                    }
                    if (currentMaterial != null)
                    {
                        var runs = materialRuns[currentMaterial];
                        runs.Add((currentMaterialRunStart, i - currentMaterialRunStart));
                        currentMaterial = null;
                    }

                    continue;
                }

                // --- Non-air cell ---
                if (airRunStart != -1)
                {
                    // flush air run
                    airRuns.Add((airRunStart, airRunCount, airRunDiscovered));
                    airRunStart = -1;
                    airRunCount = 0;
                }

                // --- Solid run ---
                if (solidRunStart == -1)
                    solidRunStart = i;
                solidRunData.Add(cell.Data); // BitVector32

                // --- BlockDef run ---
                if (currentBlock != cell.Block.BlockDef)
                {
                    if (currentBlock != null)
                        blockRuns[currentBlock].Add((currentBlockRunStart, i - currentBlockRunStart));

                    currentBlock = cell.Block.BlockDef;
                    if (!blockRuns.ContainsKey(currentBlock))
                        blockRuns[currentBlock] = new List<(int, int)>();
                    currentBlockRunStart = i;
                }

                // --- MaterialDef run ---
                if (currentMaterial != cell.Material)
                {
                    if (currentMaterial != null)
                        materialRuns[currentMaterial].Add((currentMaterialRunStart, i - currentMaterialRunStart));

                    currentMaterial = cell.Material;
                    if (!materialRuns.ContainsKey(currentMaterial))
                        materialRuns[currentMaterial] = new List<(int, int)>();
                    currentMaterialRunStart = i;
                }
            }

            // --- Final flushes ---
            if (airRunStart != -1)
                airRuns.Add((airRunStart, airRunCount, airRunDiscovered));
            if (solidRunStart != -1)
                solidRuns.Add((solidRunStart, solidRunData));
            if (currentBlock != null)
                blockRuns[currentBlock].Add((currentBlockRunStart, Cells.Length - currentBlockRunStart));
            if (currentMaterial != null)
                materialRuns[currentMaterial].Add((currentMaterialRunStart, Cells.Length - currentMaterialRunStart));

            // --- Serialize air runs ---
            var airTag = new SaveTag(SaveTag.Types.List, "Air", SaveTag.Types.Compound);
            foreach (var (start, count, discovered) in airRuns)
            {
                var runTag = new SaveTag(SaveTag.Types.Compound);
                runTag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", start));
                runTag.Add(new SaveTag(SaveTag.Types.Int, "Count", count));
                runTag.Add(new SaveTag(SaveTag.Types.Bool, "Discovered", discovered));
                airTag.Add(runTag);
            }
            chunktag.Add(airTag);

            // --- Serialize solid runs ---
            var solidTag = new SaveTag(SaveTag.Types.List, "Solid", SaveTag.Types.Compound);
            foreach (var (start, data) in solidRuns)
            {
                var runTag = new SaveTag(SaveTag.Types.Compound);
                runTag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", start));
                var dataTag = new SaveTag(SaveTag.Types.List, "Data", SaveTag.Types.Int);
                foreach (var bits in data)
                    dataTag.Add(new SaveTag(SaveTag.Types.Int, "Data", bits.Data)); // store BitVector32 as int
                runTag.Add(dataTag);
                solidTag.Add(runTag);
            }
            chunktag.Add(solidTag);



            // --- Serialize block runs ---
            var blockRunsTag = new SaveTag(SaveTag.Types.Compound, "IndicesByBlock");
            //var blockRunsTag = new SaveTag(SaveTag.Types.List, "IndicesByBlock", SaveTag.Types.List);
            foreach (var kvp in blockRuns)
            {
                var blockTag = new SaveTag(SaveTag.Types.List, kvp.Key.Name, SaveTag.Types.Compound);
                foreach (var (start, count) in kvp.Value)
                {
                    var runTag = new SaveTag(SaveTag.Types.Compound);
                    runTag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", start));
                    runTag.Add(new SaveTag(SaveTag.Types.Int, "Count", count));
                    blockTag.Add(runTag);
                }
                blockRunsTag.Add(blockTag);
            }
            chunktag.Add(blockRunsTag);

            // --- Serialize material runs ---
            var materialRunsTag = new SaveTag(SaveTag.Types.Compound, "IndicesByMaterial");
            //var materialRunsTag = new SaveTag(SaveTag.Types.List, "IndicesByMaterial", SaveTag.Types.List);
            foreach (var kvp in materialRuns)
            {
                var matTag = new SaveTag(SaveTag.Types.List, kvp.Key.Name, SaveTag.Types.Compound);
                foreach (var (start, count) in kvp.Value)
                {
                    var runTag = new SaveTag(SaveTag.Types.Compound);
                    runTag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", start));
                    runTag.Add(new SaveTag(SaveTag.Types.Int, "Count", count));
                    matTag.Add(runTag);
                }
                materialRunsTag.Add(matTag);
            }
            chunktag.Add(materialRunsTag);
        }
        private void LoadCellsFromTagCompressedOptimized(SaveTag chunktag)
        {
            var airRunsTag = chunktag.GetList("Air");
            foreach (var runTag in airRunsTag)
            {
                int startIndex = runTag.GetInt("StartIndex");
                int count = runTag.GetInt("Count");
                bool discovered = runTag.GetBool("Discovered");

                for (int i = startIndex; i < startIndex + count; i++)
                {
                    var cell = Cells[i];
                    cell.Block = BlockDefOf.Air.Worker;
                    cell.Discovered = discovered;
                    // BitVector32 or material not needed for air
                }
            }
            var solidRunsTag = chunktag.GetList("Solid");
            foreach (var runTag in solidRunsTag)
            {
                int startIndex = runTag.GetInt("StartIndex");
                var dataListTag = runTag.GetList("Data");

                for (int offset = 0; offset < dataListTag.Count; offset++)
                {
                    var cell = Cells[startIndex + offset];
                    cell.Data = new BitVector32(dataListTag[offset].GetInt());
                    // BlockDef and MaterialDef will be reconstructed from run-based indices
                }
            }
            var blockRunsTag = chunktag.GetCompound("IndicesByBlock"); // chunktag["IndicesByBlock"].Value as SaveTag;//
            foreach (var runTags in blockRunsTag)
            {
                if (runTags.Key.IsNullEmptyOrWhiteSpace())
                    continue;
                //var runsTag = blockRunsTag[blockName].GetList();
                var blockDef = Def.GetDef<BlockDef>(runTags.Key); // your lookup method
                var listTag = runTags.Value.GetList();
                foreach (var runTag in listTag)
                {
                    int start = runTag.GetInt("StartIndex");
                    int count = runTag.GetInt("Count");
                    for (int i = start; i < start + count; i++)
                        Cells[i].Block = blockDef.Worker;
                }
            }

            var materialRunsTag = chunktag.GetCompound("IndicesByMaterial");
            foreach (var runTags in materialRunsTag)
            {
                if (runTags.Key.IsNullEmptyOrWhiteSpace())
                    continue;
                var runsTag2 = runTags.Value.GetList();
                var materialDef = Def.GetDef<MaterialDef>(runTags.Key);
                foreach (var runTag in runsTag2)
                {
                    int start = runTag.GetInt("StartIndex");
                    int count = runTag.GetInt("Count");
                    for (int i = start; i < start + count; i++)
                        Cells[i].Material = materialDef;
                }
            }

        }
        private void SaveCellsToTagCompressedAsBlockDefsNew(SaveTag chunktag)
        {
            var consecutiveAir = 0;
            bool airIsDiscovered = false;
            bool airRun = false;
            int airRunStartIndex = 0;
            int solidStartIndex = 0;
            Dictionary<BlockDef, List<int>> indicesByBlock = [];
            List<(int startIndex, int count, bool discovered)> airIndicesCounts = [];
            List<(int startIndex, List<Cell> cells)> solidRuns = [];
            List<Cell> currentSolidRunData = [];
            for (int i = 0; i < this.Cells.Length; i++)
            {
                var cell = this.Cells[i];
                if (cell.Block == BlockDefOf.Air.Worker)
                {
                    if (!airRun)
                    {
                        // flush solid run
                        if (i > solidStartIndex)
                        {
                            solidRuns.Add((solidStartIndex, currentSolidRunData));
                            currentSolidRunData = [];
                        }
                        // start air run
                        airRun = true;
                        airRunStartIndex = i;
                        airIsDiscovered = cell.Discovered;
                    }
                    consecutiveAir++;
                    continue;
                }
                if (airRun)
                {
                    // flush air run
                    if (i > airRunStartIndex)
                    {
                        airIndicesCounts.Add((airRunStartIndex, consecutiveAir, airIsDiscovered));
                        airRun = false;
                        consecutiveAir = 0;
                    }

                    // start solid run
                    solidStartIndex = i;
                }

                var blockDef = cell.Block.BlockDef;
                if (!indicesByBlock.TryGetValue(blockDef, out var list))
                    indicesByBlock[blockDef] = list = [];
                list.Add(i);
                currentSolidRunData.Add(cell);
            }
            // wrap final run
            if (airRun)
                airIndicesCounts.Add((airRunStartIndex, consecutiveAir, airIsDiscovered));
            else
                solidRuns.Add((solidStartIndex, currentSolidRunData));


            // save air runs
            var airtag = new SaveTag(SaveTag.Types.List, "Air", SaveTag.Types.Compound);
            foreach (var (startIndex, count, discovered) in airIndicesCounts)
            {
                var currentairtag = new SaveTag(SaveTag.Types.Compound);
                currentairtag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", startIndex));
                currentairtag.Add(new SaveTag(SaveTag.Types.Int, "Count", count));
                currentairtag.Add(new SaveTag(SaveTag.Types.Bool, "Discovered", discovered));
                airtag.Add(currentairtag);
            }
            chunktag.Add(airtag);

            // save solid runs
            var solidtag = new SaveTag(SaveTag.Types.List, "Solid", SaveTag.Types.Compound);
            foreach (var (startIndex, cells) in solidRuns)
            {
                var currentsolidruntag = new SaveTag(SaveTag.Types.Compound);
                currentsolidruntag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", startIndex));
                foreach(var cell in cells)
                    currentsolidruntag.Add(cell.SaveNew());
                solidtag.Add(currentsolidruntag);
            }
            chunktag.Add(solidtag);

            List<SaveTag> asd = [];
            foreach (var (blockDef, indices) in indicesByBlock)
                asd.Add(indices.Save(blockDef.Name));
            var indicesByBlockTag = new SaveTag(SaveTag.Types.Compound, "IndicesByBlock", asd);
            chunktag.Add(indicesByBlockTag);
        }
        private void SaveCellsToTagCompressedAsBlockDefs(SaveTag chunktag)
        {
            SaveTag cellstag = new(SaveTag.Types.List, "Cells", SaveTag.Types.Compound);
            var consecutiveAir = 0;
            bool airIsDiscovered = false;
            bool airRun = false;
            //bool solidRun = false;
            int airRunStartIndex = 0;
            int solidStartIndex = 0;
            Dictionary<BlockDef, List<int>> indicesByBlock = [];
            List<(int startIndex, int count, bool discovered)> airIndicesCounts = [];
            List<(int startIndex, List<(MaterialDef material, int data)>)> solidRuns = [];
            for (int i = 0; i < this.Cells.Length; i++)
            {
                var cell = this.Cells[i];
                if (cell.Block == BlockDefOf.Air.Worker)
                {
                    // start air run
                    if (!airRun)
                    {
                        airRun = true;
                        airRunStartIndex = i;
                        airIsDiscovered = cell.Discovered;
                    }
                    consecutiveAir++;
                    continue;
                }
                else
                {
                    if(airRun)
                    {
                        airIndicesCounts.Add((airRunStartIndex, consecutiveAir, airIsDiscovered));
                        airRun = false;
                        consecutiveAir = 0;
                    }
                }
                
                var blockDef = cell.Block.BlockDef;
                if (!indicesByBlock.TryGetValue(blockDef, out var list))
                    indicesByBlock[blockDef] = list = [];
                list.Add(i);
                cellstag.Add(cell.SaveNew());
            }
            // wrap final air run
            if(airRun)
            {
                airIndicesCounts.Add((airRunStartIndex, consecutiveAir, airIsDiscovered));
            }
            // save air runs
            var airtag = new SaveTag(SaveTag.Types.List, "Air", SaveTag.Types.Compound);
            foreach (var (startIndex, count, discovered) in airIndicesCounts)
            {
                var currentairtag = new SaveTag(SaveTag.Types.Compound);
                currentairtag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", startIndex));
                currentairtag.Add(new SaveTag(SaveTag.Types.Int, "Count", count));
                currentairtag.Add(new SaveTag(SaveTag.Types.Bool, "Discovered", discovered));
                airtag.Add(currentairtag);
            }
            chunktag.Add(airtag);

            List<SaveTag> asd = new();
            foreach (var (blockDef, indices) in indicesByBlock)
                asd.Add(indices.Save(blockDef.Name));
            var indicesByBlockTag = new SaveTag(SaveTag.Types.Compound, "IndicesByBlock", asd);
            chunktag.Add(indicesByBlockTag);

            //// TODO when the last cell in the cell array is air, the air savetag isn't written
            //if (consecutiveAir > 0)
            //    saveAirTag(cellstag, consecutiveAir, airIsDiscovered);

            chunktag.Add(cellstag);

            static void saveAirTag(SaveTag cellstag, int airLength, bool airIsDiscovered)
            {
                var airtag = new SaveTag(SaveTag.Types.Compound);
                //airtag.Save(BlockDefOf.Air, "Block");
                airtag.Add(new SaveTag(SaveTag.Types.Int, "Data", airLength));
                airtag.Add(new SaveTag(SaveTag.Types.Bool, "Discovered", airIsDiscovered));
                cellstag.Add(airtag);
            }
        }
        private void SaveCellsToTagCompressed(SaveTag chunktag)
        {
            SaveTag cellstag = new(SaveTag.Types.List, "Cells", SaveTag.Types.Compound);
            var airLength = 0;
            bool airIsDiscovered = false;
            bool foundAir = false;
            foreach (var cell in this.Cells)
            {
                if (cell.Block == BlockDefOf.Air.Worker)
                {
                    airLength++;
                    if (!foundAir)
                    {
                        foundAir = true;
                        airIsDiscovered = cell.Discovered;
                    }
                    else if (airIsDiscovered != cell.Discovered)
                    {
                        foundAir = false;
                        saveAirTag(cellstag, airLength, airIsDiscovered);
                        airLength = 0;
                    }
                    continue;
                }

                // TODO when the last cell in the cell array is air, the air savetag isn't written
                if (airLength > 0)
                {
                    foundAir = false;
                    saveAirTag(cellstag, airLength, airIsDiscovered);
                    airLength = 0;
                }

                cellstag.Add(cell.Save());
            }
            // TODO when the last cell in the cell array is air, the air savetag isn't written
            if (airLength > 0)
                saveAirTag(cellstag, airLength, airIsDiscovered);

            chunktag.Add(cellstag);

            static void saveAirTag(SaveTag cellstag, int airLength, bool airIsDiscovered)
            {
                var airtag = new SaveTag(SaveTag.Types.Compound);
                airtag.SaveDef("Block", BlockDefOf.Air);
                airtag.Add(new SaveTag(SaveTag.Types.Int, "Data", airLength));
                airtag.Add(new SaveTag(SaveTag.Types.Bool, "Discovered", airIsDiscovered));
                cellstag.Add(airtag);
            }
        }
        private void LoadCellsFromTagCompressedAsBlockDefs(SaveTag chunktag)
        {
            var celllist = chunktag["Cells"].Value as List<SaveTag>;
            var airlist = chunktag["Air"].Value as List<SaveTag>;
            var byblock = chunktag["IndicesByBlock"].Value as List<SaveTag>;
            int n = 0;
            var airCount = 0;
            bool airDiscovered = true;
            var listPosition = 0;
            var maxn = Size * Size * MapBase.MaxHeight;
            for(int i = 0; i < this.Cells.Length; i++)
            {

            }
            foreach(var airtag in airlist)
            {
                var index = airtag.GetValue<int>("StartIndex");
                var count = airtag.GetValue<int>("Count");
                var discovered = airtag.GetValue<bool>("Discovered");
                for(int i = index; i < count; i++)
                    this.Cells[i].Set(BlockDefOf.Air.Worker, MaterialDefOf.Air, 0, discovered);
            }
            foreach(var blockindices in byblock)
            {
                var bdef = Def.GetDef<BlockDef>(blockindices.Name);
                foreach (var inttag in blockindices.Value as List<SaveTag>)
                {
                    var index = (int)inttag.Value;

                }
            }
            while (n < maxn)
            {

            }
            while (listPosition < celllist.Count)
            {
                var celltag = celllist[listPosition++];
                var block = celltag.LoadDef<BlockDef>("Block").Worker;
                if (block.BlockDef == BlockDefOf.Air)
                {
                    airCount = (int)celltag["Data"].Value;
                    celltag.TryGetTagValue("Discovered", ref airDiscovered);
                    for (int i = n; i < n + airCount; i++)
                    {
                        var c = this.Cells[i];
                        c.Block = BlockDefOf.Air.Worker;
                        c.Discovered = airDiscovered;
                    }

                    n += airCount;

                    continue;
                }
                var cell = this.Cells[n++];
                cell.LoadWithoutBlock(celltag);
            }

            var indicesByBlock = chunktag["IndicesByBlock"].Value as List<SaveTag>;
            foreach(var tag in indicesByBlock)
            {
                var blockDef = Def.GetDef<BlockDef>(tag.Name);
                var worker = blockDef.Worker;
                foreach(var index in tag.Value as List<int>)
                    this.Cells[index].Block = worker;
            }
        }
        private void LoadCellsFromTagCompressed(SaveTag chunktag)
        {
            var celllist = chunktag["Cells"].Value as List<SaveTag>;
  
            int n = 0;
            var airCount = 0;
            bool airDiscovered = true;
            var listPosition = 0;
            var maxn = Size * Size * MapBase.MaxHeight;
            while (listPosition < celllist.Count)
            {
                var celltag = celllist[listPosition++];
                var block = celltag.LoadDef<BlockDef>("Block").Worker;

                //if (block == BlockDefOf.Air)
                if (block.BlockDef == BlockDefOf.Air)
                {
                    airCount = (int)celltag["Data"].Value;
                    celltag.TryGetTagValue("Discovered", ref airDiscovered);
                    for (int i = n; i < n + airCount; i++)
                    {
                        var c = this.Cells[i];
                        c.Discovered = airDiscovered;
                    }

                    n += airCount;

                    continue;
                }
                var cell = this.Cells[n++];
                cell.Load(celltag);

            }

            //Cell[] newCells = new Cell[this.Cells.Length];
            //for (int z = 0; z < MapBase.MaxHeight; z++)
            //    for (int y = 0; y < Size; y++)
            //        for (int x = 0; x < Size; x++)
            //        {
            //            int oldIndex = (z * Size + x) * Size + y;
            //            int newIndex = (z * Size + y) * Size + x;
            //            var cell = this.Cells[oldIndex];
            //            newCells[newIndex] = cell;
            //            cell.X = (byte)x;
            //            cell.Y = (byte)y;
            //        }
            //this.Cells = newCells;
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
                    //var entity = this.GetLocalCell(originGlobal.ToLocal()).Block.BlockDef.CreateEntity(originGlobal);
                    //entity.Read(r);
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

        public List<IntVec3> GetEdges(Edges edges)
        {
            var list = new HashSet<IntVec3>();
            if ((edges & Edges.East) == Edges.East)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < MapBase.MaxHeight; z++)
                        list.Add(new IntVec3(this.Start.X + Chunk.Size - 1, this.Start.Y + i, z));

            if ((edges & Edges.West) == Edges.West)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < MapBase.MaxHeight; z++)
                        list.Add(new IntVec3(this.Start.X, this.Start.Y + i, z));

            if ((edges & Edges.North) == Edges.North)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < MapBase.MaxHeight; z++)
                        list.Add(new IntVec3(this.Start.X + i, this.Start.Y, z));

            if ((edges & Edges.South) == Edges.South)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < MapBase.MaxHeight; z++)
                        list.Add(new IntVec3(this.Start.X + i, this.Start.Y + Chunk.Size - 1, z));

            return list.ToList();
        }
        internal IEnumerable<GameObject> GetObjectsLazy()
        {
            foreach (var obj in this.Objects)
                yield return obj;
        }
        internal List<GameObject> GetObjects()
        {
            return new List<GameObject>(this.Objects);
        }

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
            var refids = this.Objects.Select(o => o.RefId).ToList();
            if (refids.Any(c => c == 0))
                throw new Exception();
            writer.Write(refids);
            
            this.WriteBlockEntitiesDistinct(writer);
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    //for (int j = 0; j < Size; j++)
                    writer.Write(this.HeightMap[i][j]);

            writer.Write(this.Sunlight);
            writer.Write(this.BlockLight);
        }
        void Read(IDataReader reader)
        {
            this.MapCoords = reader.ReadVector2();

            this.LightValid = reader.ReadBoolean();
            this.EdgesValid = reader.ReadBoolean();

            // TODO: OPTIMIZE
            this.InitCells();

            var serializer = new ChunkSerializer();
            serializer.Deserialize(this, reader);

            var entityRefIds = reader.ReadListInt32();
            foreach (var refId in entityRefIds)
                this.Add(this.Map.World.GetEntity(refId));
            this.ReadBlockEntitiesDistinct(reader);
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    this.HeightMap[i][j] = reader.ReadInt32();

            this.Sunlight = reader.ReadBytes(Volume);//.ToList();
            this.BlockLight = reader.ReadBytes(Volume);
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
        {
            return global.GetChunkCoords() == this.MapCoords;
        }

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
                byte light = (byte)((this.Sunlight[n] << 4) + this.BlockLight[n++]);
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

            var entityRefIds = this.Objects.Select(e => e.RefId).ToList();
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
                        this.Sunlight[n] = sunlight;
                        this.BlockLight[n] = blocklight;
                        n++;
                    }

            var heightTag = chunktag["Heightmap"].Value as List<SaveTag>;
            n = 0;
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    this.HeightMap[i][j] = (byte)heightTag[n++].Value;

            var list = chunktag.LoadListInt("Entities");
            foreach (var refId in list)
                this.Add(this.Map.World.GetEntity(refId));

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

        internal Block GetBlockFromGlobal(float globalx, float globaly, float globalz)
        {
            return this[globalx - this.Start.X, globaly - this.Start.Y, globalz].Block;
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

        public void BuildSlice(Slice slice, Camera camera, MapBase map, int z)
        {
            var unknown = new List<Cell>();
            var visible = new List<Cell>();

            // create the slice's undiscovered blocks mesh
            for (int i = 0; i < Chunk.Size; i++)
                for (int j = 0; j < Chunk.Size; j++)
                {
                    var local = new IntVec3(i, j, z);
                    var cell = this.Cells[GetCellIndex(local)];
                    var global = local.ToGlobal(this);

                    // DO I NEED THIS?
                    if (!camera.MysteriousBlocks)
                    {
                        if (cell.Block != BlockDefOf.Air.Worker)
                        {
                            if (!map.IsVisible(global))
                                unknown.Add(cell);
                            else // did i need visibleoutercells list afterall?
                                visible.Add(cell);
                        }
                    }
                    else
                    {
                        if (map.IsUndiscovered(global) || !map.IsVisible(global)) // did i need visibleoutercells list afterall?
                        {
                            unknown.Add(cell);
                        }
                        else
                        {
                            if (cell.Block != BlockDefOf.Air.Worker)
                                visible.Add(cell);
                        }
                    }
                }

            var unknownCount = unknown.Count;
            var unknownSlice = new MySpriteBatch(Game1.Instance.GraphicsDevice, unknownCount);
            var topCover = new Canvas(Game1.Instance.GraphicsDevice, unknownCount);

            foreach (var cell in unknown)
            {
                if (camera.MysteriousBlocks)
                    camera.DrawUnknown(unknownSlice, map, this, cell);
                else
                    camera.DrawCell(topCover, map, this, cell);
            }

            var visibleCount = visible.Count;
            var canvas = new Canvas(Game1.Instance.GraphicsDevice, visibleCount);
            for (int i = 0; i < visibleCount; i++)
            {
                var cell = visible[i];
                camera.DrawCell(canvas, map, this, cell);
            }
            slice.Canvas = canvas;
            slice.Cover = topCover;
            slice.Unknown = unknownSlice;
        }
        public void BuildSliceNew(Slice slice, Camera camera, MapBase map, int z, (int x, int y) frontCells)
        {
            var maxCapacity = Size * Size;
            var obstructed = new List<Cell>(maxCapacity);
            var mysterious = new List<Cell>(maxCapacity);
            var visible = new List<Cell>(maxCapacity);
            var frontmost = new List<Cell>(maxCapacity);
            var frontmostMysterious = new List<Cell>(maxCapacity);

            var canvas = new Canvas(Game1.Instance.GraphicsDevice, visible.Count + frontmost.Count + frontmostMysterious.Count);

            for (int i = 0; i < Chunk.Size; i++)
                for (int j = 0; j < Chunk.Size; j++)
                {
                    var local = new IntVec3(i, j, z);
                    var cell = this.Cells[GetCellIndex(local)];
                    var global = local.ToGlobal(this);
                    var isair = cell.Block == BlockDefOf.Air.Worker;// BlockDefOf.Air;
                    // HACK
                    if (isair && this.Map.Town.ConstructionsManager.IsDesignatedConstruction(global)) 
                     //if (isair && this.Map.Town.DesignationManager.IsDesignation(global, DesignationDefOf.Construct)) // HACK
                        camera.DrawBlock(canvas, BlockDefOf.Designation.Worker, map, this, local);

                    var isobstructed = !map.IsVisible(global);// || !(global.X == frontCellX || global.Y == frontCellY);
                    var isundiscovered = map.IsUndiscovered(global);
                    var ismysterious = camera.MysteriousBlocks && isundiscovered;

                    if (global.X == frontCells.x || global.Y == frontCells.y)
                    {
                        if (ismysterious)
                            frontmostMysterious.Add(cell);
                        if (!isair)
                            frontmost.Add(cell);
                    }
                    else
                    {
                        if (ismysterious)
                            mysterious.Add(cell);
                        else
                        {
                            if (!isair)
                            {
                                if (isobstructed)
                                    obstructed.Add(cell);
                                else
                                    visible.Add(cell);
                            }
                        }
                    }
                }

            var topCover = new Canvas(Game1.Instance.GraphicsDevice, obstructed.Count + mysterious.Count);

            foreach(var cell in obstructed)
                camera.DrawCell(topCover, map, this, cell);

            foreach(var cell in mysterious)
                camera.DrawUnknown(topCover, map, this, cell);


            foreach(var cell in visible)
                camera.DrawCell(canvas, map, this, cell);

            foreach(var cell in frontmost)
                camera.DrawCell(canvas, map, this, cell);

            foreach (var cell in frontmostMysterious)
                camera.DrawUnknown(canvas, map, this, cell);

            slice.Canvas = canvas;
            slice.Cover = topCover;
        }
        public void BuildFrontmostBlocksNewSlicesNew(Camera camera)
        {
            var chunkX = this.MapCoords.X;
            var chunkY = this.MapCoords.Y;
            var mapSizeInChunks = this.Map.GetSizeInChunks();
            int edgeX = 0, edgeY = 0;
            IntVec3 offset;
            switch ((int)camera.Rotation)
            {
                case 0:
                    edgeX = mapSizeInChunks - 1;
                    edgeY = mapSizeInChunks - 1;
                    offset.X = Chunk.Size - 1;
                    offset.Y = Chunk.Size - 1;
                    break;
                case 1:
                    edgeX = mapSizeInChunks - 1;
                    edgeY = 0;
                    offset.X = Chunk.Size - 1;

                    break;
                case 2:
                    edgeX = 0;
                    edgeY = 0;
                    break;
                case 3:
                    edgeX = 0;
                    edgeY = mapSizeInChunks - 1;
                    offset.Y = Chunk.Size - 1;
                    break;
                default:
                    break;
            }
            var maxheight = this.Map.GetMaxHeight();
            var map = this.Map;
            
        }

        [InspectorHidden]
        public Slice[] Slices = new Slice[128];

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
                foreach(var cell in entity.CellsOccupied)
                    this.BlockEntitiesByPosition.Remove(cell.ToLocal());
            }
            return entity is not null;
        }

        public IEnumerable<(IntVec3 local, BlockEntity entity)> GetBlockEntitiesByPosition()
        {
            foreach (var be in this.BlockEntitiesByPosition)
                yield return (be.Key, be.Value);
        }

        internal void ApplyBlockWork(IntVec3 local, int work)
        {
            if (!this.BlockTokens.TryGetValue(local, out var token))
            {
                token = new(this.GetLocalCell(local));
                this.BlockTokens.Add(local, token);
            }
            if (token.ApplyWork(work))
                this.InvalidateSlice(token.Cell.Z);
        }
    }
}