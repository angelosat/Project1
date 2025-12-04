using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class StaticMap : MapBase, ITooltippable
    {
        public override float LoadProgress => this.ActiveChunks.Count / (float)(this.Size.Chunks * this.Size.Chunks); 

        public MapSize Size;
        public class MapSize : INamed
        {
            public string Name { get; private set; }
            public int Blocks { get; private set; }
            public int Chunks { get; private set; }
            public MapSize(
                string name, int blocks)
            {
                this.Name = name;
                this.Blocks = blocks;
                this.Chunks = blocks / Chunk.Size;
            }
            public static readonly MapSize Micro = new("Micro", 32);
            public static readonly MapSize Tiny = new("Tiny", 64);
            public static readonly MapSize Small = new("Small", 128);
            public static readonly MapSize Normal = new("Normal", 256);
            public static readonly MapSize Huge = new("Huge", 512);

            public static MapSize Default = Micro;

            public static List<MapSize> GetList()
            {
                return new List<MapSize>() { Micro, Tiny, Small, Normal, Huge };
            }
        }

        public byte SkyDarkness = 0, SkyDarknessMax = 13;
        public Color AmbientColor = Color.Blue;//Color.MidnightBlue; //Color.RoyalBlue;//Color.MidnightBlue; //Color.MediumPurple; //Color.Lerp(Color.White, Color.Cornsilk, 0.5f);

        public bool Lighting = true;
        public int TickLengthSeconds = (int)(60 * 1.44f); // one tick is 1.44 ingame minutes
        public const int Zenith = 14;
        public double DayTimeNormal = 0;
        public Vector2 Global;

        public static int VisibleCellCount = 0;
        public Game1 game;
        public bool hasClicked = false;
        public static float MaxDepth = 0, MinDepth = 0;
        public List<Texture2D> VisibleTileTypes;
        public Vector2 tileLocation = new(16, 8);
        public const double GroundDensity = 0.1;
        public static List<Rectangle> Icons;

        public string Name;
        public Texture2D[] Thumbnails;
        public MapThumb Thumb;
        internal void Init()
        {
            this.Town.Init();
        }

        internal Vector3 GetRandomEdgeCell()
        {
            var i = (int)(this.Size.Blocks * this.Random.NextDouble());
            var j = this.Random.Chance(.5f) ? 0 : this.Size.Blocks - 1;
            var vec2 = this.Random.Chance(.5f) ? new IntVec2(i, j) : new IntVec2(j, i);
            return new Vector3(vec2.X, vec2.Y, this.GetHeightmapValue(vec2.X, vec2.Y));
        }

        public List<GameObject> SavedPlayers = new();

        public override bool AddChunk(Chunk chunk)
        {
            this.ActiveChunks.Add(chunk.MapCoords, chunk);
            // sort chunks back to front to prevent glitches with semi-transparent blocks on chunk edges
            this.ActiveChunks = this.ActiveChunks.OrderBy(c => c.Key.X + c.Key.Y).ToDictionary(i => i.Key, i => i.Value);
            return true;
        }

        public StaticMap(string name = "")
        {
            this.LightingEngine = new LightingEngine(this);
            this.Camera = new Camera(Game1.Bounds.Width, Game1.Bounds.Height);
            this.Name = name;
            this.ActiveChunks = new Dictionary<IntVec2, Chunk>();
            this.Thumbnails = new Texture2D[3];
            this.Town = new Town(this);
            this.Regions = new RegionManager(this);
            this.UndiscoveredAreaManager = new UndiscoveredAreaManager(this);
            this.ParticleManager = new Particles.ParticleManager(this);
        }
        public StaticMap(StaticWorld world, Vector2 coords, string name = "")
            : this(name)
        {
            this.World = world;
            this.Coordinates = coords;
            this.Size = MapSize.Default;// MapSize.Normal;
            this.Global = this.Coordinates * this.Size.Blocks;
            this.Thumb = new MapThumb(this);
        }
        public StaticMap(StaticWorld world, string name, Vector2 coords, MapSize size)
            : this(name)
        {
            this.World = world;
            this.Coordinates = coords;
            this.Size = size;
            this.Global = this.Coordinates * this.Size.Blocks;
            this.Thumb = new MapThumb(this);
        }

        public void AddTime()
        {
            var clock = this.Clock;
            double normal = (clock.TotalMinutes - Ticks.PerSecond * (Zenith - 12)) / 1440f;
            double nn = normal * 2 * Math.PI;
            nn = 3 * Math.Cos(nn);
            this.DayTimeNormal = Math.Max(0, Math.Min(1, (1 + nn) / 2f));
            this.SkyDarkness = 0;
        }
      
        internal void Despawn(Actor actor)
        {
            actor.Despawn();
            PacketEntityDespawn.Send(this.Net, actor);
        }

        #region Updating
        public override void Update()
        {
            IconOffset = (float)Math.Sin(this.Net.Clock.TotalMilliseconds / Ticks.PerSecond);
            this.Sunlight = 1 - (float)this.GetDayTimeNormal();
            this.TryPerformQueuedRandomBlockUpdates();
            this.CachedAmbientColor = this.UpdateAmbientColor();

            this.CacheObjects();
            this.CacheBlockEntities();

            foreach (var chunk in this.ActiveChunks.Values.ToList())
                chunk.Update();

            this.Town.Update();
            this.ApplyLightChanges();
        }

        private void TryPerformQueuedRandomBlockUpdates()
        {
            while (this.RandomBlockUpdateQueue.Any())
            {
                var global = this.RandomBlockUpdateQueue.Peek();
                var cell = this.GetCell(global);
                if (cell == null)
                {
                    continue;
                }

                cell.Block.RandomBlockUpdate(this.Net, global, cell);
                this.RandomBlockUpdateQueue.Dequeue();
            }
        }
        public override void Tick()
        {
            this.AddTime();
            this.Regions.Update();
            foreach (var chunk in this.ActiveChunks.Values.ToList())
                chunk.Tick();

            this.Town.Tick();
        }

        #endregion

        #region Drawing

        public override void DrawBlocks(MySpriteBatch sb, Camera camera, EngineArgs a)
        {
            var copyOfActiveChunks = new Dictionary<IntVec2, Chunk>(this.ActiveChunks);
            Vector3? playerGlobal = null;
            var hiddenRects = new List<Rectangle>();

            camera.UpdateMaxDrawLevel(this);

            foreach (var chunk in copyOfActiveChunks)
            {
                var chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, MaxHeight / 2, Chunk.Bounds);
                if (!camera.ViewPort.Intersects(chunkBounds))
                    continue;
                camera.DrawChunk(sb, this, chunk.Value, playerGlobal, hiddenRects, a);
            }
        }

        public override void DrawObjects(MySpriteBatch sb, Camera camera, SceneState scene)
        {
            foreach (var chunk in this.ActiveChunks)
            {
                var chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, MaxHeight / 2, Chunk.Bounds);
                if (camera.ViewPort.Intersects(chunkBounds))
                    chunk.Value.DrawObjects(sb, camera, Controller.Instance, this, scene);
            }
        }

        public override void DrawInterface(SpriteBatch sb, Camera camera)
        {
            var copyOfActiveChunks = new Dictionary<IntVec2, Chunk>(this.ActiveChunks);
            foreach (var chunk in copyOfActiveChunks)
            {
                Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, MaxHeight / 2, Chunk.Bounds);  //chunk.Value.GetBounds(camera);
                if (camera.ViewPort.Intersects(chunkBounds))
                    chunk.Value.DrawInterface(sb, camera);
                Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            }
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            this.Town.DrawUI(sb, camera);
        }

        #endregion

        public override string GetFullPath()
        {
            return GlobalVars.SaveDir + @"Worlds\Static\" + this.World.Name + @" \" + this.GetFolderName() + @"\";
        }
        public override string GetFolderName()
        {
            return this.Coordinates.X.ToString() + "." + this.Coordinates.Y.ToString();
        }

        public override SaveTag Save()
        {
            return this.SaveToTag();
        }

        SaveTag SaveToTag()
        {
            var mapTag = new SaveTag(SaveTag.Types.Compound, "Map");

            mapTag.Add(new SaveTag(SaveTag.Types.Int, "X", (int)this.Coordinates.X));
            mapTag.Add(new SaveTag(SaveTag.Types.Int, "Y", (int)this.Coordinates.Y));
            mapTag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
            this.CurrentTick.Save(mapTag, "CurrentTick");
            mapTag.Add(this.Town.Save("Town"));

            mapTag.Add(new SaveTag(SaveTag.Types.String, "Size", this.Size.Name));

            SaveTag playerTag = new SaveTag(SaveTag.Types.Compound, "Player");
            mapTag.Add(playerTag);

            var chunkstags = this.SaveChunks();
            mapTag.Add(chunkstags);

            var sw = Stopwatch.StartNew();
            mapTag.Add(this.UndiscoveredAreaManager.Save("UndiscoveredAreas"));
            sw.Stop();
            $"undiscovered areas saved in {sw.ElapsedMilliseconds} ms".ToConsole();
            sw.Stop();

            mapTag.Add(this.RandomOrderedChunkIndices.Save("_RandomOrderedChunkIndices")); //save the property to force it to initialize if it's not already

            return mapTag;
        }
        private SaveTag SaveChunks()
        {
            var chunkstags = new SaveTag(SaveTag.Types.List, "Chunks", SaveTag.Types.Compound);
            foreach (var ch in this.ActiveChunks)
            {
                var chtag = new SaveTag(SaveTag.Types.Compound);
                chtag.Add(ch.Key.Save("Key"));
                chtag.Add(ch.Value.SaveToTag());
                chunkstags.Add(chtag);
            }
            return chunkstags;
        }
        public void LoadChunks(SaveTag tag)
        {
            var list = tag.Value as List<SaveTag>;
            foreach (var item in list)
            {
                var key = item["Key"].LoadVector2();
                var chunk = Chunk.Load(this, key, item["Chunk"]);
                this.ActiveChunks.Add(key, chunk);
            }
            this.InitChunks();
        }

        public override void GenerateThumbnails()
        {
            this.GenerateThumbnails(this.GetFullPath());
        }
        public override void GenerateThumbnails(string fullMapDir)
        {
            if (!Directory.Exists(fullMapDir))
                Directory.CreateDirectory(fullMapDir);

            if (this.ActiveChunks.Count > 0)
            {
                using Texture2D thumbnail = this.GetThumbnail();
                using (var stream = new FileStream(fullMapDir + "thumbnailSmall.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width, thumbnail.Height);
                    stream.Close();
                }
                using (var stream = new FileStream(fullMapDir + "thumbnailSmaller.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width / 2, thumbnail.Height / 2);
                    stream.Close();
                }
                using (var stream = new FileStream(fullMapDir + "thumbnailSmallest.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width / 4, thumbnail.Height / 4);
                    stream.Close();
                }
            }
        }
        internal override void OnHudCreated(Hud hud)
        {
            this.Town.OnHudCreated(hud);
        }
        public static StaticMap Load(StaticWorld world, Vector2 coords, SaveTag mapTag)
        {
            var map = new StaticMap(world, coords)
            {
                Name = (string)mapTag["Name"].Value,
                Coordinates = new Vector2((int)mapTag["X"].Value, (int)mapTag["Y"].Value)
            };

            mapTag.TryGetTagValue<string>("Size", txt => map.Size = MapSize.GetList().First(f => f.Name == txt));

            mapTag.TryGetTag("Chunks", map.LoadChunks);
            mapTag.TryGetTag("Town", tag => map.Town.Load(tag)); // LOAD TOWN AFTER CHUNKS because references are resolved pertaining to the map

            mapTag.TryGetTag("UndiscoveredAreas", map.UndiscoveredAreaManager.Load);
            mapTag.TryGetTag("_RandomOrderedChunkIndices", t => map._randomOrderedChunkIndices = new List<int>().Load(t).ToArray());

            return map;
        }

        public override void LoadThumbnails()
        {
            this.LoadThumbnails(this.GetFullPath());
        }
        public void LoadThumbnails(string folderPath)
        {
            var thumbFiles = new List<FileInfo>();

            int i = 0;
            foreach (FileInfo thumbFile in thumbFiles)
            {
                using FileStream stream = new(thumbFile.FullName, FileMode.Open);
                Texture2D tex = Texture2D.FromStream(Game1.Instance.GraphicsDevice, stream);
                this.Thumbnails[i] = tex;
                this.Thumb.Sprites[i++] = new Sprite(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, tex.Bounds.Center.ToVector());
            }
        }

        public bool InitChunks(Action<string, float> callback = null)
        {
            callback?.Invoke("Post processing chunks", 0);
            var sw = Stopwatch.StartNew();

            this.ResetChunkEdges();
            $"chunk edges reset in {sw.ElapsedMilliseconds} ms".ToConsole();

            this.Regions.Init();
            callback?.Invoke("Cacheing objects", 0);

            this.FinishLoading();

            return true;
        }
        public IEnumerable<(string, Action)> InitChunksNew()
        {
            yield return ("Post processing chunks", () =>
            {
                var sw = Stopwatch.StartNew();
                this.ResetChunkEdges();
                $"chunk edges reset in {sw.ElapsedMilliseconds} ms".ToConsole();
            });
            yield return ("Initializing Regions", this.Regions.Init);
            yield return ("Caching objects", this.FinishLoading);
        }
        void ResetChunkEdges()
        {
            foreach (var ch in this.ActiveChunks.Values)
            {
                /// i'm calculating light at the end of map generation
                //if (!ch.LightValid)
                //{
                //    this.ResetLight(ch);
                //    ch.LightValid = true;
                //    this.UpdateChunkNeighborsLight(ch);
                //}
                foreach (var vector in ch.MapCoords.GetNeighbors())
                {
                    if (this.ActiveChunks.TryGetValue(vector, out var neighbor))
                        neighbor.InvalidateEdges();
                }
            }
        }
        void UpdateChunkNeighborsLight(Chunk chunk)
        {
            var actives = this.GetActiveChunks();

            if (actives.TryGetValue(chunk.MapCoords + new IntVec2(1, 0), out var neighbor))
                this.LightingEngine.HandleImmediate(neighbor.GetEdges(Edges.West));

            if (actives.TryGetValue(chunk.MapCoords + new IntVec2(-1, 0), out neighbor))
                this.LightingEngine.HandleImmediate(neighbor.GetEdges(Edges.East));

            if (actives.TryGetValue(chunk.MapCoords + new IntVec2(0, 1), out neighbor))
                this.LightingEngine.HandleImmediate(neighbor.GetEdges(Edges.North));

            if (actives.TryGetValue(chunk.MapCoords + new IntVec2(0, -1), out neighbor))
                this.LightingEngine.HandleImmediate(neighbor.GetEdges(Edges.South));
        }

        void ResetLight(Chunk chunk)
        {
            var cellList = chunk.ResetHeightMap();
            this.UpdateLight(cellList);
        }

        public override Texture2D GetThumbnail()
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            float zoom = 1 / 8f;
            int width = (int)(this.Size.Blocks * Block.Width * zoom);
            Vector2 mapCoords = this.Global;
            var camera = new Camera(width, width, x: mapCoords.X, y: mapCoords.Y, z: MaxHeight / 2, zoom: zoom);
            var final = new RenderTarget2D(gd, width, width);
            camera.NewDraw(final, this, gd, EngineArgs.Default, new SceneState(), ToolManager.Instance);
            gd.SetRenderTarget(null);
            return final;
        }

        public static StaticMap Create(StaticWorld world, Vector2 coords)
        {
            var map = new StaticMap(world, coords);//a);
            world.Maps[coords] = map;
            return map;
        }
        public override void GetTooltipInfo(Control tooltip)
        {
            tooltip.AddControls(this.ToString().ToLabel());
        }

        public void CacheBlockEntities()
        {
            var list = new Dictionary<IntVec3, BlockEntity>();
            foreach (var chunk in this.ActiveChunks.Values)
                foreach (var (local, entity) in chunk.GetBlockEntitiesByPosition())
                    list.Add(local.ToGlobal(chunk), entity);

            this.CachedBlockEntities = list;
        }
        public void CacheObjects()
        {
            var list = new List<GameObject>();
            foreach (var chunk in this.ActiveChunks)
                list.AddRange(chunk.Value.GetObjects());

            this.CachedObjects = list;
        }
        public override IEnumerable<GameObject> GetObjects()
        {
            return this.CachedObjects.Where(o => o.Exists); // because an object might have despawned during an earlier operation on the current frame
        }
        public override IEnumerable<GameObject> GetObjects(Vector3 min, Vector3 max)
        {
            return this.GetObjects(new BoundingBox(min, max));
        }
        public override IEnumerable<GameObject> GetObjects(BoundingBox box)
        {
            foreach (var o in this.CachedObjects)
            {
                var type = box.Contains(o.Global);
                if (type != ContainmentType.Disjoint)
                    yield return o;
            }
        }

        public override void WriteData(BinaryWriter w)
        {
            w.Write(this.Name);
            w.Write(this.Coordinates.X);
            w.Write(this.Coordinates.Y);
            w.Write(this.Size.Name);
            this.Town.Write(w);
            this.UndiscoveredAreaManager.Write(w);
        }

        public static StaticMap ReadData(INetEndpoint net, IDataReader r)
        {
            var map = new StaticMap
            {
                Name = r.ReadString(),
                Coordinates = new Vector2(r.ReadSingle(), r.ReadSingle()),
            };
            var size = r.ReadString();
            map.Size = MapSize.GetList().First(foo => foo.Name == size);
            map.Town.Read(r);
            map.UndiscoveredAreaManager.Read(r);
            return map;
        }

        public override void OnGameEvent(GameEvent e)
        {
            base.OnGameEvent(e);
            this.Town.HandleGameEvent(e);
            this.Regions.OnGameEvent(e);
            this.UndiscoveredAreaManager.OnGameEvent(e);
        }

        public override bool SetBlockLuminance(IntVec3 global, byte luminance)
        {
            if (!this.TryGetAll(global, out var chunk, out var cell))
                return false;

            if (cell.Luminance == luminance)
                return true;

            cell.Luminance = luminance;
            this.InvalidateCell(global);
            return true;
        }
      
        public override bool InvalidateCell(IntVec3 global)
        {
            if (!this.TryGetAll(global, out Chunk chunk, out Cell cell))
                return false;
            return chunk.InvalidateCell(cell);
        }
        public IEnumerable<(string, Action)> GetGenerationTasks()
        {
            var size = this.Size.Chunks;
            var max = size * size;
            var mutatorlist = this.World.Terraformers.ToList();
            mutatorlist.ForEach(m => m.SetWorld(this.World));
            var watch = new Stopwatch();
            var gradCache = new Dictionary<Chunk, Dictionary<IntVec3, double>>();
            yield return ("Initializing Chunks", () =>
            {
                watch.Start();
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        var pos = new Vector2(i, j);
                        var chunk = Chunk.Create(this, pos);
                        gradCache[chunk] = chunk.InitCells2(mutatorlist);// WARNING!
                        this.ActiveChunks.Add(pos, chunk);
                    }
                }
                watch.Stop();
                $"chunks initialized in {watch.ElapsedMilliseconds} ms".ToConsole();
            }
            );
            //var gradients = new Dictionary<IntVec3, double>();
            var gradients = gradCache.SelectMany(c => c.Value.Select(cc => (cc.Key.ToGlobal(c.Key), cc.Value))).ToDictionary(c => c.Item1, c => c.Value);

            foreach (var m in mutatorlist)
            {
                yield return ("Applying " + m.Def.Label, () =>
                {
                    watch.Restart();
                    foreach (var chunk in this.ActiveChunks.Values)
                    {
                        var cached = gradCache[chunk];
                        chunk.InitCells3(m, cached);
                        m.Finally(chunk, cached);
                    }
                    m.Generate(this);
                    watch.Stop();

                    $"{m} finished in {watch.ElapsedMilliseconds} ms".ToConsole();
                }
                );
            }

            foreach (var a in this.InitChunksNew())
                yield return a;

            foreach (var a in finishCreatingNew())
                yield return a;

            yield return  ("Detecting undiscovered areas", () => this.InitUndiscoveredAreas(null));

            IEnumerable<(string, Action)> finishCreatingNew()
            {
                Stopwatch watch;

                yield return ("Calculating light", () =>
                {
                    watch = Stopwatch.StartNew();
                    this.InitializeLight();
                    watch.Stop();
                    $"light updated in {watch.ElapsedMilliseconds} ms".ToConsole();
                }
                );
                yield return ("Generating plants", () =>
                {
                    watch = Stopwatch.StartNew();
                    TerraformerDefOf.Trees.Create().Generate(this); // wtf
                    watch.Stop();
                    $"plants generated in {watch.ElapsedMilliseconds} ms".ToConsole();
                }
                );
            }
        }
        [Obsolete]
        public override Task Generate(bool showDialog)
        {
            var loadingDialog = new DialogLoading();
            if (showDialog)
                loadingDialog.ShowDialog();
            return Task.Factory.StartNew(() =>
            {
                var tasks = new List<(string label, Action action)>();
                var size = this.Size.Chunks;
                var max = size * size;
                var mutatorlist = this.World.Terraformers.ToList();
                mutatorlist.ForEach(m => m.SetWorld(this.World));
                var watch = new Stopwatch();
                var gradCache = new Dictionary<Chunk, Dictionary<IntVec3, double>>();
                tasks.Add(("Initializing Chunks", () =>
                {
                    watch.Start();
                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            var pos = new Vector2(i, j);
                            var chunk = Chunk.Create(this, pos);
                            gradCache[chunk] = chunk.InitCells2(mutatorlist);// WARNING!
                            this.ActiveChunks.Add(pos, chunk);
                        }
                    }
                    watch.Stop();
                    $"chunks initialized in {watch.ElapsedMilliseconds} ms".ToConsole();
                }
                ));
                //var gradients = new Dictionary<IntVec3, double>();
                var gradients = gradCache.SelectMany(c => c.Value.Select(cc => (cc.Key.ToGlobal(c.Key), cc.Value))).ToDictionary(c => c.Item1, c => c.Value);

                foreach (var m in mutatorlist)
                {
                    tasks.Add(("Applying " + m.Def.Label, () =>
                    {
                        watch.Restart();
                        foreach (var chunk in this.ActiveChunks.Values)
                        {
                            var cached = gradCache[chunk];
                            chunk.InitCells3(m, cached);
                            m.Finally(chunk, cached);
                        }
                        m.Generate(this);
                        watch.Stop();

                        $"{m} finished in {watch.ElapsedMilliseconds} ms".ToConsole();
                    }
                    ));
                }

                foreach (var a in this.InitChunksNew())
                    tasks.Add(a);

                foreach (var a in finishCreatingNew())
                    tasks.Add(a);

                tasks.Add(("Detecting undiscovered areas", () => this.InitUndiscoveredAreas(null)));

                for (int i = 0; i < tasks.Count; i++)
                {
                    var (label, action) = tasks[i];
                    loadingDialog.Refresh(string.Format(label, i, tasks.Count), i / (float)tasks.Count);
                    action();
                }
                if (showDialog) 
                    loadingDialog.Close();
            });

            IEnumerable<(string, Action)> finishCreatingNew()
            {
                Stopwatch watch;

                yield return ("Calculating light", () =>
                {
                    watch = Stopwatch.StartNew();
                    this.InitializeLight();
                    watch.Stop();
                    $"light updated in {watch.ElapsedMilliseconds} ms".ToConsole();
                }
                );
                yield return ("Generating plants", () =>
                {
                    watch = Stopwatch.StartNew();
                    TerraformerDefOf.Trees.Create().Generate(this); // wtf
                    watch.Stop();
                    $"plants generated in {watch.ElapsedMilliseconds} ms".ToConsole();
                }
                );
            }
        }

        public override void DrawWorld(MySpriteBatch mySB, Camera camera)
        {
        }
        public override void DrawBeforeWorld(MySpriteBatch mySB, Camera camera)
        {
            this.Town.DrawBeforeWorld(mySB, this, camera);
        }

        public override bool IsInBounds(Vector3 global)
        {
            var maxz = this.GetMaxHeight();
            var maxside = this.Size.Chunks * Chunk.Size;
            return
                global.X >= 0 && global.X < maxside &&
                global.Y >= 0 && global.Y < maxside &&
                global.Z >= 0 && global.Z < maxz;
        }

        

        public override void UpdateLight(IEnumerable<IntVec3> positions)
        {
            this.LightingEngine.HandleImmediate(positions);
        }
        #region IMap implementation

        public override bool TryGetAll(int gx, int gy, int gz, out Chunk chunk, out Cell cell, out int lx, out int ly)
        {
            if (gz > MaxHeight - 1 || gz < 0)
            {
                lx = 0;
                ly = 0;
                chunk = null;
                cell = null;
                return false;
            }
            if (this.TryGetChunk(gx, gy, out chunk))
            {
                lx = gx - (int)chunk.Start.X;
                ly = gy - (int)chunk.Start.Y;
                cell = chunk[Chunk.GetCellIndex(lx, ly, gz)];
                return true;
            }
            lx = 0;
            ly = 0;
            chunk = null;
            cell = null;
            return false;
        }
        public override Vector2 GetOffset()
        {
            return this.Global;
        }
        public override string GetName()
        {
            return this.Name;
        }
        public override Dictionary<IntVec2, Chunk> GetActiveChunks()
        {
            return this.ActiveChunks;
        }
        public override void SetSkyLight(IntVec3 global, byte value)
        {
            var ch = this.GetChunk(global);
            var loc = global.ToLocal();
            ch.SetSunlight(loc, value);
            ch.InvalidateLight(global);
            foreach (var n in global.GetNeighbors())
            {
                if (this.TryGetChunk(n, out Chunk nchunk))
                {
                    nchunk.InvalidateLight(n);
                }
            }
            return;
        }
        public override void SetBlockLight(IntVec3 global, byte value)
        {
            var ch = this.GetChunk(global);
            if (ch is null)
            {
                return;
            }

            var loc = global.ToLocal();
            ch.SetBlockLight(loc, value);
            ch.InvalidateLight(global);
            foreach (var n in global.GetNeighbors())
            {
                if (this.TryGetChunk(n, out Chunk nchunk))
                {
                    nchunk.InvalidateLight(n);
                }
            }
            return;
        }

        readonly Queue<Dictionary<IntVec3, byte>> SkyLightChanges = new();
        public override void AddSkyLightChanges(Dictionary<IntVec3, byte> changes)
        {
            this.SkyLightChanges.Enqueue(changes);
            this.ApplyLightChanges();
        }

        readonly Queue<Dictionary<IntVec3, byte>> BlockLightChanges = new();
        public override void AddBlockLightChanges(Dictionary<IntVec3, byte> changes)
        {
            this.BlockLightChanges.Enqueue(changes);
            this.ApplyLightChanges();
        }
        public override void ApplyLightChanges()
        {
            while (this.SkyLightChanges.Any())
                foreach (var item in this.SkyLightChanges.Dequeue())
                    this.SetSkyLight(item.Key, item.Value);

            while (this.BlockLightChanges.Any())
                foreach (var item in this.BlockLightChanges.Dequeue())
                    this.SetBlockLight(item.Key, item.Value);
        }

        public override byte GetSkyDarkness()
        {
            return this.SkyDarkness;
        }
        public override byte GetBlockData(IntVec3 global)
        {
            return this.TryGetCell(global, out Cell cell) ? cell.BlockData : (byte)0;
        }
        public override byte SetBlockData(IntVec3 global, byte data = 0)
        {
            var cell = this.GetCell(global);
            var old = cell.BlockData;
            cell.BlockData = data;
            return old;
        }
        public override byte GetSunLight(IntVec3 global)
        {
            Chunk.TryGetSunlight(this, global, out byte sunlight);
            return sunlight;
        }

        public override List<GameObject> GetObjectsAtChunk(Vector3 global)
        {
            var chunks = this.GetChunks(global.GetChunkCoords(), 1);
            var entities = new List<GameObject>();
            foreach (var ch in chunks)
                entities.AddRange(ch.GetObjects());
            return entities;
        }
        public override int GetSizeInChunks()
        {
            return this.Size.Chunks;
        }

        public override int GetMaxHeight()
        {
            return MaxHeight;
        }

        static readonly Color ColorMidnight = new(21, 27, 84);
        static readonly Color ColorMango = new(255, 128, 64);
        static readonly Color ColorBronze = new(205, 127, 50);

        static readonly Dictionary<float, Color> AmbientColors = new() { { 0, Color.White }, { 0.5f, Color.Red }, { 1f, Color.Blue } };
        Color CachedAmbientColor;
        /// <summary>
        /// TODO: move ambient color to biome class
        /// </summary>
        /// <returns></returns>
        public override Color GetAmbientColor()
        {
            return this.CachedAmbientColor;
        }
        private Color UpdateAmbientColor()
        {
            var nightAmount = 1 - this.Sunlight;// (float)this.GetDayTimeNormal();
            for (int i = 0; i < AmbientColors.Count - 2; i++)
            {
                var a = AmbientColors.ElementAt(i);
                var b = AmbientColors.ElementAt(i + 1);
                var c = AmbientColors.ElementAt(i + 2);

                if (a.Key <= nightAmount && nightAmount < c.Key)
                {
                    var t = (nightAmount - a.Key) / (c.Key - a.Key);
                    var ab = Color.Lerp(a.Value, b.Value, t);
                    var bc = Color.Lerp(b.Value, c.Value, t);
                    return Color.Lerp(ab, bc, t);
                }
                else if (nightAmount == c.Key)
                    return c.Value;
            }

            return Color.Lime;
        }
        public override void SetAmbientColor(Color color)
        {
            this.AmbientColor = color;
        }
        public override MapThumb GetThumb()
        {
            return this.Thumb;
        }
        public override double GetDayTimeNormal()
        {
            //double normal = (this.Clock.TotalMinutes - Ticks.PerSecond * (Zenith - 12)) / 1440f;
            double normal = (this.Clock.TotalMinutes - Ticks.PerGameMinute * (Zenith - 12)) / 1440f;
            double nn = normal * 2 * Math.PI;
            nn = 3 * Math.Cos(nn);
            return Math.Max(0, Math.Min(1, (1 + nn) / 2f));
        }
        #endregion

     

        /// <summary>
        /// Called after terrain has been generated. Detects cells at the edges of 'caves' and enqueues them to receive light from their adjacent cells that are open to sunlight 
        /// </summary>
        private void InitializeLight()
        {
            var queued = new HashSet<IntVec3>();
            for (int i = 0; i < Size.Blocks; i++)
            {
                for (int j = 0; j < Size.Blocks; j++)
                {
                    var h = this.GetHeightmapValue(i, j);
                    for (int k = 0; k < h; k++)
                    {
                        var pos = new IntVec3(i, j, k);
                        if (pos.GetAdjacentHorLazy().Any(n => this.GetCell(n) is Cell cell && this.GetHeightmapValue(n.X, n.Y) < k && !queued.Contains(n)))
                            queued.Add(pos);
                    }
                }
            }
            this.UpdateLight(queued);
        }

        internal void FinishLoading()
        {
            this.CacheObjects();
        }
        readonly UndiscoveredAreaManager UndiscoveredAreaManager;
        internal void InitUndiscoveredAreas(Action<string, float> callback = null)
        {
            callback?.Invoke("Detecing undiscovered areas", 0);
            this.UndiscoveredAreaManager.Init(); // TODO: send undiscovered areas to clients instead of them initializing them themselves?
        }
        internal override bool IsUndiscovered(Vector3 global)
        {
            if (!this.UndiscoveredAreaManager.IsUndiscovered(global))
                return false;

            foreach (var n in global.GetAdjacentLazy())
                if (this.IsAir(n) && !this.UndiscoveredAreaManager.IsUndiscovered(n))
                    return false;
            return true;
        }
        internal override void AreaDiscovered(HashSet<Vector3> hashSet)
        {
            foreach (var global in hashSet)
            {
                var chunk = this.GetChunk(global);
                chunk.InvalidateMesh();
            }
            this.Net.Write("Area discovered!");
        }
        internal override void CameraRecenter()
        {
            var x = this.Size.Blocks / 2;
            var y = x;
            var z = this.GetHeightmapValue(x, y);
            this.Camera.CenterOn(new Vector3(x, y, z), true);
        }

        internal void AddStartingActors(Actor[] actors)
        {
            var x = this.Size.Blocks / 2;
            var y = x;
            var z = this.GetHeightmapValue(x, y);
            var center = new IntVec3(x, y, z);
            var radial = VectorHelper.GetRadialLarge(center).GetEnumerator();
            for (int i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                IntVec3 current;
                do
                {
                    radial.MoveNext();
                    current = radial.Current;
                } while (!this.IsStandableIn(current));
                actor.Global = current;
                //actor.Spawn(this, current);
                this.Add(actor);
                actor.OnSpawnNew();
            }
        }
    }
}
