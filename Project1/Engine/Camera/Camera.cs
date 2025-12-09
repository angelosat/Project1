using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Blocks;
using Start_a_Town_.Components;
using Start_a_Town_.Graphics;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Start_a_Town_
{
    public class Camera : IKeyEventHandler
    {
        static XElement XCameraSettings = GameSettings.XmlNodeSettings.GetOrCreateElement("Camera");
        static Camera()
        {
            SmoothCentering = (bool?)XCameraSettings.Element(nameof(SmoothCentering)) ?? true;
        }
        public const int FogZOffset = 2, FogFadeLength = 8;
        Vector4 FogColor = Color.SteelBlue.ToVector4();
        GameObject Following;
        Vector2 _Coordinates;
        bool _hideUnknownBlocks = true;
        public bool HideUnknownBlocks // TODO: make it static
        {
            get => this._hideUnknownBlocks;
            set
            {
                this._hideUnknownBlocks = value;
                Ingame.CurrentMap.InvalidateChunks();
                this.TopSliceChanged = true;
            }
        }
        bool _drawTopSlice = true;
        public bool DrawTopSlice
        {
            get => this._drawTopSlice;
            set
            {
                this._drawTopSlice = value;
                Ingame.CurrentMap.InvalidateChunks();
            }
        }
        public bool DrawZones = true;
        public static bool HideCeiling;
        public static bool SmoothCentering;
        public Vector2 Location;
        public bool HideTerrainAbovePlayer;
        public int HideTerrainAbovePlayerOffset;
        public float ZoomMax = 8;// 16;
        public float ZoomMin = 0.125f;
        public int Width, Height;
        public Vector3? Center = Vector3.Zero;
        int _DrawLevel = MapBase.MaxHeight - 1;
        public int DrawLevel
        {
            get => this._DrawLevel;
            set
            {
                var oldvalue = this._DrawLevel;
                this._DrawLevel = value;
                if (oldvalue != value)
                {
                    this.TopSliceChanged = true;
                }

                if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
                {
                    this.Move(this.Coordinates - new Vector2(0, Block.BlockHeight * (value - oldvalue)));
                }
            }
        }
        public float ZoomNext;
        public float Zoom = 2;//1;
        public static Rectangle CellIntersectBox = new Rectangle();
        public static bool BlockTargeting = true;
        const float InitialZoom = 2;
        public static bool Fog = true;
        public bool HideUnderground;
        public bool BorderShading;
        public float FogLevel = 0;
        public int MaxDrawZ;
        Vector2 CameraOffset = Vector2.Zero;
        public int RenderIndex = 0;
        public RenderTarget2D MapRender,
          WaterRender, WaterDepth, WaterLight, WaterFog,
          WaterComposite,
          MapDepth, MapLight, TextureFogWater, MapComposite,
          RenderBeforeFog, LightBeforeFog, DepthBeforeFog, FogBeforeFog,
          FinalScene;
        public Rectangle ViewPort;
        double _rotation;
        public double RotCos, RotSin;
        public float LastZTarget;
        float DepthFar, DepthNear;
        public MySpriteBatch SpriteBatch;
        public MySpriteBatch WaterSpriteBatch, // waterspritebatch is not used!?
            ParticlesSpriteBatch, BlockParticlesSpriteBatch, TransparentBlocksSpriteBatch;
        float FogT = 0;
        public Effect Effect;
        public static bool DrawnOnce = false;
        public RenderTarget2D[] RenderTargets = new RenderTarget2D[5];
        public double Rotation
        {
            get => this._rotation;
            set
            {
                double oldRot = this._rotation;
                this._rotation = value % 4;

                if (this._rotation < 0)
                    this._rotation = 4 + value;

                this.RotationReverse = -(int)this._rotation;
                if (this.RotationReverse < 0)
                    this.RotationReverse += 4;

                this.RotCos = Math.Cos((Math.PI / 2f) * this._rotation);
                this.RotSin = Math.Sin((Math.PI / 2f) * this._rotation);

                this.RotCos = Math.Round(this.RotCos + this.RotCos) / 2f;
                this.RotSin = Math.Round(this.RotSin + this.RotSin) / 2f;


                if (this._rotation != oldRot)
                    this.OnRotationChanged();
            }
        }
        public int RotationReverse { get; private set; }
        public Vector2 Coordinates
        {
            get => this._Coordinates;
            set
            {
                this._Coordinates = value;
                this.Location = this.Coordinates - new Vector2((int)((this.Width / 2) / this.Zoom), (int)((this.Height / 2) / this.Zoom));
            }
        }
        public bool TopSliceChanged = true;

        public Camera()
            : this(Game1.Bounds.Width, Game1.Bounds.Height)
            //: this(Game1.Instance.graphics.PreferredBackBufferWidth, Game1.Instance.graphics.PreferredBackBufferHeight)
        {
            this.WaterSpriteBatch = new MySpriteBatch(Game1.Instance.GraphicsDevice);
            this.SpriteBatch = new MySpriteBatch(Game1.Instance.GraphicsDevice);
        }
        public Camera(int width, int height, float x = 0, float y = 0, float z = 0, float zoom = 2, int rotation = 0)
        {
            this.Width = width;
            this.Height = height;
            this.ViewPort = new Rectangle(0, 0, this.Width, this.Height);
            this.Zoom = zoom;
            this.ZoomNext = zoom;
            this.Rotation = rotation;
            this.CenterOn(new Vector3(x, y, z));
            Game1.Instance.graphics.DeviceReset += this.gfx_DeviceReset;
            this.OnDeviceLost();
        }

        public override string ToString()
        {
            string text = this.Location.ToString();
            text += "\nZoom: " + this.Zoom +
                "\nRotation: " + this.Rotation;
            return text;
        }

        protected void OnRotationChanged()
        {
            Ingame.CurrentMap.OnCameraRotated(this);
            SelectionManager.OnCameraRotated(this);
        }

        void gfx_DeviceReset(object sender, EventArgs e)
        {
            this.Width = Game1.Bounds.Width;
            this.Height = Game1.Bounds.Height;
            this.ViewPort = new Rectangle(0, 0, this.Width, this.Height);
            this.OnDeviceLost();
        }

        public void Update(MapBase map)
        {
            this.Follow();
            this.SmoothZoom(this.ZoomNext);
            this.UpdateFog(map);
        }

        void UpdateFog(MapBase map)
        {
            this.FogT = (this.FogT + 0.05f * map.Net.Speed) % 100;
        }
        public void Move(Vector2 coords)
        {
            this.Center = null;
            this.Following = null;
            this.Coordinates = coords;
        }

        void SetZoom(float value)
        {
            this.Zoom = value;
            var offset = new Vector2(this.Width / 2, this.Height / 2);
            offset /= this.Zoom;
            this.Location = this.Coordinates - offset;
        }
        public void SmoothZoom(float next)
        {
            float diff = next - this.Zoom;
            var zoomSpeed = 0.1f;
            var n = zoomSpeed * diff;
            if (Math.Abs(n) < 0.001f)
                this.SetZoom(next);
            else
                this.SetZoom(this.Zoom + n);
        }

        static int PreviousDrawLevel = -1;
        internal bool HideWalls;

        public void SliceOn(int next)
        {
            var current = this.DrawLevel;
            if (next != current)
            {
                PreviousDrawLevel = current;
                this.DrawLevel = next;
            }
            else if (PreviousDrawLevel != -1)
                this.DrawLevel = PreviousDrawLevel;
        }
        public void CenterOn(Vector3 global, bool forceSnap = false)
        {
            this.Center = global;
            this.DrawLevel = (int)Math.Max(this.DrawLevel, global.Z + 1);
            if (!SmoothCentering || forceSnap)
            {
                Coords.Iso(this, global.X, global.Y, global.Z, out int xx, out int yy);
                this.Coordinates = new Vector2(xx, yy);
            }
        }
        public void Follow()
        {
            if (this.Following is null)
            {
                if(this.Center.HasValue)
                    this.Follow(this.Center.Value);
                return;
            }
            if (this.Following.IsIndoors())
                this.DrawLevel = (int)(this.Following.Global.CeilingZ().Z + this.Following.Physics.Height - 1);
            else
                this.DrawLevel = this.Following.Map.GetMaxHeight();
            this.Follow(this.Following.Global);
        }
        public void Follow(Vector3 global)
        {
            this.Center = global;
            Coords.Iso(this, global.X, global.Y, global.Z, out float xx, out float yy);

            Vector2
                currentLoc = this.Coordinates,
                nextLoc = new(xx, yy),
                diff = nextLoc - currentLoc;

            diff *= 100;
            diff = diff.ToRounded();
            diff /= 100;

            var nextCoords = currentLoc + 0.05f * diff;

            // TODO: find a way to make it smooth without seaming between sprites

            /// uncomment this to make camera movement rigid instead of smooth
            nextCoords = nextCoords.ToRounded(); // must round to prevent seaming between blocks when moving camera
            ///

            this.Coordinates = nextCoords;
        }

        public void GetEverything(MapBase map, Vector3 global, Rectangle spriteRect, out float depth, out Rectangle screenBounds, out Vector2 screenLoc)
        {
            depth = global.GetDrawDepth(map, this);
            screenBounds = this.GetScreenBounds(global, spriteRect);
            screenLoc = new Vector2(screenBounds.X, screenBounds.Y);
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle)
        {
            return this.GetScreenBounds(x, y, z, spriteRectangle, 0, 0);
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy)
        {
            Coords.Iso(this, x, y, z, out int xx, out int yy);
            return new Rectangle(
                (int)(this.Zoom * (xx + spriteRectangle.X - this.Location.X - originx)),
                (int)(this.Zoom * (yy + spriteRectangle.Y - this.Location.Y - originy)),
                (int)(this.Zoom * spriteRectangle.Width),
                (int)(this.Zoom * spriteRectangle.Height));
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy, float scale)
        {
            Coords.Iso(this, x, y, z, out int xx, out int yy);
            var scalezoom = scale * this.Zoom;
            return new Rectangle(
                (int)(this.Zoom * (xx + scale * spriteRectangle.X - this.Location.X - originx)),
                (int)(this.Zoom * (yy + scale * spriteRectangle.Y - this.Location.Y - originy)),
                (int)(scalezoom * spriteRectangle.Width),
                (int)(scalezoom * spriteRectangle.Height));
        }
        public Vector4 GetScreenBoundsVector4(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin, float scale = 1)
        {
            Coords.Iso(this, x, y, z, out float xx, out float yy);
            var loc = this.Location;
            float xxx = (float)xx + scale * spriteRectangle.X - loc.X - origin.X;
            float yyy = (float)yy + scale * spriteRectangle.Y - loc.Y - origin.Y;
            float w = scale * spriteRectangle.Width;
            float h = scale * spriteRectangle.Height;
            var vector = new Vector4(xxx, yyy, w, h);
            vector *= this.Zoom;
            return vector;
        }
        public Vector4 GetScreenBoundsVector4NoOffset(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin)
        {
            Coords.Iso(this, x, y, z, out float xx, out float yy);
            float xxx = (float)(xx + spriteRectangle.X - origin.X);
            float yyy = (float)(yy + spriteRectangle.Y - origin.Y);
            float w = spriteRectangle.Width;
            float h = spriteRectangle.Height;
            var vector = new Vector4(xxx, yyy, w, h);
            return vector;
        }
        public Vector2 GetScreenPosition(TargetArgs t)
        {
            var fx = t.Face.X * .5f;
            var fy = t.Face.Y * .5f;
            var yx = fx + fy;
            var fz = yx == 0 ? (t.Face.Z == 1 ? 1 : 0) : .5f;
            return this.GetScreenPosition(t.Global + new Vector3(fx, fy, fz));
        }
        public Vector2 GetScreenPosition(Vector3 pos)
        {
            Coords.Iso(this, pos.X, pos.Y, pos.Z, out int xx, out int yy);
            return new Vector2(this.Zoom * (xx - this.Location.X), this.Zoom * (yy - this.Location.Y));
        }
        public Vector2 GetScreenPositionFloat(Vector3 pos)
        {
            Coords.Iso(this, pos.X, pos.Y, pos.Z, out float xx, out float yy);
            var loc = this.Location;
            var screenpos = new Vector2(this.Zoom * (xx - loc.X), this.Zoom * (yy - loc.Y));
            return screenpos;
        }

        public Rectangle GetScreenBounds(Vector3 global, Rectangle spriteRectangle)
        {
            return this.GetScreenBounds(global.X, global.Y, global.Z, spriteRectangle);
        }

        internal float GetDrawDepth(GameObject o)
        {
            return o.Global.GetDrawDepth(o.Map, this);
        }
        internal float GetDrawDepth(MapBase map, Vector3 global)
        {
            return global.GetDrawDepth(map, this);
        }
        internal int GetDrawDepthSimple(IntVec3 global)
        {
            Coords.Rotate(this, global.X, global.Y, out int rx, out int ry);
            return rx + ry + global.Z;
        }
        public bool CullingCheck(float x, float y, float z, Rectangle sourceBounds, out Rectangle screenBounds)
        {
            screenBounds = this.GetScreenBounds(x, y, z, sourceBounds);
            return this.ViewPort.Intersects(screenBounds);
        }

        /// <summary>
        /// TODO: make rotation a field for speed and calculate the shits in some other way
        /// </summary>
        public void RotateClockwise()
        {
            this.Rotation++;
        }
        public void RotateCounterClockwise()
        {
            this.Rotation--;
        }
        public void RotationReset()
        {
            this.Rotation = 0;
        }
        public bool DrawCell(Canvas canvas, MapBase map, Chunk chunk, Cell cell)
        {
            int z = cell.Z;
            var cellTile = cell.Block;
            if (cellTile is BlockAir)
            {
                throw new Exception(); /// drawcell should never be called for air blocks, there are problems elsewhere
                //chunk.InvalidateCell(cell);
                //("tried to draw air at " + cell.GetGlobalCoords(chunk).ToString()).ToConsole();
                //return false;
            }

            var block = cell.Block;

            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;
            var light = GetFinalLight(this, map, chunk, cell, gx, gy, z);

            var screenBoundsVector4 = this.GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            Coords.Rotate(this, lx, ly, out int rlx, out int rly);
            var depth = rlx + rly;

            var finalFogColor = Color.Transparent; // i calculate fog inside the shader from now on
            var global = new IntVec3(gx, gy, z);
            var isDiscovered = !map.IsUndiscovered(global);
            /// DONT ERASE
            ///if (cell.AllEdges == 0 && HideUnknownBlocks)  // do i want cells that have already been discoverd, to remain visible even if they become obstructed again?
            if (!isDiscovered && this.HideUnknownBlocks)// && isAir) // do i want cells that have already been discoverd, to remain visible even if they become obstructed again?
                Block.DrawUnknown(canvas.Opaque, new IntVec3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth);
            else
                block.Draw(canvas, chunk, new IntVec3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth, cell);//.Variation, cell.Orientation, cell.BlockData, cell.Material);

            return true;
        }
        static readonly Color CellSelectionTint = Color.White * .5f;
        public void DrawBlockSelectionGlobal(MySpriteBatch sb, IntVec3 global)
        {
            this.DrawBlockSelectionGlobal(sb, Block.BlockHighlight, global);
        }
        public void DrawBlockSelectionGlobal(MySpriteBatch sb, AtlasDepthNormals.Node.Token texToken, IntVec3 global)
        {
            this.DrawBlockSelectionGlobal(sb, global, texToken, CellSelectionTint);
        }
        public void DrawBlockSelectionGlobal(MySpriteBatch sb, IntVec3 global, AtlasDepthNormals.Node.Token texToken, Color tint)
        {
            int z = global.Z;
            int gx = global.X;
            int gy = global.Y;

            var screenBoundsVector4 = this.GetScreenBoundsVector4NoOffset(gx, gy, z, Block.Bounds, Vector2.Zero);
            Coords.Rotate(this, gx, gy, out int rlx, out int rly);
            var depth = rlx + rly;

            sb.DrawBlock(Block.Atlas.Texture, screenBoundsVector4,
                texToken,
                this.Zoom, Color.Transparent, tint, Color.White, Color.White, Vector4.One, Vector4.Zero, depth, null, global);
        }
        public bool DrawUnknown(Canvas canvas, MapBase map, Chunk chunk, Cell cell)
        {
            int z = cell.Z;
            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;
            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out int rgx, out int rgy);
            var light = GetFinalLight(this, map, chunk, cell, gx, gy, z, false);

            var screenBoundsVector4 = this.GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            Coords.Rotate(this, lx, ly, out int rlx, out int rly);
            var depth = rlx + rly;

            Block.DrawUnknown(canvas.Opaque, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, Color.Transparent, Color.White, depth);

            return true;
        }
        public bool DrawUnknown(MySpriteBatch sb, MapBase map, Chunk chunk, Cell cell)
        {
            int z = cell.Z;
            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;

            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out int rgx, out int rgy);
            var light = GetFinalLight(this, map, chunk, cell, gx, gy, z, false);

            var screenBoundsVector4 = this.GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            Coords.Rotate(this, lx, ly, out int rlx, out int rly);
            var depth = rlx + rly;

            Block.DrawUnknown(sb, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, Color.Transparent, Color.White, depth);

            return true;
        }

        public Color GetFogColorNew(int z)
        {
            if (!Fog)
                return Color.Transparent;

            if (this.LastZTarget > 1)
            {
                if (z < this.LastZTarget - FogZOffset)
                {
                    var d = Math.Abs(z - this.LastZTarget + FogZOffset);
                    d = MathHelper.Clamp(d, 0, FogFadeLength) / FogFadeLength;
                    var fog = Color.Lerp(Color.White, Color.DarkSlateBlue, d);
                    var val = (byte)(d * 255);
                    var finalFogColor = new Color(fog.R, fog.G, fog.B, val);
                    return finalFogColor;
                }
            }

            return Color.Transparent;
        }

        internal void DrawChunk(MySpriteBatch sb, MapBase map, Chunk chunk, Vector3? playerGlobal, List<Rectangle> hiddenRects, EngineArgs a)
        {
            throw new Exception();
        }

        public void PrepareShader(MapBase map)
        {
            var view =
                new Matrix(
                  1.0f, 0.0f, 0.0f, 0.0f,
                  0.0f, -1.0f, 0.0f, 0.0f,
                  0.0f, 0.0f, 1.0f, 0.0f,
                  0.0f, 0.0f, 0.0f, 1.0f);
            float camerax = this.Coordinates.X;
            float cameray = this.Coordinates.Y;
            view = view * Matrix.CreateTranslation(new Vector3(-camerax, cameray, 0)) * Matrix.CreateScale(this.Zoom) * Matrix.CreateTranslation(new Vector3(this.Width / 2, -this.Height / 2, 0));
            var near = -this.GetFarDepth(map) * this.Zoom;
            var far = -this.GetNearDepth(map) * this.Zoom;
            var projection = Matrix.CreateOrthographicOffCenter(
                0, this.Width, -this.Height, 0, near, far);
            this.Effect.CurrentTechnique = this.Effect.Techniques["Chunks"];
            this.Effect.Parameters["View"].SetValue(view);
            this.Effect.Parameters["Projection"].SetValue(projection);

            this.DepthNear = this.GetNearDepth(map);
            this.DepthFar = this.GetFarDepth(map);
            this.Effect.Parameters["FarDepth"].SetValue(this.DepthFar);
            this.Effect.Parameters["NearDepth"].SetValue(this.DepthNear);
            this.Effect.Parameters["DepthResolution"].SetValue((2) / (this.DepthNear - this.DepthFar));
            this.Effect.Parameters["OutlineThreshold"].SetValue((1) / (this.DepthNear - this.DepthFar));
        }

        public void PrepareShaderTransparent(MapBase map)
        {
            var view =
                new Matrix(
                   1.0f, 0.0f, 0.0f, 0.0f,
                   0.0f, -1.0f, 0.0f, 0.0f,
                   0.0f, 0.0f, 1.0f, 0.0f,
                   0.0f, 0.0f, 0.0f, 1.0f);
            float camerax = this.Coordinates.X;
            float cameray = this.Coordinates.Y;
            view = view * Matrix.CreateTranslation(new Vector3(-camerax, cameray, 0)) * Matrix.CreateScale(this.Zoom) * Matrix.CreateTranslation(new Vector3(this.Width / 2, -this.Height / 2, 0));
            var near = -this.GetFarDepth(map) * this.Zoom;
            var far = -this.GetNearDepth(map) * this.Zoom;
            var projection = Matrix.CreateOrthographicOffCenter(
                0, this.Width, -this.Height, 0, near, far);
            this.Effect.CurrentTechnique = this.Effect.Techniques["CombinedWater"];
            this.Effect.Parameters["View"].SetValue(view);
            this.Effect.Parameters["Projection"].SetValue(projection);

            this.DepthNear = this.GetNearDepth(map);
            this.DepthFar = this.GetFarDepth(map);
            this.Effect.Parameters["FarDepth"].SetValue(this.DepthFar);
            this.Effect.Parameters["NearDepth"].SetValue(this.DepthNear);
            this.Effect.Parameters["DepthResolution"].SetValue((2) / (this.DepthNear - this.DepthFar));
            this.Effect.Parameters["OutlineThreshold"].SetValue((1) / (this.DepthNear - this.DepthFar));

        }

        public static LightToken GetFinalLight(Camera camera, MapBase map, Chunk chunk, Cell cell, int gx, int gy, int z, bool updateblockfaces = true)
        {
            // UNCOMMENT THIS?
            //if (chunk.LightCache.TryGetValue(new Vector3(gx, gy, z), out color))
            //    return color;
            //if (cell.Light != null)
            //    return cell.Light;
            var global = new Vector3(gx, gy, z);

            if (chunk.LightCache.TryGetValue(global, out LightToken cached))
                return cached;

            // update block exposed faces too here?
            // TESTING IF REMOVING THIS BREAKS ANYTHING
            //if (updateblockfaces)
            //    chunk.UpdateBlockFaces(cell); // COMMENT if i want to see visible horizontal slices of the map

            Coords.Rotate(camera, 1, 0, out int rightx, out int righty);
            Coords.Rotate(camera, 0, 1, out int leftx, out int lefty);

            Chunk.TryGetFinalLight(map, gx + rightx, gy - righty, z, out byte suneast, out byte blockeast);
            Chunk.TryGetFinalLight(map, gx - leftx, gy + lefty, z, out byte sunsouth, out byte blocksouth);
            Chunk.TryGetFinalLight(map, gx, gy, z, out byte sunCenter, out byte blockCenter);

            byte suntop, blocktop;
            if (z + 1 < MapBase.MaxHeight)
            {
                suntop = Math.Max((byte)0, chunk.GetSunlight(cell.X, cell.Y, z + 1));
                blocktop = chunk.GetBlockLight(cell.X, cell.Y, z + 1);
            }
            else
            {
                suntop = 15;
                blocktop = 15;
            }
            // add the current cell's light as the 4th coord?
            Color sun = new((suneast + 1) / 16f, (sunsouth + 1) / 16f, (suntop + 1) / 16f, (sunCenter + 1)/ 16f);
            Vector4 block = new((blockeast + 1) / 16f, (blocksouth + 1) / 16f, (blocktop + 1) / 16f, (blockCenter + 1) / 16f);// 1f);

            var light = new LightToken(global, sun, block);
            chunk.LightCache[global] = light;
            return light;
        }


        public void DrawMap(MapBase map, ToolManager toolManager, UIManager ui, SceneState scene)
        {
            var gd = Game1.Instance.GraphicsDevice;
            if (map is null)
                return;

            //if (this.RenderTargetsInvalid)
            //{
            //    this.OnDeviceLost();
            //    this.RenderTargetsInvalid = false;
            //}

            this.RenderTargets[0] = this.MapRender;
            this.RenderTargets[1] = this.MapDepth;
            this.RenderTargets[2] = this.MapLight;
            this.RenderTargets[3] = this.TextureFogWater;

            gd.SetRenderTargets(this.MapRender, this.MapDepth, this.MapLight, this.TextureFogWater);

            var a = EngineArgs.Default;

            gd.SetRenderTargets(null);
            gd.Clear(Color.Transparent);
            gd.RasterizerState = RasterizerState.CullNone;
            this.NewDraw(map, gd, a, scene, toolManager, ui);
        }
        Effect Shader => Game1.Instance.Content.Load<Effect>("blur");
        EffectTechnique TechniqueBlockHighlight => this.Shader.Techniques["BlockHighlight"];

        private void NewDraw(MapBase map, GraphicsDevice gd, EngineArgs a, SceneState scene, ToolManager toolManager, UIManager ui)
        {
            DrawnOnce = true;
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            this.Effect = fx;
            var xx = this.CameraOffset.X / this.Width;
            var yy = this.CameraOffset.Y / this.Height;

            var world = Matrix.Identity;
            var view =
                new Matrix(
                   1.0f, 0.0f, 0.0f, 0.0f,
                   0.0f, -1.0f, 0.0f, 0.0f,
                   0.0f, 0.0f, -1.0f, 0.0f,
                   0.0f, 0.0f, 0.0f, 1.0f);
            var projection = Matrix.CreateOrthographicOffCenter(
                0, this.Width, -this.Height, 0, 0, 1);

            fx.Parameters["World"].SetValue(Matrix.Identity);

            fx.Parameters["BlockWidth"].SetValue(Block.Width);
            fx.Parameters["BlockHeight"].SetValue(Block.Height);
            fx.Parameters["AtlasWidth"].SetValue(Block.Atlas.Texture.Width);
            fx.Parameters["AtlasHeight"].SetValue(Block.Atlas.Texture.Height);
            fx.Parameters["Viewport"].SetValue(new Vector2(gd.Viewport.Width, gd.Viewport.Height));
            fx.Parameters["ViewportW"].SetValue(new Vector2(1, gd.Viewport.Width / (float)gd.Viewport.Height));
            fx.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);
            fx.Parameters["Zoom"].SetValue(this.Zoom);
            float borderPx = 1;
            fx.Parameters["BorderResolution"].SetValue(new Vector2(borderPx / gd.Viewport.Width, borderPx / gd.Viewport.Height) * this.Zoom);
            var nightAmount = (float)map.GetDayTimeNormal();
            Color ambientColor = Color.Lerp(Color.White, map.GetAmbientColor(), nightAmount);
            ambientColor = map.GetAmbientColor();

            Vector4 ambient = ambientColor.ToVector4();
            fx.Parameters["AmbientLight"].SetValue(ambient);
            var fogColor = this.FogColor;
            // choose between ambient or black background color
            fogColor = Color.Lerp(new Color(fogColor), Color.Black, nightAmount).ToVector4();
            fx.Parameters["FogColor"].SetValue(fogColor);
            fx.Parameters["FogDistance"].SetValue(FogFadeLength);

            var fogoffset = new Vector2(this.FogT / 100f, 0);
            fx.Parameters["FogOffset"].SetValue(fogoffset - this.Coordinates / 1000f);
            if (toolManager.ActiveTool.Target != null && toolManager.ActiveTool.Target.Type != TargetType.Null)
            {
                this.LastZTarget = toolManager.ActiveTool.Target.Global.Z;
                fx.Parameters["FogZ"].SetValue(toolManager.ActiveTool.Target.Global.Z - FogZOffset);
            }
            fx.Parameters["FogEnabled"].SetValue(Fog);

            fx.Parameters["PlayerOcclusion"].SetValue(this.Following != null);
            fx.Parameters["PlayerGlobal"].SetValue(this.Following != null ? this.Following.Global : Vector3.Zero);
            if (this.Following != null)
            {
                fx.Parameters["PlayerRotXY"].SetValue((float)(
                    this.Following.Global.X * this.RotCos - this.Following.Global.Y * this.RotSin +
                    this.Following.Global.X * this.RotSin + this.Following.Global.Y * this.RotCos));
                fx.Parameters["PlayerCenterOffset"].SetValue(this.GetScreenPositionFloat(this.Following.Global + this.Following.Physics.Height * Vector3.UnitZ / 2) / new Vector2(this.ViewPort.Width, this.ViewPort.Height) - Vector2.One * .5f);
            }

            fx.Parameters["FogLevel"].SetValue(this.FogLevel);
            this.MaxDrawZ = this.GetMaxDrawLevel(map);
            this.Effect.Parameters["FogEnabled"].SetValue(Fog);
            this.Effect.Parameters["MaxDrawLevel"].SetValue(this.MaxDrawZ);
            this.Effect.Parameters["HideWalls"].SetValue(Engine.HideWalls);
            this.Effect.Parameters["OcclusionRadius"].SetValue(.01f * this.Zoom * this.Zoom);

            gd.DepthStencilState = DepthStencilState.Default;

            gd.SamplerStates[0] = SamplerState.PointClamp;
            gd.SamplerStates[1] = SamplerState.PointClamp;
            gd.SamplerStates[2] = SamplerState.PointClamp;
            gd.SamplerStates[3] = SamplerState.PointClamp;

            if (this.SpriteBatch == null)
                this.SpriteBatch = new MySpriteBatch(gd);

            if (this.WaterSpriteBatch == null)
                this.WaterSpriteBatch = new MySpriteBatch(gd);

            if (this.ParticlesSpriteBatch == null)
                this.ParticlesSpriteBatch = new MySpriteBatch(gd);

            if (this.TransparentBlocksSpriteBatch == null)
                this.TransparentBlocksSpriteBatch = new MySpriteBatch(gd);

            if (this.BlockParticlesSpriteBatch == null)
                this.BlockParticlesSpriteBatch = new MySpriteBatch(gd);

            var clearcol = new Color(1f, 1f, 1f, 0); // if i put 1 for the alpha than tsansparent blocks will be shaded white  // (old comment) i put 1 again because i dont draw water on the fog texture after all
            //var clearcol = new Color(1f, 1f, 1f, 1f); // causes unhandled white background

            gd.SetRenderTargets(this.TextureFogWater);
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1, 1);
            // PROBLEM clearing the texturefogwater with the same parameters as the other rendertargets, causes the problem with the background being drawn over the toolmanager preview blocks
            gd.SetRenderTargets(this.MapRender, this.MapLight, this.MapDepth);
            //var clearcol = new Color(1f, 1f, 0, 0); // 3rd component is 0 in order to not draw water on background
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearcol, 1, 1);

            // after clearing each target with the appropriate parameters for each one, set them all together
            gd.SetRenderTargets(this.MapRender, this.MapLight, this.MapDepth, this.TextureFogWater);

            // use new technique to draw both color and light in one pass in multiple rendertargets
            fx.CurrentTechnique = fx.Techniques["Combined"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SetBlockTextures(fx);

            this.DepthNear = float.MinValue;
            this.DepthFar = float.MaxValue;

            this.Effect.Parameters["RotCos"].SetValue((float)this.RotCos);
            this.Effect.Parameters["RotSin"].SetValue((float)this.RotSin);

            var actor = map.Net.GetPlayer()?.ControllingEntity;
            if (actor != null)
            {
                if (actor.Exists)
                {
                    Sprite sprite = actor.GetSprite();
                    Rectangle spriteBounds = sprite.GetBounds();
                    Rectangle screenBounds = this.GetScreenBounds(actor.Global, spriteBounds);
                    var xxx = screenBounds.X / (float)this.Width - .5f;
                    var yyy = screenBounds.Y / (float)this.Height - .5f;
                    var www = (screenBounds.X + screenBounds.Width) / (float)this.Width - .5f;
                    var hhh = (screenBounds.Y + screenBounds.Height) / (float)this.Height - .5f;
                    xxx = -.1f * this.Zoom;
                    yyy = -.15f * this.Zoom;
                    www = .1f * this.Zoom;
                    hhh = .15f * this.Zoom;
                    var box = new Vector4(xxx, yyy, www, hhh);
                    this.Effect.Parameters["PlayerBoundingBox"].SetValue(box);
                    this.Effect.Parameters["PlayerDepth"].SetValue(actor.Global.GetDrawDepth(map, this));
                }
            }

            this.PrepareShader(map);

            var visibleChunks = (from ch in map.GetActiveChunks().Values where this.ViewPort.Intersects(ch.GetScreenBounds(this)) select ch);

            foreach (var chunk in visibleChunks)
            {
                // TODO: DONT BUILD TOP SLICE TWICE!
                if (!chunk.Valid)
                    chunk.Build(this);

                chunk.DrawOpaqueLayers(this, this.Effect); // TODO: is it faster to pass only the effectparameters?
                continue;
            }
            this.TopSliceChanged = false;

            // TODO: these temporarily only work with static maps
            this.DepthNear = this.GetNearDepth(map);
            this.DepthFar = this.GetFarDepth(map);

            fx.Parameters["FarDepth"].SetValue(this.DepthFar);
            fx.Parameters["NearDepth"].SetValue(this.DepthNear);

            fx.Parameters["DepthResolution"].SetValue(2 / (this.DepthNear - this.DepthFar));
            fx.Parameters["OutlineThreshold"].SetValue(1 / (this.DepthNear - this.DepthFar));

            fx.CurrentTechnique.Passes["Pass1"].Apply();
            this.SpriteBatch.Flush();

            var objs = map.GetObjects().ToList();

            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            //gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true }; // this broke depth on block highlights
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            toolManager.DrawBeforeWorld(this.SpriteBatch, map, this);

            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            ui.DrawWorld(this.SpriteBatch, this);
            map.DrawBeforeWorld(this.SpriteBatch, this);
            foreach (var entity in objs)
                entity.DrawAfter(this.SpriteBatch, this); // cull non visible entities

            this.SpriteBatch.Flush();

            SetBlockTextures(fx);
            fx.CurrentTechnique = fx.Techniques["CombinedWater"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            gd.SetRenderTargets(this.WaterRender, this.WaterLight, this.WaterDepth, this.WaterFog);
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearcol, 1, 1);

            this.PrepareShaderTransparent(map);

            foreach (var chunk in visibleChunks)
            {
                if (!chunk.Valid)
                    continue;
                chunk.DrawTransparentLayers(this, this.Effect);
            }

            // combine scenes and apply ambient light
            gd.SetRenderTarget(this.MapComposite);
            gd.Clear(new Color(fogColor));

            fx.Parameters["s"].SetValue(this.MapRender);
            fx.Parameters["s1"].SetValue(this.MapLight);
            fx.Parameters["s2"].SetValue(this.MapDepth);
            fx.Parameters["s3"].SetValue(this.TextureFogWater);

            var watertxt = Game1.Instance.Content.Load<Texture2D>("Graphics/watersmallpixely");
            fx.Parameters["s4"].SetValue(watertxt);

            gd.SamplerStates[4] = SamplerState.PointWrap;
            fx.Parameters["WaterTextureSize"].SetValue(new Vector2(watertxt.Width, watertxt.Height));

            var offset2 = new Vector2(0, .5f + this.FogT / 100f);

            var wateroffset = (this.Coordinates / (watertxt.Width)).ToFloored() * (watertxt.Width);
            wateroffset = (this.Coordinates - wateroffset) / (watertxt.Width);
            fx.Parameters["WaterOffset"].SetValue(fogoffset - wateroffset);

            var wateroffset2 = (this.Coordinates / (watertxt.Height)).ToFloored() * (watertxt.Height);
            wateroffset = (this.Coordinates - wateroffset) / (watertxt.Height);
            fx.Parameters["WaterOffset2"].SetValue(offset2 - wateroffset);

            this.SpriteBatch.Draw(this.MapRender, this.MapRender.Bounds, gd.Viewport.Bounds, Color.White);
            fx.CurrentTechnique = fx.Techniques["FinalInsideBorders"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            this.SpriteBatch.Flush();

            gd.SetRenderTarget(this.WaterComposite);
            gd.Clear(Color.Transparent);

            fx.Parameters["s"].SetValue(this.WaterRender);
            fx.Parameters["s1"].SetValue(this.WaterLight);
            fx.Parameters["s2"].SetValue(this.WaterDepth);
            fx.Parameters["s3"].SetValue(this.WaterFog); //it's pink/purple before in the shader i write both red values for the fog and blue values for the water

            fx.Parameters["s4"].SetValue(watertxt);

            gd.SamplerStates[4] = SamplerState.PointWrap;
            fx.Parameters["WaterTextureSize"].SetValue(new Vector2(watertxt.Width, watertxt.Height));
            this.SpriteBatch.Draw(this.WaterRender, this.WaterRender.Bounds, gd.Viewport.Bounds, Color.White);
            // TODO: Must draw entities before final composition, so fog is applied over them accordingly
            fx.CurrentTechnique = fx.Techniques["CompositeWater"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            this.SpriteBatch.Flush();

            //sort objects back to front for proper semitraspanrent rendering
            // TODO: culling
            this.SortEntities(map, objs);
            // TODO: have the particle manager set textures because different emitters might use different atlases (blocks vs entities)
            fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);

            this.DrawEntities(scene, objs);
            //map.DrawParticles(this);
            //  // draw entity shadows
            MySpriteBatch shadowsSB = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntityShadows"];
            gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteComponent.DrawShadows(shadowsSB, map, this);
            gd.SetRenderTarget(this.MapComposite);
            shadowsSB.Flush();

            // flush entity spritebatch after shadows so they get drawn above them
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true };
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            gd.SetRenderTargets(this.MapComposite, this.TextureFogWater, this.MapDepth);
            this.SpriteBatch.Flush();

            //  draw particles drawn by entities
            map.DrawParticles(this);
            fx.CurrentTechnique = fx.Techniques["Particles"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            fx.Parameters["s"].SetValue(Block.Atlas.Texture);

            this.BlockParticlesSpriteBatch.Flush();
            fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);

            this.ParticlesSpriteBatch.Flush();

            // draw block mouseover highlight, here or after fog?
            // set textures here or in tool draw method?
            // DRAW here things such as entity previews for debug spawning
            fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);

            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            //gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true }; // this broke depth on block highlights
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            toolManager.DrawAfterWorld(this.SpriteBatch, map);

            this.SpriteBatch.Flush();

            // draw entity mouseover highlight
            fx.CurrentTechnique = fx.Techniques["EntityMouseover"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            if (toolManager.ActiveTool is not null)
                if (toolManager.ActiveTool.Target is not null)
                    if (toolManager.ActiveTool.Target.Object is GameObject mouseover && mouseover.Exists)
                        mouseover.DrawMouseover(this.SpriteBatch, this);

            this.SpriteBatch.Flush();

            // draw non-water on pre-final texture
            gd.SetRenderTargets(this.RenderBeforeFog, this.FogBeforeFog);
            gd.Clear(new Color(fogColor));
            gd.Clear(ClearOptions.DepthBuffer, Color.White, 1, 1);
            fx.CurrentTechnique = fx.Techniques["RenderMapWithoutFog"];
            var fogtxt = Game1.Instance.Content.Load<Texture2D>("Graphics/Fog04");
            fx.Parameters["FogTextureSize"].SetValue(new Vector2(fogtxt.Width, fogtxt.Height));
            gd.SamplerStates[2] = SamplerState.PointWrap;

            fx.Parameters["s"].SetValue(this.MapComposite);
            fx.Parameters["s1"].SetValue(this.TextureFogWater);
            fx.Parameters["s2"].SetValue(fogtxt);
            fx.Parameters["s3"].SetValue(this.MapDepth);

            this.SpriteBatch.Draw(this.MapComposite, this.MapComposite.Bounds, gd.Viewport.Bounds, Color.White);
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            gd.DepthStencilState = DepthStencilState.Default;
            this.SpriteBatch.Flush();

            // draw water on pre-final texture

            fx.Parameters["s"].SetValue(this.WaterComposite);
            fx.Parameters["s1"].SetValue(this.WaterFog);
            fx.Parameters["s2"].SetValue(fogtxt);
            fx.Parameters["s3"].SetValue(this.WaterDepth);

            this.SpriteBatch.Draw(this.WaterComposite, this.WaterComposite.Bounds, gd.Viewport.Bounds, Color.White);
            fx.CurrentTechnique = fx.Techniques["Water"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            this.SpriteBatch.Flush();

            // draw block highlight now so it's correctly placed over water according to depth
            fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);
            // which textures to use???
            fx.Parameters["s"].SetValue(Block.Atlas.Texture);
            fx.Parameters["s1"].SetValue(Block.Atlas.DepthTexture);
            // apply fog to the pre-final texture render(that contains map + water)
            gd.SetRenderTargets(this.FinalScene);
            gd.Clear(new Color(fogColor));

            fx.Parameters["s"].SetValue(this.RenderBeforeFog);
            fx.Parameters["s1"].SetValue(this.FogBeforeFog);
            fx.Parameters["s2"].SetValue(fogtxt);
            fx.Parameters["s3"].SetValue(this.MapDepth);

            this.SpriteBatch.Draw(this.RenderBeforeFog, this.RenderBeforeFog.Bounds, gd.Viewport.Bounds, Color.White);
            fx.CurrentTechnique = fx.Techniques["ApplyFog"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            this.SpriteBatch.Flush();

            ///test
            ///i moved this here from ingame.cs's draw method
            var sb = new SpriteBatch(gd);
            gd.SetRenderTarget(this.FinalScene);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
            map.DrawInterface(sb, this);
            sb.End();
            ///

            // draw final scene to backbuffer
            RenderTarget2D[] targets = new RenderTarget2D[] {
                this.FinalScene,
                this.RenderBeforeFog,
                this.FogBeforeFog,
                this.WaterRender,
                this.WaterDepth,
                this.WaterLight,
                this.WaterFog,
                this.WaterComposite,
                this.MapRender,
                this.MapDepth,
                this.MapLight,
                this.TextureFogWater };
            this.RenderTargets = targets.ToArray();
            gd.SetRenderTarget(null);


            fx.Parameters["s"].SetValue(this.RenderTargets[this.RenderIndex]);
            fx.CurrentTechnique = fx.Techniques["Normal"];

            fx.CurrentTechnique.Passes["Pass1"].Apply();
            this.SpriteBatch.Draw(this.FinalScene, this.FinalScene.Bounds, gd.Viewport.Bounds, Color.White);

            /// added this here to draw the final scene with depth, but i have to change the shader to read depth from the depth texture
            //gd.DepthStencilState = DepthStencilState.Default;

            this.SpriteBatch.Flush();

            // draw ui and other elements
            map.DrawWorld(this.SpriteBatch, this);
            this.SpriteBatch.Flush();
        }

        private static void SetBlockTextures(Effect fx)
        {
            fx.Parameters["s"].SetValue(Block.Atlas.Texture);
            fx.Parameters["s2"].SetValue(Block.Atlas.NormalTexture);
            fx.Parameters["s3"].SetValue(Block.Atlas.DepthTexture);
        }

        private void DrawEntities(SceneState scene, List<GameObject> objs)
        {
            foreach (var obj in objs)
            {
                if (obj.Global.Z > this.MaxDrawZ + 1)
                    continue;

                // TODO: check bounding box intersection instead of single point to avoid entity pop-in
                var bounds = obj.GetScreenBounds(this); // TODO: cache bounds?
                if (!this.ViewPort.Intersects(bounds))
                    continue;

                obj.Draw(this.SpriteBatch, this);
                scene.ObjectsDrawn.Add(obj);
            }
        }

        private void SortEntities(MapBase map, List<GameObject> objs)
        {
            objs.Sort((o1, o2) =>
            {
                float d1 = o1.Global.GetDrawDepth(map, this);
                float d2 = o2.Global.GetDrawDepth(map, this);
                if (d1 < d2)
                    return -1;
                else if (d1 == d2)
                    return 0;
                else
                    return 1;
            });
        }
        public void NewDraw(RenderTarget2D target, MapBase map, GraphicsDevice gd, EngineArgs a, SceneState scene, ToolManager toolManager)
        {
            this.MapRender ??= new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);
            this.MapDepth ??= new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);
            this.MapLight ??= new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);

            var fx = Game1.Instance.Content.Load<Effect>("blur");

            fx.Parameters["BlockWidth"].SetValue(Block.Width + 2 * Graphics.Borders.Thickness);
            fx.Parameters["BlockHeight"].SetValue(Block.Height + 2 * Graphics.Borders.Thickness);
            fx.Parameters["AtlasWidth"].SetValue(Block.Atlas.Texture.Width);
            fx.Parameters["AtlasHeight"].SetValue(Block.Atlas.Texture.Height);
            fx.Parameters["Viewport"].SetValue(new Vector2(target.Width, target.Height));
            fx.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);
            fx.Parameters["Zoom"].SetValue(this.Zoom);
            float borderPx = 1;
            fx.Parameters["BorderResolution"].SetValue(new Vector2(borderPx / target.Width, borderPx / target.Height) * this.Zoom);
            Color ambientColor = Color.Lerp(Color.White, map.GetAmbientColor(), 0);
            Vector4 ambient = ambientColor.ToVector4();
            fx.Parameters["AmbientLight"].SetValue(ambient);

            gd.DepthStencilState = DepthStencilState.Default;

            gd.SamplerStates[0] = SamplerState.PointClamp;
            gd.SamplerStates[1] = SamplerState.PointClamp;
            gd.SamplerStates[2] = SamplerState.PointClamp;
            gd.SamplerStates[3] = SamplerState.PointClamp;

            MySpriteBatch mySB = new MySpriteBatch(gd);

            // use new technique to draw both color and light in one pass in multiple rendertargets
            this.DrawBlocks(map, gd, a, fx, mySB);

            // combine scenes
            gd.SetRenderTarget(target);
            this.DrawScene(target, gd, fx, mySB);

            // draw objects
            this.DrawEntities(map, gd, scene, fx, mySB);

            // draw entity shadows
            this.DrawEntityShadows(map, gd, fx, mySB);

            // draw block selection, using shadow shader for projected textures
            this.DrawBlockSelection(map, toolManager, fx, mySB);

            this.DrawMouseoverEntity(fx, mySB);
        }

        private void DrawBlocks(MapBase map, GraphicsDevice gd, EngineArgs a, Effect fx, MySpriteBatch mySB)
        {
            gd.SetRenderTargets(this.MapRender, this.MapLight, this.MapDepth);
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(1f, 1f, 1f, 0), 1, 1);
            fx.CurrentTechnique = fx.Techniques["Combined"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SetBlockTextures(fx);
            this.DepthNear = float.MinValue;
            this.DepthFar = float.MaxValue;
            map.DrawBlocks(mySB, this, a);
            fx.Parameters["FarDepth"].SetValue(this.DepthFar);
            fx.Parameters["NearDepth"].SetValue(this.DepthNear);
            fx.Parameters["DepthResolution"].SetValue((2) / (this.DepthNear - this.DepthFar));
            fx.Parameters["OutlineThreshold"].SetValue((1) / (this.DepthNear - this.DepthFar));
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            mySB.Flush();
        }
        private void DrawScene(RenderTarget2D target, GraphicsDevice gd, Effect fx, MySpriteBatch mySB)
        {
            gd.Clear(Color.Transparent);
            SetMapTextures(fx);

            mySB.Draw(this.MapRender, this.MapRender.Bounds, target.Bounds, Color.White);
            fx.CurrentTechnique = fx.Techniques["FinalInsideBorders"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            mySB.Flush();
        }

        private void SetMapTextures(Effect fx)
        {
            fx.Parameters["s"].SetValue(this.MapRender);
            fx.Parameters["s1"].SetValue(this.MapLight);
            fx.Parameters["s2"].SetValue(this.MapDepth);
        }

        private void DrawEntities(MapBase map, GraphicsDevice gd, SceneState scene, Effect fx, MySpriteBatch mySB)
        {
            fx.CurrentTechnique = fx.Techniques["Entities"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);
            map.DrawObjects(mySB, this, scene);
            mySB.Flush();
        }
        private void DrawMouseoverEntity(Effect fx, MySpriteBatch mySB)
        {
            GameObject mouseover = Controller.Instance.Mouseover.Object as GameObject;
            fx.CurrentTechnique = fx.Techniques["Default"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            if (mouseover is not null)
                mouseover.DrawMouseover(mySB, this);
            mySB.Flush();
        }
        private void DrawBlockSelection(MapBase map, ToolManager toolManager, Effect fx, MySpriteBatch mySB)
        {
            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            toolManager.DrawBeforeWorld(mySB, map, this);
            mySB.Flush();
        }
        private void DrawEntityShadows(MapBase map, GraphicsDevice gd, Effect fx, MySpriteBatch mySB)
        {
            fx.CurrentTechnique = fx.Techniques["EntityShadows"];
            gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteComponent.DrawShadows(mySB, map, this);
            mySB.Flush();
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            if (e.Handled)
                return;

            if (e.KeyValue == (int)Keys.F4)
            {
                var max = this.RenderTargets.GetUpperBound(0) + 1;
                this.RenderIndex = (this.RenderIndex + 1) % max;// 3;
                e.Handled = true;
            }
        }

        public void ZoomIncrease()
        {
            this.ZoomNext *= 2;
            this.ZoomNext = MathHelper.Clamp(this.ZoomNext, this.ZoomMin, this.ZoomMax);

        }
        public void ZoomDecrease()
        {
            this.ZoomNext /= 2;
            this.ZoomNext = MathHelper.Clamp(this.ZoomNext, this.ZoomMin, this.ZoomMax);
        }
        public void ZoomReset()
        {
            this.ZoomNext = InitialZoom;
        }

        public float GetFarDepth(MapBase map)
        {
            var size = map.GetSizeInChunks() * Chunk.Size;// -1;
            return (int)this.Rotation switch
            {
                0 => Vector3.Zero.GetDrawDepth(map, this),
                1 => new Vector3(0, size, 0).GetDrawDepth(map, this),
                2 => new Vector3(size, size, 0).GetDrawDepth(map, this),
                3 => new Vector3(size, 0, 0).GetDrawDepth(map, this),
                _ => 0,
            };
        }
        public float GetNearDepth(MapBase map)
        {
            var size = map.GetSizeInChunks() * Chunk.Size;// -1;
            return (int)this.Rotation switch
            {
                0 => new Vector3(size, size, 0).GetDrawDepth(map, this),
                1 => new Vector3(size, 0, 0).GetDrawDepth(map, this),
                2 => Vector3.Zero.GetDrawDepth(map, this),
                3 => new Vector3(0, size, 0).GetDrawDepth(map, this),
                _ => 0,
            };
        }

        public void UpdateMaxDrawLevel(MapBase map)
        {
            this.MaxDrawZ = this.GetMaxDrawLevel(map);
        }
        public int GetMaxDrawLevel(MapBase map)
        {
            var actor = map.Net.GetPlayer()?.ControllingEntity;
            var value = (this.HideTerrainAbovePlayer && (actor is not null)) ? (int)actor.Transform.Global.RoundXY().Z + 2 + this.HideTerrainAbovePlayerOffset : this.DrawLevel;
            value = Math.Min(MapBase.MaxHeight - 1, Math.Max(0, value));
            return value;
        }
        internal void ToggleHideBlocksAbove()
        {
            this.HideTerrainAbovePlayer = !this.HideTerrainAbovePlayer;
            if (this.HideTerrainAbovePlayer)
                this.HideTerrainAbovePlayerOffset = 0;
        }

        internal void AdjustDrawLevel(int p)
        {
            if (!this.HideTerrainAbovePlayer)
                this.DrawLevel = Math.Min(MapBase.MaxHeight - 1, Math.Max(0, this.DrawLevel + p));
            else
                this.HideTerrainAbovePlayerOffset += p;
        }

        public void OnDeviceLost()
        {
            int w = this.Width, h = this.Height;
            var gfx = Game1.Instance.GraphicsDevice;

            this.MapRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.MapDepth = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.MapLight = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.TextureFogWater = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.MapComposite = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);

            this.RenderBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.LightBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.DepthBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.FogBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);

            this.FinalScene = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);

            this.WaterRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterDepth = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterLight = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterComposite = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
        }

        public void MousePicking(MapBase map, bool ignoreEntities = false)
        {
            var visibleChunks = map.GetActiveChunks().Values.Where(ch => this.ViewPort.Intersects(ch.GetScreenBounds(this)));
            if(!(ignoreEntities || Controller.IsBlockTargeting()))
                foreach (var chunk in visibleChunks)
                    chunk.HitTestEntities(this);

            /// uncomment this to prefer targetting entities even when they are behind blocks
            //if (Controller.Instance.MouseoverNext.Object is not null)
            //    return;
          
            if (!BlockTargeting)
                return;

            var controller = Controller.Instance;
            var hidewalls = Engine.HideWalls;
            var actor = map.Net.GetPlayer()?.ControllingEntity;
            var playerExists = actor != null;
            var playerGlobal = playerExists ? actor.Global : default;
            var radius = .01f * this.Zoom * this.Zoom; //occlusion radius
            var found = false;
            var foundDepth = float.MinValue;
            var foundGlobal = Vector3.Zero;
            var foundMouse = Vector2.Zero;
            Block foundBlock;
            var foundRect = Rectangle.Empty;
            var camx = this.Coordinates.X - (this.Width / 2f) / this.Zoom;
            var camy = this.Coordinates.Y - (this.Height / 2f) / this.Zoom;
            var mouse = UIManager.Mouse;
            var mousex = (int)mouse.X;
            var mousey = (int)mouse.Y;
            var behind = InputState.IsKeyDown(Keys.Menu);

            var rectw = (int)(Block.Width * this.Zoom);
            var recth = (int)(Block.Height * this.Zoom);
            foreach (var chunk in visibleChunks)
            {
                var chunkBounds = chunk.GetScreenBounds(this);
                if (!chunkBounds.Contains(mousex, mousey))
                    continue;

                Coords.Iso(this, chunk.X * Chunk.Size, chunk.Y * Chunk.Size, 0, out float chunkx, out float chunky);
                chunkx -= camx;
                chunky -= camy;

                var foglvl = this.GetFogLevel();
                for (int j = this.MaxDrawZ; j >= foglvl; j--)
                {
                    var slice = chunk.Slices[j];
                    if (slice is null)
                        continue;

                    /// removing this check because it screws up mousepicking when slices are invalidated by blocks changing (like actors trampling grass)
                    //if (!slice.Valid)
                    //    continue;
                    if (slice.Canvas is null)
                        continue;

                 
                    var arrays = slice.Canvas.GetMouseoverableMeshes();

                    //if (j == this.MaxDrawZ)
                    //    arrays.Add(slice.Unknown.vertices);
                    if (j == this.MaxDrawZ)
                    {
                        if(this.HideUnknownBlocks)
                            arrays = arrays.Append(slice.Unknown.vertices);
                        else
                            arrays = arrays.Concat(slice.TopCover.GetMouseoverableMeshes());
                    }

                    foreach (var array in arrays)
                    {
                        var count = array.Length;
                        for (int i = count - 4; i >= 0; i -= 4)
                        {
                            if (!this.EarlyOutMousePicking(array, i, mousex, mousey, chunkx, chunky, rectw, recth, out int rectx, out int recty, out Vector3 global))
                                continue;

                            // TODO: check intersection in previous stages
                            //if (rectx <= mousex && mousex < rectx + rectw && recty <= mousey && mousey < recty + recth)
                            //{
                            var block = chunk.GetBlockFromGlobal(global.X, global.Y, global.Z);

                            if (!block.IsTargetable(global))
                                continue;

                            if (hidewalls)
                            {
                                if (playerExists)
                                {
                                    if (global.Z >= playerGlobal.Z)
                                    {
                                        if (global.X + global.Y > playerGlobal.X + playerGlobal.Y)
                                        {
                                            if (block.Opaque)
                                            {
                                                //distance between mouse and center of screen normalized between -1,1
                                                var dx = mousex - this.Width / 2f;
                                                var dy = mousey - this.Height / 2f;
                                                var d = new Vector2(dx, dy);
                                                d.Y /= this.Width / (float)this.Height;
                                                d /= new Vector2(this.Width / 2f, this.Height / 2f);
                                                var l = d.LengthSquared();
                                                if (l < radius)
                                                    continue;
                                            }
                                        }
                                    }
                                }
                            }

                            var xx = (int)((mousex - rectx) / this.Zoom);
                            var yy = (int)((mousey - recty) / this.Zoom);
                            if (!block.MouseMap.HitTestEarly(xx, yy))
                                continue;

                            Coords.Rotate(this, global.X, global.Y, out int rx, out int ry);
                            var currentDepth = rx + ry + global.Z;

                            if (currentDepth > foundDepth)
                            {
                                foundDepth = currentDepth;
                                foundGlobal = global;
                                foundMouse = mouse;
                                foundRect = new Rectangle(rectx, recty, rectw, recth);
                                foundBlock = block;
                                found = true;
                            }
                            //}
                        }
                    }
                }

            }
            if (found)
            {
                // create mouseover anyway even if air in case of undiscovered area? or check drawunknownblocks?
                this.CreateMouseover(map, foundGlobal, foundRect, foundMouse, behind);
            }
        }
        public void CreateMouseover(MapBase map, Vector3 global, Rectangle rect, Vector2 point, bool behind)
        {
            /// uncomment this to prefer targetting entities even when they are behind blocks
            /// i also call this at the start of the mouspicking method, no need to call it here too
            //if (Controller.Instance.MouseoverNext.Object != null)
            //    return;
            if (Controller.Instance.MouseoverNext.Object is TargetArgs target && target.Object is GameObject obj)
                if (this.GetDrawDepthSimple(obj.CellIfSpawned.Value) > this.GetDrawDepthSimple(global)) // HACK
                    return;

            if (!map.TryGetAll(global, out var chunk, out var cell))
                return;

            var uvCoords = new Vector2((point.X - rect.X) / this.Zoom, (point.Y - rect.Y) / this.Zoom);
            int faceIndex = (int)uvCoords.Y * cell.Block.MouseMap.Texture.Width + (int)uvCoords.X;

            // find block coordinates
            var sample = cell.Block.UV[faceIndex];
            float u = sample.R / 255f;
            float v = sample.G / 255f;
            float w = sample.B / 255f;
            var precise = new Vector3(u, v, w);// Vector3.Zero;
            precise.X -= 0.5f;
            precise.Y -= 0.5f; // compensate for (0,0) being at the center of the block

            cell.Block.MouseMap.HitTest(behind, (int)uvCoords.X, (int)uvCoords.Y, out Vector3 vec);

            // comment these lines if i want to select blocks even if mouseover face is inaccessible
            //if (!Cell.CheckFace(this, cell, vec))
            //    return;

            Coords.Rotate((int)this.Rotation, vec, out Vector3 rotVec);
            precise = precise.Rotate(-this.Rotation);
            // TODO: find more elegant way to do this
            if (rotVec == Vector3.UnitX || rotVec == -Vector3.UnitX)
                precise.X = 0;
            else if (rotVec == Vector3.UnitY || rotVec == -Vector3.UnitY)
                precise.Y = 0;
            else if (rotVec == Vector3.UnitZ || rotVec == -Vector3.UnitZ)
                precise.Z = 0;
            Controller.SetMouseoverBlock(this, map, global, rotVec, precise);
        }

        public bool EarlyOutMousePicking(MyVertex[] array, int i, float mousex, float mousey, float chunkx, float chunky, int rectw, int recth, out int rectx, out int recty, out Vector3 global)
        {
            rectx = recty = 0;
            var v = array[i];
            global = v.BlockCoords;
            var tl = v.Position;

            var br = array[i + 2].Position;
            if (br.X - tl.X == 0)
                return false;

            var xxx = tl.X + chunkx;
            rectx = (int)(xxx * this.Zoom);
            if (mousex < rectx)
                return false;

            var yyy = tl.Y + chunky;
            recty = (int)(yyy * this.Zoom);
            if (mousey < recty)
                return false;

            if (mousex >= rectx + rectw)
                return false;

            if (mousey >= recty + recth)
                return false;

            return true;
        }

        public int GetFogLevel()
        {
            return (int)Math.Max(0, this.LastZTarget - FogZOffset - FogFadeLength);
        }

        public void DrawGrid(MySpriteBatch sb, MapBase map, IEnumerable<IntVec3> positions, Color col)
        {
            Sprite.Atlas.Begin(this.Effect);

            foreach (var pos in positions)
                this.DrawGridCell(sb, col, pos);

            sb.Flush();
        }
        public void DrawGridCells(MySpriteBatch sb, Color col, IEnumerable<IntVec3> globals)
        {
            foreach (var pos in globals)
                this.DrawGridCell(sb, col, pos);
        }
        public void DrawGridCell(MySpriteBatch sb, Color col, IntVec3 global)
        {
            if (global.Z > this.DrawLevel + 1)
                return;

            var bounds = this.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, this);
            var highlight = Block.FaceHighlights[IntVec3.UnitZ];
            sb.Draw(highlight.Atlas.Texture, pos, highlight.Rectangle, 0, Vector2.Zero, this.Zoom, col, SpriteEffects.None, depth);
        }
        [Obsolete]
        public void DrawGridBlock(MySpriteBatch sb, Color col, IntVec3 global)
        {
            if (global.Z > this.DrawLevel)
                return;

            var bounds = this.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, this);
            sb.Draw(Sprite.Atlas.Texture, pos, Sprite.BlockHighlight.AtlasToken.Rectangle, 0, Vector2.Zero, this.Zoom, col * .5f, SpriteEffects.None, depth);
        }
        public void DrawGridBlock(MySpriteBatch sb, Graphics.AtlasDepthNormals.Node.Token sprite, Color col, IntVec3 global)
        {
            if (global.Z > this.DrawLevel)
                return;
            //col *= .5f;
            sprite.Atlas.Begin(this.Effect); // this was commented out
            var bounds = this.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, this);
            //sb.Draw(Sprite.Atlas.Texture, pos, sprite.Rectangle, 0, Vector2.Zero, this.Zoom, col, SpriteEffects.None, depth);
            sb.Draw(sprite.Atlas.Texture, pos, sprite.Rectangle, 0, Vector2.Zero, this.Zoom, col, SpriteEffects.None, depth);
        }
        public void DrawGridBlocks(MySpriteBatch sb, IEnumerable<IntVec3> positions, Color col)
        {
            Sprite.Atlas.Begin(this.Effect);
            foreach (var pos in positions)
                this.DrawGridBlock(sb, col, pos);
            sb.Flush();
        }
        public void DrawCellHighlights(AtlasDepthNormals.Node.Token sprite, IEnumerable<IntVec3> positions, Color col)
        {
            this.DrawCellHighlights(this.SpriteBatch, sprite, positions, col);
        }
        public void DrawCellHighlights(MySpriteBatch sb, AtlasDepthNormals.Node.Token sprite, IEnumerable<IntVec3> positions, Color col)
        {
            if (!positions.Any())
                return;
            sb.Flush();
            var fx = this.Shader;
            fx.CurrentTechnique = this.TechniqueBlockHighlight;// fx.Techniques["BlockHighlight"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            sprite.Atlas.Begin(this.Effect);
            foreach (var pos in positions)
                this.DrawGridBlock(sb, sprite, col, pos);
            sb.Flush();
        }
        public void DrawBlockMouseover(MySpriteBatch sb, MapBase map, Vector3 global, Color color)
        {
            if (global.Z > this.DrawLevel)
                return;

            Rectangle bounds = Block.Bounds;
            this.GetEverything(map, global, bounds, out float cd, out Rectangle screenBounds, out Vector2 screenLoc);
            var scrbnds = this.GetScreenBoundsVector4(global.X, global.Y, global.Z, bounds, Vector2.Zero);
            screenLoc = new Vector2(scrbnds.X, scrbnds.Y);
            cd = global.GetDrawDepth(map, this);
            
            Block.Atlas.Begin(this.Effect);
            var c = color * .5f;
            var zoom = new Vector2(this.Zoom);
            sb.Draw(Block.Atlas.Texture, screenLoc, Block.BlockHighlightBack.Rectangle, 0, Vector2.Zero, zoom,
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);

            /// this code draw individual faces instead of the whole highlight
            //sb.Draw(Block.Atlas.Texture, screenLoc, Block.FaceHighlights[-IntVec3.UnitX].Rectangle, 0, Vector2.Zero, zoom,
            //  Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, global.West().GetDrawDepth(map, this));
            //sb.Draw(Block.Atlas.Texture, screenLoc, Block.FaceHighlights[-IntVec3.UnitY].Rectangle, 0, Vector2.Zero, zoom,
            //  Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, global.North().GetDrawDepth(map, this));
            //sb.Draw(Block.Atlas.Texture, screenLoc, Block.FaceHighlights[-IntVec3.UnitZ].Rectangle, 0, Vector2.Zero, zoom,
            //              Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, global.Below().GetDrawDepth(map, this));


            sb.Draw(Block.Atlas.Texture, screenLoc, Block.BlockHighlight.Rectangle, 0, Vector2.Zero, zoom,
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);
            sb.Flush(); // flush here because i might have to switch textures in an overriden tool draw call
        }
       
        internal bool IsDrawable(MapBase map, Vector3 global)
        {
            return global.Z <= this.GetMaxDrawLevel(map) + 1;
        }

        internal void ToggleFollowing(GameObject gameObject)
        {
            this.Following = this.Following == gameObject ? null : gameObject;
        }

        internal bool IsCompletelyHiddenByFog(float z)
        {
            return z < this.LastZTarget - FogZOffset - FogFadeLength + 1;
        }

        public void HandleKeyPress(KeyPressEventArgs e)
        {
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
        }

        public void HandleMouseMove(HandledMouseEventArgs e)
        {
        }

        public void HandleLButtonDown(HandledMouseEventArgs e)
        {
        }

        public void HandleLButtonUp(HandledMouseEventArgs e)
        {
        }

        public void HandleRButtonDown(HandledMouseEventArgs e)
        {
        }

        public void HandleRButtonUp(HandledMouseEventArgs e)
        {
        }

        public void HandleMiddleUp(HandledMouseEventArgs e)
        {
        }

        public void HandleMiddleDown(HandledMouseEventArgs e)
        {
        }

        public void HandleMouseWheel(HandledMouseEventArgs e)
        {
        }

        public void HandleLButtonDoubleClick(HandledMouseEventArgs e)
        {
        }
    }
}
