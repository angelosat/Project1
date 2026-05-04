using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Simulation.Lighting;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Graphics;
using Project1.Framework.Input;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Project1.Core.Rendering;

interface IDrawContext
{
    RenderContext View { get; }
}
public sealed class Renderer : IDrawContext, IInputEventHandler
{
    static Renderer()
    {
    }
    int _lastWidth, _lastHeight;
    public const int FogZOffset = 2, FogFadeLength = 8;
    Vector4 FogColor = Color.SteelBlue.ToVector4();
    bool _hideUnknownBlocks = true;
    public bool MysteriousBlocks // TODO: make it static
    {
        get => this._hideUnknownBlocks;
        set
        {
            this._hideUnknownBlocks = value;
            Ingame.MainViewportMap.InvalidateChunks();
        }
    }
    bool _drawTopSlice = true;
    public bool DrawTopSlice
    {
        get => this._drawTopSlice;
        set
        {
            this._drawTopSlice = value;
            Ingame.MainViewportMap.InvalidateChunks();
        }
    }
    public bool DrawZones = true;
    public static bool HideCeiling;
    public bool HideTerrainAbovePlayer;
    public int HideTerrainAbovePlayerOffset;


    public static bool BlockTargeting = true;
    public static bool Fog = true;
    public bool HideUnderground;
    public bool BorderShading;
    public float FogLevel = 0;
    public int MaxDrawZ;
    public int RenderIndex = 0;
    public RenderTarget2D MapRender,
      WaterRender, WaterDepth, WaterLight, WaterFog,
      WaterComposite,
      MapDepth, MapNormals, MapLight, TextureFogWater, MapComposite,
      RenderBeforeFog, LightBeforeFog, DepthBeforeFog, FogBeforeFog,
      FinalScene;


    public float LastZTarget;
    float DepthFar, DepthNear;
    public MySpriteBatch SpriteBatch;
    public MySpriteBatch WaterSpriteBatch, // waterspritebatch is not used!?
        ParticlesSpriteBatch, BlockParticlesSpriteBatch, TransparentBlocksSpriteBatch;
    //float FogT = 0;
    public static Effect Effect = Game1.Instance.Content.Load<Effect>("blur");
    private readonly Texture2D WaterTexture;
    private Texture2D FogTexture;
    public static bool DrawnOnce = false;
    public RenderTarget2D[] RenderTargets = new RenderTarget2D[5];
    
    public Renderer()
        : this(Game1.Bounds.Width, Game1.Bounds.Height)
    {
        this.WaterSpriteBatch = new(Game1.Instance.GraphicsDevice);
        this.SpriteBatch = new(Game1.Instance.GraphicsDevice);
        this.UISpritebatch = new(Game1.Instance.GraphicsDevice);
    }
    public Renderer(int width, int height, float x = 0, float y = 0, float z = 0, float zoom = 2, int rotation = 0)
    {
        this._lastWidth = width;
        this._lastHeight = height;
        //this.Width = width;
        //this.Height = height;
        //this.ViewPort = new Rectangle(0, 0, this.Width, this.Height);
        //this.Zoom = zoom;
        //this.ZoomNext = zoom;
        //this.Rotation = rotation;
        //this.CenterOn(new Vector3(x, y, z));
        Game1.Instance.graphics.DeviceReset += this.gfx_DeviceReset;
        this.RecreateRenderTargets();
        //Effect = Game1.Instance.Content.Load<Effect>("blur");
        this.WaterTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/watersmallpixely");
        this.FogTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/Fog04");
        this.SetInvariants();
    }
  
    void OnRotationChanged()
    {
        Ingame.MainViewportMap.OnCameraRotated(this);
        SelectionManager.Instance.OnCameraRotated(this);
    }
    void gfx_DeviceReset(object sender, EventArgs e)
    {
        //this.Width = Game1.Bounds.Width;
        //this.Height = Game1.Bounds.Height;
        //this.ViewPort = new Rectangle(0, 0, this.Width, this.Height);
        this.RecreateRenderTargets();
    }
    
    internal bool HideWalls;
 
    public void DrawBlock(Canvas canvas, MapView view, Block block, Chunk chunk, IntVec3Local local)
    {
        int lx = local.X, ly = local.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;
        int z = local.Z;
        var global = new IntVec3(gx, gy, z);
        var light = GetFinalLight(view, chunk, global);

        var screenBoundsVector4 = view.GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
        view.Rotate(lx, ly, out int rlx, out int rly);
        var depth = rlx + rly;

        var finalFogColor = Color.Transparent; // i calculate fog inside the shader from now on
        //var isDiscovered = !map.IsUndiscovered(global);

        block.Draw(canvas, chunk, global, view, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth, 0, 0, 0, null);
    }
    
    public bool DrawCell(Canvas canvas, MapView view, Chunk chunk, IntVec3Local local)
    {
        var block = chunk.GetBlock(local);
        if (block is BlockAir)
            throw new Exception(); /// drawcell should never be called for air blocks, there are problems elsewhere

        int lx = local.X, ly = local.Y, gx = chunk.Start.X + lx, gy = chunk.Start.Y + ly;
        int z = local.Z;
        var global = local.ToGlobal(chunk);
        var light = GetFinalLight(view, chunk, global);

        var screenBoundsVector4 = view.GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
        view.Rotate(lx, ly, out int rlx, out int rly);
        var depth = rlx + rly;

        var finalFogColor = Color.Transparent; // i calculate fog inside the shader from now on
        //var isDiscovered = !map.IsUndiscovered(global);

        block.Draw(canvas, chunk, global, view, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth);//.Variation, cell.Orientation, cell.BlockData, cell.Material);

        return true;
    }
    static readonly Color CellSelectionTint = Color.White * .5f;
    public void DrawBlockSelectionGlobal(MySpriteBatch sb, MapView view, IntVec3 global)
    {
        this.DrawBlockSelectionGlobal(sb, view, Block.BlockHighlight, global);
    }
    public void DrawBlockSelectionGlobal(MySpriteBatch sb, MapView view, AtlasDepthNormals.Node.Token texToken, IntVec3 global)
    {
        this.DrawBlockSelectionGlobal(sb, view, global, texToken, CellSelectionTint);
    }
    public void DrawBlockSelectionGlobal(MySpriteBatch sb, MapView view, IntVec3 global, AtlasDepthNormals.Node.Token texToken, Color tint)
    {
        int z = global.Z;
        int gx = global.X;
        int gy = global.Y;
        var screenBoundsVector4 = view.GetScreenBoundsVector4NoOffset(gx, gy, z, Block.Bounds, Vector2.Zero);
        view.Rotate(gx, gy, out int rlx, out int rly);
        var depth = rlx + rly;

        sb.DrawBlock(Block.Atlas.Texture, screenBoundsVector4,
            texToken,
            view.Zoom, Color.Transparent, tint, Color.White, Color.White, Vector4.One, Vector4.Zero, depth, null, global);
    }
    
    public bool DrawUnknown(Canvas canvas, MapView view, Chunk chunk, IntVec3Local local)
    {
        int z = local.Z;
        int lx = local.X, ly = local.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;
        var map = view.Map;
        var camera = view.Camera;
        var global = local.ToGlobal(chunk);
        var mapOffset = map.GetOffset();
        view.Rotate(gx - mapOffset.X, gy - mapOffset.Y, out int rgx, out int rgy);
        var light = GetFinalLight(view, chunk, global);

        var screenBoundsVector4 = view.GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
        view.Rotate(lx, ly, out int rlx, out int rly);
        var depth = rlx + rly;

        Block.DrawUnknown(canvas.Opaque, new Vector3(gx, gy, z), camera, screenBoundsVector4, light.Sun, light.Block, Color.Transparent, Color.White, depth);

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
    public void PrepareShaderNew(MapView viewport, string technique)
    {
        var camera = viewport.Camera;
        var zoom = camera.Zoom;
        var coordinates = camera.Position;
        var w = viewport.Width;
        var h = viewport.Height;
        var view =
            new Matrix(
              1.0f, 0.0f, 0.0f, 0.0f,
              0.0f, -1.0f, 0.0f, 0.0f,
              0.0f, 0.0f, 1.0f, 0.0f,
              0.0f, 0.0f, 0.0f, 1.0f);
        float camerax = coordinates.X;
        float cameray = coordinates.Y;
        view = view * Matrix.CreateTranslation(new Vector3(-camerax, cameray, 0)) * Matrix.CreateScale(zoom) * Matrix.CreateTranslation(new Vector3(w / 2, -h / 2, 0));
        this.ApplyDepthParameters(viewport);

        // NOTE: DepthNear/Far are inverted here because world Z and
        // orthographic projection depth are negated in this pipeline
        // (view matrix flips Y + depth convention is reversed).
        // Swapping is required to preserve correct depth ordering.
        var nearScaled = -this.DepthFar * zoom;
        var farScaled = -this.DepthNear * zoom;
        // Depth space is inverted relative to world Z.
        // This engine treats higher world Z as "closer to camera",
        // which requires swapping near/far when constructing orthographic depth bounds.

        var projection = Matrix.CreateOrthographicOffCenter(
            0, w, -h, 0, nearScaled, farScaled);
        Effect.CurrentTechnique = Effect.Techniques[technique];
        Effect.Parameters["View"].SetValue(view);
        Effect.Parameters["Projection"].SetValue(projection);
    }
    private void ApplyDepthParameters(MapView view)
    {
        var fx = Effect;

        this.DepthNear = view.GetNearDepth();
        this.DepthFar = view.GetFarDepth();

        fx.Parameters["FarDepth"].SetValue(this.DepthFar);
        fx.Parameters["NearDepth"].SetValue(this.DepthNear);

        var dd = this.DepthNear - this.DepthFar;
        fx.Parameters["DepthResolution"].SetValue(2 / dd);
        fx.Parameters["OutlineThreshold"].SetValue(1 / dd);
    }
    public void PrepareShader(MapView viewport)
    {
        var camera = viewport.Camera;
        var zoom = camera.Zoom;
        var coordinates = camera.Position;
        var w = viewport.Width;
        var h = viewport.Height;
        var view =
            new Matrix(
              1.0f, 0.0f, 0.0f, 0.0f,
              0.0f, -1.0f, 0.0f, 0.0f,
              0.0f, 0.0f, 1.0f, 0.0f,
              0.0f, 0.0f, 0.0f, 1.0f);
        float camerax = coordinates.X;
        float cameray = coordinates.Y;
        view = view * Matrix.CreateTranslation(new Vector3(-camerax, cameray, 0)) * Matrix.CreateScale(zoom) * Matrix.CreateTranslation(new Vector3(w / 2, -h / 2, 0));
        var far = viewport.GetFarDepth();
        var near = viewport.GetNearDepth();

        var nearScaled = -far * zoom;
        var farScaled = -near * zoom;

        var projection = Matrix.CreateOrthographicOffCenter(
            0, w, -h, 0, nearScaled, farScaled);
        Effect.CurrentTechnique = Effect.Techniques["Chunks"];
        Effect.Parameters["View"].SetValue(view);
        Effect.Parameters["Projection"].SetValue(projection);

        this.DepthNear = near;
        this.DepthFar = far;
        Effect.Parameters["FarDepth"].SetValue(this.DepthFar);
        Effect.Parameters["NearDepth"].SetValue(this.DepthNear);
        var depthDiff = this.DepthNear - this.DepthFar;
        Effect.Parameters["DepthResolution"].SetValue(2 / depthDiff);
        Effect.Parameters["OutlineThreshold"].SetValue(1 / depthDiff);
    }

    public void PrepareShaderTransparent(MapView viewport)
    {
        var camera = viewport.Camera;
        var zoom = camera.Zoom;
        var coordinates = camera.Position;
        var w = viewport.Width;
        var h = viewport.Height;
        var view =
            new Matrix(
               1.0f, 0.0f, 0.0f, 0.0f,
               0.0f, -1.0f, 0.0f, 0.0f,
               0.0f, 0.0f, 1.0f, 0.0f,
               0.0f, 0.0f, 0.0f, 1.0f);
        float camerax = coordinates.X;
        float cameray = coordinates.Y;
        view = view * Matrix.CreateTranslation(new Vector3(-camerax, cameray, 0)) * Matrix.CreateScale(zoom) * Matrix.CreateTranslation(new Vector3(w / 2, -h / 2, 0));
        
        var near = viewport.GetNearDepth();
        var far = viewport.GetFarDepth();

        var nearScaled = -far * zoom;
        var farScaled = -near * zoom;
        var projection = Matrix.CreateOrthographicOffCenter(
            0, w, -h, 0, nearScaled, farScaled);
        Effect.CurrentTechnique = Effect.Techniques["CombinedWater"];
        Effect.Parameters["View"].SetValue(view);
        Effect.Parameters["Projection"].SetValue(projection);

        this.DepthNear = near;
        this.DepthFar = far;
        Effect.Parameters["FarDepth"].SetValue(this.DepthFar);
        Effect.Parameters["NearDepth"].SetValue(this.DepthNear);
        var depthD = this.DepthNear - this.DepthFar;
        Effect.Parameters["DepthResolution"].SetValue(2 / depthD);
        Effect.Parameters["OutlineThreshold"].SetValue(1 / depthD);

    }
    public static LightToken GetFinalLight(MapView view, Chunk chunk, IntVec3 global)
    {
        if (chunk.TryGetCachedLight(global, out LightToken cached))
            return cached;

        //Coords.Rotate(camera, 1, 0, out int rightx, out int righty);
        //Coords.Rotate(camera, 0, 1, out int leftx, out int lefty);
        view.Rotate(1, 0, out int rightx, out int righty);
        view.Rotate(0, 1, out int leftx, out int lefty);
        var map = view.Map;
        Chunk.TryGetFinalLight(map, global + new IntVec3(rightx, -righty, 0), out byte suneast, out byte blockeast);
        Chunk.TryGetFinalLight(map, global + new IntVec3(-leftx, lefty, 0), out byte sunsouth, out byte blocksouth);
        Chunk.TryGetFinalLight(map, global, out byte sunCenter, out byte blockCenter);

        byte suntop, blocktop;
        if (global.Z + 1 < MapBase.MaxHeight)
        {
            suntop = Math.Max((byte)0, chunk.GetSkylight(global.Above));
            blocktop = chunk.GetBlockLight(global.Above);
        }
        else
        {
            suntop = 15;
            blocktop = 15;
        }
        // add the current cell's light as the 4th coord?
        //Color sun = new((suneast + 1) / 16f, (sunsouth + 1) / 16f, (suntop + 1) / 16f, (sunCenter + 1) / 16f);
        //Vector4 block = new((blockeast + 1) / 16f, (blocksouth + 1) / 16f, (blocktop + 1) / 16f, (blockCenter + 1) / 16f);// 1f);

        var sun = new Color(suneast / 15f, sunsouth / 15f, suntop / 15f, sunCenter / 15f);
        var block = new Vector4(blockeast / 15f, blocksouth / 15f, blocktop / 15f, blockCenter / 15f);// 1f);

        var light = new LightToken(global, sun, block);
        chunk.CacheLight(global, light);
        return light;
    }

    public void DrawMap(MapView viewport, ToolManager toolManager, UIManager ui, SceneState scene)
    {
        var map = viewport.Map;
        var camera = viewport.Camera;
        var gd = Game1.Instance.GraphicsDevice;
        if (map is null)
            return;
        var w = viewport.Width;
        var h = viewport.Height;
        if (this._lastWidth != w || this._lastHeight != h)
        {
            _lastWidth = w;
            _lastHeight = h;

            RecreateRenderTargets();
        }
        this.RenderTargets[0] = this.MapRender;
        this.RenderTargets[1] = this.MapDepth;
        this.RenderTargets[2] = this.MapLight;
        this.RenderTargets[3] = this.TextureFogWater;

        gd.SetRenderTargets(this.MapRender, this.MapDepth, this.MapLight, this.TextureFogWater);

        var a = EngineArgs.Default;

        gd.SetRenderTargets(null);
        gd.Clear(Color.Transparent);
        gd.RasterizerState = RasterizerState.CullNone;
        this.NewDraw(viewport, gd, a, scene, toolManager, ui);
    }
    Effect Shader;
    EffectTechnique TechniqueBlockHighlight => this.Shader.Techniques["BlockHighlight"];

    public RenderContext View { get; private set; }

    void SetInvariants()
    {
        var fx = Effect;
        var gd = Game1.Instance.GraphicsDevice;
        fx.Parameters["World"].SetValue(Matrix.Identity);
        fx.Parameters["BlockWidth"].SetValue(Block.Width);
        fx.Parameters["BlockHeight"].SetValue(Block.Height);
        fx.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);
        // TODO: pass a viewport's dimensions instead of the graphic's device backbuffer dimensions
        fx.Parameters["ViewportW"].SetValue(new Vector2(1, gd.Viewport.Width / (float)gd.Viewport.Height));

    }
    private void NewDraw(MapView view, GraphicsDevice gd, EngineArgs a, SceneState scene, ToolManager toolManager, UIManager ui)
    {
        var frame = SetupFrame(view, gd, a, scene, toolManager);
        //var fogColor = frame.FogColor;
        var map = view.Map;
        DrawnOnce = true;
        //var fogtxt = this.FogTexture;
        this.DrawOpaqueStage(gd, frame, view);
        this.DrawOverlayStage(toolManager, ui, map, frame);
        this.DrawTransparentStage(gd, frame, view);
        this.DrawCompositionStage(gd, frame, view);
        this.DrawEntityStage(gd, map, frame, scene);
        this.DrawParticleStage(frame);
        this.DrawHighlightStage(toolManager, frame);
        this.DrawFinalCompositionStage(gd, frame);//, fogColor, fogtxt);
        this.DrawUI(view, gd);
        this.PresentStage(gd, view, frame);
    }

    private void DrawFinalCompositionStage(GraphicsDevice gd, FrameData frame)//, Vector4 fogColor, Texture2D fogtxt)
    {
        this.DrawMapPreFogStage(gd, frame);//, fogColor);
        this.ComposeWaterPreFogStage(gd);
        this.ApplyFinalFogStage(gd, frame);//, fogColor, fogtxt);
    }

    private void DrawWorldOverlayStage(ToolManager toolManager, UIManager ui, MapBase map, RenderContext ctx, List<Entity> entities)
    {
        var fx = Effect;
        fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
        //gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true }; // this broke depth on block highlights
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        toolManager.DrawBeforeWorld(this.SpriteBatch, ctx);
        ui.DrawWorld(this.SpriteBatch, ctx);
        map.DrawBeforeWorld(this.SpriteBatch, ctx); // should i move this to draw right after the regular map drawing (specifically drawtransparent layers)
        foreach (var entity in entities)
            entity.DrawAfter(this.SpriteBatch, ctx); // cull non visible entities
    }

    private void ApplyFinalFogStage(GraphicsDevice gd, FrameData frame)//, Vector4 fogColor, Texture2D fogtxt)
    {
        var fx = Effect;
        var fogtxt = this.FogTexture;
        var fogColor = frame.FogColor;
        gd.SetRenderTargets(this.FinalScene);
        gd.Clear(new Color(fogColor));

        fx.CurrentTechnique = fx.Techniques["ApplyFog"];

        fx.Parameters["s"].SetValue(this.RenderBeforeFog);
        fx.Parameters["s1"].SetValue(this.FogBeforeFog);
        fx.Parameters["s2"].SetValue(fogtxt);
        fx.Parameters["s3"].SetValue(this.MapDepth);

        fx.CurrentTechnique.Passes["Pass1"].Apply();

        this.SpriteBatch.Draw(this.RenderBeforeFog, this.RenderBeforeFog.Bounds, gd.Viewport.Bounds, Color.White);
        this.SpriteBatch.Flush();
    }
    private void ComposeWaterPreFogStage(GraphicsDevice gd)
    {
        var fx = Effect;
        var fogtxt = this.FogTexture;

        fx.Parameters["s"].SetValue(this.WaterComposite);
        fx.Parameters["s1"].SetValue(this.WaterFog);
        fx.Parameters["s2"].SetValue(fogtxt);
        fx.Parameters["s3"].SetValue(this.WaterDepth);

        this.SpriteBatch.Draw(this.WaterComposite, this.WaterComposite.Bounds, gd.Viewport.Bounds, Color.White);
        fx.CurrentTechnique = fx.Techniques["Water"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        this.SpriteBatch.Flush();
    }

    private void DrawMapPreFogStage(GraphicsDevice gd, FrameData frame)//, Vector4 fogColor)
    {
        var fx = Effect;
        var fogtxt = this.FogTexture;
        var fogColor = frame.FogColor;
        gd.SetRenderTargets(this.RenderBeforeFog, this.FogBeforeFog);
        gd.Clear(new Color(fogColor));
        gd.Clear(ClearOptions.DepthBuffer, Color.White, 1, 1);

        fx.CurrentTechnique = fx.Techniques["RenderMapWithoutFog"];

        fx.Parameters["FogTextureSize"].SetValue(new Vector2(fogtxt.Width, fogtxt.Height));

        fx.Parameters["s"].SetValue(this.MapComposite);
        fx.Parameters["s1"].SetValue(this.TextureFogWater);
        fx.Parameters["s2"].SetValue(fogtxt);
        fx.Parameters["s3"].SetValue(this.MapDepth);

        gd.SamplerStates[2] = SamplerState.PointWrap;
        gd.DepthStencilState = DepthStencilState.Default;

        fx.CurrentTechnique.Passes["Pass1"].Apply();

        this.SpriteBatch.Draw(this.MapComposite, this.MapComposite.Bounds, gd.Viewport.Bounds, Color.White);
        this.SpriteBatch.Flush();
    }
    private void DrawEntityStage(GraphicsDevice gd, MapBase map, RenderContext ctx, SceneState scene, List<Entity> entities)
    {
        //SortEntities(ctx.View, this._visibleEntities);
        var fx = Effect;
        fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
        fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);
        DrawEntityShadowsNew(gd, ctx);
        //DrawEntitiesOnly(ctx, scene, objs);
        DrawEntitiesInternal(ctx, scene, entities);
        ApplyEntityFogPass(gd);
    }
    private void ApplyEntityFogPass(GraphicsDevice gd)
    {
        var fx = Effect;

        fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
        gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true };
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        gd.SetRenderTargets(this.MapComposite, this.TextureFogWater, this.MapDepth);
        this.SpriteBatch.Flush();
    }
    private void DrawEntityShadowsNew(GraphicsDevice gd, RenderContext ctx)
    {
        MySpriteBatch shadowsSB = new MySpriteBatch(gd);

        Effect.CurrentTechnique = Effect.Techniques["EntityShadows"];
        gd.DepthStencilState = new DepthStencilState { DepthBufferWriteEnable = false };

        Effect.CurrentTechnique.Passes["Pass1"].Apply();

        SpriteComp.DrawShadows(shadowsSB, ctx);

        gd.SetRenderTarget(this.MapComposite);
        shadowsSB.Flush();
    }
    private void SetupPlayerShaderParams(
    MapBase map,
    MapView view,
    Effect fx,
    RenderContext ctx)
    {
        var actor = map.Net.GetPlayer()?.ControllingEntity;
        if (actor == null || !actor.Exists)
            return;

        var sprite = actor.GetSprite();
        var spriteBounds = sprite.GetBounds();

        var screenBounds = ctx.View.GetScreenBounds(actor.Global, spriteBounds);

        var w = view.Width;
        var h = view.Height;

        float xxx = screenBounds.X / (float)w - .5f;
        float yyy = screenBounds.Y / (float)h - .5f;
        float www = (screenBounds.X + screenBounds.Width) / (float)w - .5f;
        float hhh = (screenBounds.Y + screenBounds.Height) / (float)h - .5f;

        xxx = -.1f * view.Zoom;
        yyy = -.15f * view.Zoom;
        www = .1f * view.Zoom;
        hhh = .15f * view.Zoom;

        fx.Parameters["PlayerBoundingBox"].SetValue(new Vector4(xxx, yyy, www, hhh));
        fx.Parameters["PlayerDepth"].SetValue(ctx.View.GetDrawDepth(actor.Global));
    }
    private void DrawOpaquePass(RenderContext ctx, IEnumerable<Chunk> visibleChunks)
    {
        Effect.Parameters["HideWalls"].SetValue(ctx.View.HideWalls);
        foreach (var chunk in visibleChunks)
        {
            // TODO: DONT BUILD TOP SLICE TWICE!
            if (!chunk.Valid)
                chunk.Build(ctx);
            this.ApplyChunkTransform(chunk, ctx.View);
 
            Effect.CurrentTechnique.Passes["Pass1"].Apply();

            chunk.DrawOpaqueLayers(ctx.View); // TODO: is it faster to pass only the effectparameters?
        }
    }
    private void DrawTransparentPass(
    GraphicsDevice gd,
    MapView view,
    RenderContext ctx,
    List<Chunk> visibleChunks,
    Color clearcol)
    {
        var fx = Effect;

        this.SetBlockTextures();

        fx.CurrentTechnique = fx.Techniques["CombinedWater"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();

        gd.SetRenderTargets(this.WaterRender, this.WaterLight, this.WaterDepth, this.WaterFog);
        gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearcol, 1, 1);

        PrepareShaderNew(view, "CombinedWater");

        foreach (var chunk in visibleChunks)
        {
            if (!chunk.Valid)
                continue;

            chunk.DrawTransparentLayers(this, ctx.View, Effect);
        }
    }

    private void ComposeMapPass(
        GraphicsDevice gd,
        MapView view,
        Vector4 fogColor,
        Vector2 fogoffset)
    {
        var fx = Effect;
        var cameraPos = view.Position;

        gd.SetRenderTarget(this.MapComposite);
        gd.Clear(new Color(fogColor));

        fx.Parameters["s"].SetValue(this.MapRender);
        fx.Parameters["s1"].SetValue(this.MapLight);
        fx.Parameters["s2"].SetValue(this.MapDepth);
        fx.Parameters["s3"].SetValue(this.TextureFogWater);

        var watertxt = this.WaterTexture;// Game1.Instance.Content.Load<Texture2D>("Graphics/watersmallpixely");
        fx.Parameters["s4"].SetValue(watertxt);
        //fx.Parameters["s5"].SetValue(this.MapNormals);

        gd.SamplerStates[4] = SamplerState.PointWrap;
        fx.Parameters["WaterTextureSize"].SetValue(
            new Vector2(watertxt.Width, watertxt.Height));

        var fog = view.FogT;
        var offset2 = new Vector2(0, .5f + fog / 100f);

        var wateroffset = (cameraPos / watertxt.Width).ToFloored() * watertxt.Width;
        wateroffset = (cameraPos - wateroffset) / watertxt.Width;
        fx.Parameters["WaterOffset"].SetValue(fogoffset - wateroffset);

        var wateroffset2 = (cameraPos / watertxt.Height).ToFloored() * watertxt.Height;
        wateroffset = (cameraPos - wateroffset) / watertxt.Height;
        fx.Parameters["WaterOffset2"].SetValue(offset2 - wateroffset);

        this.SpriteBatch.Draw(
            this.MapRender,
            this.MapRender.Bounds,
            gd.Viewport.Bounds,
            Color.White);

        fx.CurrentTechnique = fx.Techniques["FinalInsideBorders"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();

        this.SpriteBatch.Flush();
    }
    private void ComposeWaterPass(
    GraphicsDevice gd,
    MapView view)
    {
        var fx = Effect;

        gd.SetRenderTarget(this.WaterComposite);
        gd.Clear(Color.Transparent);

        //var fogtxt = this.FogTexture;// Game1.Instance.Content.Load<Texture2D>("Graphics/Fog04");
        var watertxt = this.WaterTexture;// Game1.Instance.Content.Load<Texture2D>("Graphics/watersmallpixely");

        fx.Parameters["s"].SetValue(this.WaterRender);
        fx.Parameters["s1"].SetValue(this.WaterLight);
        fx.Parameters["s2"].SetValue(this.WaterDepth);
        fx.Parameters["s3"].SetValue(this.WaterFog);
        fx.Parameters["s4"].SetValue(watertxt);

        gd.SamplerStates[4] = SamplerState.PointWrap;
        fx.Parameters["WaterTextureSize"].SetValue(
            new Vector2(watertxt.Width, watertxt.Height));

        this.SpriteBatch.Draw(
            this.WaterRender,
            this.WaterRender.Bounds,
            gd.Viewport.Bounds,
            Color.White);

        fx.CurrentTechnique = fx.Techniques["CompositeWater"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();

        this.SpriteBatch.Flush();
    }

    //private void DrawEntityAndOverlayPass(
    //MapView view,
    //RenderContext ctx,
    //SceneState scene,
    //ToolManager toolManager,
    //UIManager ui)
    //{
    //    var map = view.Map;
    //    var objs = map.Entities.ToList();

    //    // keep deterministic ordering
    //    this.SortEntities(view);

    //    // pre-world tool effects (ghost placement, previews)
    //    toolManager.DrawBeforeWorld(this.SpriteBatch, ctx);

    //    // world-space UI overlays (selection boxes, etc.)
    //    ui.DrawWorld(this.SpriteBatch, ctx);

    //    // map-level pre entity overlays
    //    map.DrawBeforeWorld(this.SpriteBatch, ctx);

    //    // main entity draw
    //    foreach (var entity in objs)
    //    {
    //        if (entity.Global.Z > this.MaxDrawZ + 1)
    //            continue;

    //        var bounds = entity.GetScreenBounds(ctx);

    //        if (!ctx.Viewport.Intersects(bounds))
    //            continue;

    //        entity.Draw(this.SpriteBatch, ctx);
    //        scene.ObjectsDrawn.Add(entity);
    //    }

    //    this.SpriteBatch.Flush();

    //    // post-world tool rendering
    //    toolManager.DrawAfterWorld(this.SpriteBatch, ctx);

    //    // mouseover highlight (final overlay)
    //    if (toolManager.ActiveTool?.Target?.Object is GameObject mouseover && mouseover.Exists)
    //    {
    //        mouseover.DrawMouseover(this.SpriteBatch, ctx);
    //    }

    //    this.SpriteBatch.Flush();
    //}
    private void DrawParticleStage(MapBase map, RenderContext ctx)
    {
        map.DrawParticles(ctx);

        Effect.CurrentTechnique = Effect.Techniques["Particles"];
        Effect.Parameters["s"].SetValue(Block.Atlas.Texture);
        Effect.CurrentTechnique.Passes["Pass1"].Apply();

        BlockParticlesSpriteBatch.Flush();

        Effect.Parameters["s"].SetValue(Sprite.Atlas.Texture);
        //this.Effect.CurrentTechnique.Passes["Pass1"].Apply(); // no need to apply after changing parameter unless alss changing technique??

        this.ParticlesSpriteBatch.Flush();
    }
    private static List<Chunk> GetVisibleChunks(MapView view, MapBase map)
        => [.. map.GetActiveChunks().Values.Where(ch=> view.Viewport.Intersects(ch.GetScreenBounds(view)))];
    

    private void SetupRenderTargets(GraphicsDevice gd, Color clearcol)
    {
        gd.SetRenderTargets(this.TextureFogWater);
        gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1, 1);
        // PROBLEM clearing the texturefogwater with the same parameters as the other rendertargets, causes the problem with the background being drawn over the toolmanager preview blocks
        gd.SetRenderTargets(this.MapRender, this.MapLight, this.MapDepth, this.MapNormals);
        //var clearcol = new Color(1f, 1f, 0, 0); // 3rd component is 0 in order to not draw water on background
        gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearcol, 1, 1);

        // after clearing each target with the appropriate parameters for each one, set them all together
        gd.SetRenderTargets(this.MapRender, this.MapLight, this.MapDepth, this.TextureFogWater, this.MapNormals);
    }

    private void EnsureSpritebatches(GraphicsDevice gd)
    {
        this.SpriteBatch ??= new MySpriteBatch(gd);
        this.WaterSpriteBatch ??= new MySpriteBatch(gd);
        this.ParticlesSpriteBatch ??= new MySpriteBatch(gd);
        this.TransparentBlocksSpriteBatch ??= new MySpriteBatch(gd);
        this.BlockParticlesSpriteBatch ??= new MySpriteBatch(gd);
    }

    private void SetupVisibility(MapView view, float zoom)
    {
        // VISIBILITY
        this.MaxDrawZ = view.GetMaxDrawLevel();
        Effect.Parameters["MaxDrawLevel"].SetValue(this.MaxDrawZ);
        Effect.Parameters["HideWalls"].SetValue(Engine.HideWalls);
        Effect.Parameters["OcclusionRadius"].SetValue(.01f * zoom * zoom);
    }

    private static void SetupFollowingEntityParams(MapView view, Camera camera, int w, int h, Effect fx, out double rotCos, out double rotSin)
    {
        // SET PARAMETERS IF CAMERA IS FOLLOWING AN ENTITY (for occluding & cutout)
        rotCos = camera.RotCos;
        rotSin = camera.RotSin;
        var followTarget = view.Following;
        fx.Parameters["PlayerOcclusion"].SetValue(followTarget != null);
        fx.Parameters["PlayerGlobal"].SetValue(followTarget != null ? followTarget.Global : Vector3.Zero);
        if (followTarget != null)
        {
            fx.Parameters["PlayerRotXY"].SetValue((float)(
                followTarget.Global.X * rotCos - followTarget.Global.Y * rotSin +
                followTarget.Global.X * rotSin + followTarget.Global.Y * rotCos));
            fx.Parameters["PlayerCenterOffset"].SetValue(
                //camera.GetScreenPositionFloat(followTarget.Global + followTarget.Physics.Height * Vector3.UnitZ / 2) / new Vector2(w, h) - Vector2.One * .5f);
                view.GetScreenPositionFloat(followTarget.Global + followTarget.Physics.Height * Vector3.UnitZ / 2) / new Vector2(w, h) - Vector2.One * .5f);
        }
    }

    private static void SetupGlobalShaderParams(float zoom, Vector2 viewportDimensions, Effect fx)
    {
        fx.Parameters["Viewport"].SetValue(viewportDimensions);
        fx.Parameters["Zoom"].SetValue(zoom);
        fx.Parameters["AtlasWidth"].SetValue(Block.Atlas.Texture.Width);
        fx.Parameters["AtlasHeight"].SetValue(Block.Atlas.Texture.Height);
    }

    private static void SetupOutlines(float zoom, int w, int h, Effect fx)
    {
        float borderPx = 1;
        var borderResolution = new Vector2(borderPx / w, borderPx / h) * zoom;
        fx.Parameters["BorderResolution"].SetValue(borderResolution);
    }

    private void SetupFogAndLight(ToolManager toolManager, MapBase map, Vector2 cameraPos, RenderContext ctx, Effect fx, out Vector4 fogColor, out float fog, out Vector2 fogoffset)
    {
        // FOG, & AMBIENT COLOR
        var nightAmount = (float)map.GetDayTimeNormal();
        var ambientColor = Color.Lerp(Color.White, map.GetAmbientColor(), nightAmount);
        ambientColor = map.GetAmbientColor();

        var ambient = ambientColor.ToVector4();
        fx.Parameters["AmbientLight"].SetValue(ambient);

        fogColor = this.FogColor;
        // choose between ambient or black background color
        fogColor = Color.Lerp(new Color(fogColor), Color.Black, nightAmount).ToVector4();
        fx.Parameters["FogColor"].SetValue(fogColor);
        fx.Parameters["FogDistance"].SetValue(FogFadeLength);

        // FOG
        fog = ctx.View.FogT;
        fogoffset = new Vector2(fog / 100f, 0);
        fx.Parameters["FogOffset"].SetValue(fogoffset - cameraPos / 1000f);
        if (toolManager.ActiveTool.Target != null && toolManager.ActiveTool.Target.Type != TargetType.Null)
        {
            this.LastZTarget = toolManager.ActiveTool.Target.Global.Z;
            fx.Parameters["FogZ"].SetValue(toolManager.ActiveTool.Target.Global.Z - FogZOffset);
        }
        fx.Parameters["FogEnabled"].SetValue(Fog);
        fx.Parameters["FogLevel"].SetValue(this.FogLevel);
    }


    //private void NewDraw(MapView viewport, GraphicsDevice gd, EngineArgs a, SceneState scene, ToolManager toolManager, UIManager ui)
    //{
    //    var map = viewport.Map;
    //    var camera = viewport.Camera;
    //    var zoom = camera.Zoom;
    //    var coordinates = camera.Coordinates;
    //    var w = viewport.Width;
    //    var h = viewport.Height;
    //    RenderContext ctx = BeginContext(viewport);

    //    DrawnOnce = true;
    //    Effect fx = Game1.Instance.Content.Load<Effect>("blur");
    //    this.Effect = fx;


    //    var world = Matrix.Identity;
    //    var view =
    //        new Matrix(
    //           1.0f, 0.0f, 0.0f, 0.0f,
    //           0.0f, -1.0f, 0.0f, 0.0f,
    //           0.0f, 0.0f, -1.0f, 0.0f,
    //           0.0f, 0.0f, 0.0f, 1.0f);
    //    var projection = Matrix.CreateOrthographicOffCenter(
    //        0, w, -h, 0, 0, 1);

    //    fx.Parameters["World"].SetValue(Matrix.Identity);

    //    fx.Parameters["BlockWidth"].SetValue(Block.Width);
    //    fx.Parameters["BlockHeight"].SetValue(Block.Height);
    //    fx.Parameters["AtlasWidth"].SetValue(Block.Atlas.Texture.Width);
    //    fx.Parameters["AtlasHeight"].SetValue(Block.Atlas.Texture.Height);
    //    fx.Parameters["Viewport"].SetValue(new Vector2(gd.Viewport.Width, gd.Viewport.Height));
    //    fx.Parameters["ViewportW"].SetValue(new Vector2(1, gd.Viewport.Width / (float)gd.Viewport.Height));
    //    fx.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);
    //    fx.Parameters["Zoom"].SetValue(zoom);
    //    float borderPx = 1;
    //    fx.Parameters["BorderResolution"].SetValue(new Vector2(borderPx / gd.Viewport.Width, borderPx / gd.Viewport.Height) * zoom);
    //    var nightAmount = (float)map.GetDayTimeNormal();
    //    Color ambientColor = Color.Lerp(Color.White, map.GetAmbientColor(), nightAmount);
    //    ambientColor = map.GetAmbientColor();

    //    Vector4 ambient = ambientColor.ToVector4();
    //    fx.Parameters["AmbientLight"].SetValue(ambient);
    //    var fogColor = this.FogColor;
    //    // choose between ambient or black background color
    //    fogColor = Color.Lerp(new Color(fogColor), Color.Black, nightAmount).ToVector4();
    //    fx.Parameters["FogColor"].SetValue(fogColor);
    //    fx.Parameters["FogDistance"].SetValue(FogFadeLength);

    //    var fog = ctx.View.FogT;
    //    var fogoffset = new Vector2(fog / 100f, 0);
    //    fx.Parameters["FogOffset"].SetValue(fogoffset - coordinates / 1000f);
    //    if (toolManager.ActiveTool.Target != null && toolManager.ActiveTool.Target.Type != TargetType.Null)
    //    {
    //        this.LastZTarget = toolManager.ActiveTool.Target.Global.Z;
    //        fx.Parameters["FogZ"].SetValue(toolManager.ActiveTool.Target.Global.Z - FogZOffset);
    //    }
    //    fx.Parameters["FogEnabled"].SetValue(Fog);

    //    var rotCos = camera.RotCos;
    //    var rotSin = camera.RotSin;
    //    var followTarget = viewport.Following;
    //    fx.Parameters["PlayerOcclusion"].SetValue(followTarget != null);
    //    fx.Parameters["PlayerGlobal"].SetValue(followTarget != null ? followTarget.Global : Vector3.Zero);
    //    if (followTarget != null)
    //    {
    //        fx.Parameters["PlayerRotXY"].SetValue((float)(
    //            followTarget.Global.X * rotCos - followTarget.Global.Y * rotSin +
    //            followTarget.Global.X * rotSin + followTarget.Global.Y * rotCos));
    //        fx.Parameters["PlayerCenterOffset"].SetValue(
    //            //camera.GetScreenPositionFloat(followTarget.Global + followTarget.Physics.Height * Vector3.UnitZ / 2) / new Vector2(w, h) - Vector2.One * .5f);
    //            viewport.GetScreenPositionFloat(followTarget.Global + followTarget.Physics.Height * Vector3.UnitZ / 2) / new Vector2(w, h) - Vector2.One * .5f);
    //    }

    //    fx.Parameters["FogLevel"].SetValue(this.FogLevel);
    //    this.MaxDrawZ = viewport.GetMaxDrawLevel();
    //    this.Effect.Parameters["FogEnabled"].SetValue(Fog);
    //    this.Effect.Parameters["MaxDrawLevel"].SetValue(this.MaxDrawZ);
    //    this.Effect.Parameters["HideWalls"].SetValue(Engine.HideWalls);
    //    this.Effect.Parameters["OcclusionRadius"].SetValue(.01f * zoom * zoom);

    //    gd.DepthStencilState = DepthStencilState.Default;

    //    gd.SamplerStates[0] = SamplerState.PointClamp;
    //    gd.SamplerStates[1] = SamplerState.PointClamp;
    //    gd.SamplerStates[2] = SamplerState.PointClamp;
    //    gd.SamplerStates[3] = SamplerState.PointClamp;

    //    if (this.SpriteBatch == null)
    //        this.SpriteBatch = new MySpriteBatch(gd);

    //    if (this.WaterSpriteBatch == null)
    //        this.WaterSpriteBatch = new MySpriteBatch(gd);

    //    if (this.ParticlesSpriteBatch == null)
    //        this.ParticlesSpriteBatch = new MySpriteBatch(gd);

    //    if (this.TransparentBlocksSpriteBatch == null)
    //        this.TransparentBlocksSpriteBatch = new MySpriteBatch(gd);

    //    if (this.BlockParticlesSpriteBatch == null)
    //        this.BlockParticlesSpriteBatch = new MySpriteBatch(gd);

    //    var clearcol = new Color(1f, 1f, 1f, 0); // if i put 1 for the alpha than tsansparent blocks will be shaded white  // (old comment) i put 1 again because i dont draw water on the fog texture after all
    //    //var clearcol = new Color(1f, 1f, 1f, 1f); // causes unhandled white background

    //    gd.SetRenderTargets(this.TextureFogWater);
    //    gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1, 1);
    //    // PROBLEM clearing the texturefogwater with the same parameters as the other rendertargets, causes the problem with the background being drawn over the toolmanager preview blocks
    //    gd.SetRenderTargets(this.MapRender, this.MapLight, this.MapDepth);
    //    //var clearcol = new Color(1f, 1f, 0, 0); // 3rd component is 0 in order to not draw water on background
    //    gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearcol, 1, 1);

    //    // after clearing each target with the appropriate parameters for each one, set them all together
    //    gd.SetRenderTargets(this.MapRender, this.MapLight, this.MapDepth, this.TextureFogWater);

    //    // use new technique to draw both color and light in one pass in multiple rendertargets
    //    fx.CurrentTechnique = fx.Techniques["Combined"];
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    SetBlockTextures(fx);

    //    this.DepthNear = float.MinValue;
    //    this.DepthFar = float.MaxValue;

    //    this.Effect.Parameters["RotCos"].SetValue((float)rotCos);
    //    this.Effect.Parameters["RotSin"].SetValue((float)rotSin);

    //    var actor = map.Net.GetPlayer()?.ControllingEntity;
    //    if (actor != null)
    //    {
    //        if (actor.Exists)
    //        {
    //            var sprite = actor.GetSprite();
    //            var spriteBounds = sprite.GetBounds();
    //            //Rectangle screenBounds = camera.GetScreenBounds(actor.Global, spriteBounds);
    //            var screenBounds = ctx.View.GetScreenBounds(actor.Global, spriteBounds);
    //            var xxx = screenBounds.X / (float)w - .5f;
    //            var yyy = screenBounds.Y / (float)h - .5f;
    //            var www = (screenBounds.X + screenBounds.Width) / (float)w - .5f;
    //            var hhh = (screenBounds.Y + screenBounds.Height) / (float)h - .5f;
    //            xxx = -.1f * zoom;
    //            yyy = -.15f * zoom;
    //            www = .1f * zoom;
    //            hhh = .15f * zoom;
    //            var box = new Vector4(xxx, yyy, www, hhh);
    //            this.Effect.Parameters["PlayerBoundingBox"].SetValue(box);
    //            //var d = actor.Global.GetDrawDepth(map, camera);
    //            var d = ctx.View.GetDrawDepth(actor.Global);
    //            this.Effect.Parameters["PlayerDepth"].SetValue(d);
    //        }
    //    }

    //    this.PrepareShader(viewport);

    //    var visibleChunks = (from ch in map.GetActiveChunks().Values where viewport.Viewport.Intersects(ch.GetScreenBounds(viewport)) select ch);

    //    foreach (var chunk in visibleChunks)
    //    {
    //        // TODO: DONT BUILD TOP SLICE TWICE!
    //        if (!chunk.Valid)
    //            chunk.Build(ctx);

    //        chunk.DrawOpaqueLayers(this, ctx.View, this.Effect); // TODO: is it faster to pass only the effectparameters?
    //        continue;
    //    }
    //    //this.TopSliceChanged = false;

    //    // TODO: these temporarily only work with static maps
    //    this.DepthNear = viewport.GetNearDepth();
    //    this.DepthFar = viewport.GetFarDepth();

    //    fx.Parameters["FarDepth"].SetValue(this.DepthFar);
    //    fx.Parameters["NearDepth"].SetValue(this.DepthNear);
    //    var dd = this.DepthNear - this.DepthFar;
    //    fx.Parameters["DepthResolution"].SetValue(2 / dd);
    //    fx.Parameters["OutlineThreshold"].SetValue(1 / dd);

    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    this.SpriteBatch.Flush();

    //    //var objs = map.GetEntities().ToList();
    //    var objs = map.Entities.ToList();

    //    fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
    //    //gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true }; // this broke depth on block highlights
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();

    //    toolManager.DrawBeforeWorld(this.SpriteBatch, ctx);

    //    fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    ui.DrawWorld(this.SpriteBatch, ctx);
    //    map.DrawBeforeWorld(this.SpriteBatch, ctx); // should i move this to draw right after the regular map drawing (specifically drawtransparent layers)
    //                                                     // so that designation manager can draw designations with correct transparency?
    //    foreach (var entity in objs)
    //        entity.DrawAfter(this.SpriteBatch, ctx); // cull non visible entities

    //    this.SpriteBatch.Flush();

    //    SetBlockTextures(fx);
    //    fx.CurrentTechnique = fx.Techniques["CombinedWater"];
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();

    //    gd.SetRenderTargets(this.WaterRender, this.WaterLight, this.WaterDepth, this.WaterFog);
    //    gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearcol, 1, 1);

    //    this.PrepareShaderTransparent(viewport);

    //    foreach (var chunk in visibleChunks)
    //    {
    //        if (!chunk.Valid)
    //            continue;
    //        chunk.DrawTransparentLayers(this, ctx.View, this.Effect);
    //    }

    //    // combine scenes and apply ambient light
    //    gd.SetRenderTarget(this.MapComposite);

    //    gd.Clear(new Color(fogColor));

    //    fx.Parameters["s"].SetValue(this.MapRender);
    //    fx.Parameters["s1"].SetValue(this.MapLight);
    //    fx.Parameters["s2"].SetValue(this.MapDepth);
    //    fx.Parameters["s3"].SetValue(this.TextureFogWater);

    //    var watertxt = Game1.Instance.Content.Load<Texture2D>("Graphics/watersmallpixely");
    //    fx.Parameters["s4"].SetValue(watertxt);

    //    gd.SamplerStates[4] = SamplerState.PointWrap;
    //    fx.Parameters["WaterTextureSize"].SetValue(new Vector2(watertxt.Width, watertxt.Height));

    //    //var offset2 = new Vector2(0, .5f + this.FogT / 100f);
    //    var offset2 = new Vector2(0, .5f + fog / 100f);

    //    var wateroffset = (coordinates / (watertxt.Width)).ToFloored() * (watertxt.Width);
    //    wateroffset = (coordinates - wateroffset) / (watertxt.Width);
    //    fx.Parameters["WaterOffset"].SetValue(fogoffset - wateroffset);

    //    var wateroffset2 = (coordinates / (watertxt.Height)).ToFloored() * (watertxt.Height);
    //    wateroffset = (coordinates - wateroffset) / (watertxt.Height);
    //    fx.Parameters["WaterOffset2"].SetValue(offset2 - wateroffset);

    //    this.SpriteBatch.Draw(this.MapRender, this.MapRender.Bounds, gd.Viewport.Bounds, Color.White);
    //    fx.CurrentTechnique = fx.Techniques["FinalInsideBorders"];
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    this.SpriteBatch.Flush();

    //    gd.SetRenderTarget(this.WaterComposite);
    //    gd.Clear(Color.Transparent);

    //    fx.Parameters["s"].SetValue(this.WaterRender);
    //    fx.Parameters["s1"].SetValue(this.WaterLight);
    //    fx.Parameters["s2"].SetValue(this.WaterDepth);
    //    fx.Parameters["s3"].SetValue(this.WaterFog); //it's pink/purple before in the shader i write both red values for the fog and blue values for the water

    //    fx.Parameters["s4"].SetValue(watertxt);

    //    gd.SamplerStates[4] = SamplerState.PointWrap;
    //    fx.Parameters["WaterTextureSize"].SetValue(new Vector2(watertxt.Width, watertxt.Height));
    //    this.SpriteBatch.Draw(this.WaterRender, this.WaterRender.Bounds, gd.Viewport.Bounds, Color.White);
    //    // TODO: Must draw entities before final composition, so fog is applied over them accordingly
    //    fx.CurrentTechnique = fx.Techniques["CompositeWater"];
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();

    //    this.SpriteBatch.Flush();

    //    //sort objects back to front for proper semitraspanrent rendering
    //    // TODO: culling
    //    this.SortEntities(ctx.View, objs);
    //    // TODO: have the particle manager set textures because different emitters might use different atlases (blocks vs entities)
    //    fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
    //    fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);

    //    this.DrawEntities(ctx, scene, objs);
    //    //map.DrawParticles(this);
    //    //  // draw entity shadows
    //    MySpriteBatch shadowsSB = new MySpriteBatch(gd);
    //    fx.CurrentTechnique = fx.Techniques["EntityShadows"];
    //    gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    SpriteComp.DrawShadows(shadowsSB, ctx);
    //    gd.SetRenderTarget(this.MapComposite);
    //    shadowsSB.Flush();

    //    // flush entity spritebatch after shadows so they get drawn above them
    //    fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
    //    gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true };
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    gd.SetRenderTargets(this.MapComposite, this.TextureFogWater, this.MapDepth);
    //    this.SpriteBatch.Flush();

    //    //  draw particles drawn by entities
    //    map.DrawParticles(ctx);
    //    fx.CurrentTechnique = fx.Techniques["Particles"];
    //    //fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    fx.Parameters["s"].SetValue(Block.Atlas.Texture);
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();

    //    this.BlockParticlesSpriteBatch.Flush();
    //    fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();

    //    this.ParticlesSpriteBatch.Flush();

    //    // draw block mouseover highlight, here or after fog?
    //    // set textures here or in tool draw method?
    //    // DRAW here things such as entity previews for debug spawning
    //    fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
    //    fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);

    //    fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
    //    //gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true }; // this broke depth on block highlights
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    toolManager.DrawAfterWorld(this.SpriteBatch, ctx);

    //    this.SpriteBatch.Flush();

    //    // draw entity mouseover highlight
    //    fx.CurrentTechnique = fx.Techniques["EntityMouseover"];
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    if (toolManager.ActiveTool is not null)
    //        if (toolManager.ActiveTool.Target is not null)
    //            if (toolManager.ActiveTool.Target.Object is GameObject mouseover && mouseover.Exists)
    //                mouseover.DrawMouseover(this.SpriteBatch, ctx);

    //    this.SpriteBatch.Flush();

    //    // draw non-water on pre-final texture
    //    gd.SetRenderTargets(this.RenderBeforeFog, this.FogBeforeFog);
    //    gd.Clear(new Color(fogColor));
    //    gd.Clear(ClearOptions.DepthBuffer, Color.White, 1, 1);
    //    fx.CurrentTechnique = fx.Techniques["RenderMapWithoutFog"];
    //    var fogtxt = Game1.Instance.Content.Load<Texture2D>("Graphics/Fog04");
    //    fx.Parameters["FogTextureSize"].SetValue(new Vector2(fogtxt.Width, fogtxt.Height));
    //    gd.SamplerStates[2] = SamplerState.PointWrap;

    //    fx.Parameters["s"].SetValue(this.MapComposite);
    //    fx.Parameters["s1"].SetValue(this.TextureFogWater);
    //    fx.Parameters["s2"].SetValue(fogtxt);
    //    fx.Parameters["s3"].SetValue(this.MapDepth);

    //    this.SpriteBatch.Draw(this.MapComposite, this.MapComposite.Bounds, gd.Viewport.Bounds, Color.White);
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    gd.DepthStencilState = DepthStencilState.Default;
    //    this.SpriteBatch.Flush();

    //    // draw water on pre-final texture

    //    fx.Parameters["s"].SetValue(this.WaterComposite);
    //    fx.Parameters["s1"].SetValue(this.WaterFog);
    //    fx.Parameters["s2"].SetValue(fogtxt);
    //    fx.Parameters["s3"].SetValue(this.WaterDepth);

    //    this.SpriteBatch.Draw(this.WaterComposite, this.WaterComposite.Bounds, gd.Viewport.Bounds, Color.White);
    //    fx.CurrentTechnique = fx.Techniques["Water"];
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    this.SpriteBatch.Flush();

    //    // draw block highlight now so it's correctly placed over water according to depth
    //    fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
    //    fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);
    //    // which textures to use???
    //    fx.Parameters["s"].SetValue(Block.Atlas.Texture);
    //    fx.Parameters["s1"].SetValue(Block.Atlas.DepthTexture);
    //    // apply fog to the pre-final texture render(that contains map + water)
    //    gd.SetRenderTargets(this.FinalScene);
    //    gd.Clear(new Color(fogColor));

    //    fx.Parameters["s"].SetValue(this.RenderBeforeFog);
    //    fx.Parameters["s1"].SetValue(this.FogBeforeFog);
    //    fx.Parameters["s2"].SetValue(fogtxt);
    //    fx.Parameters["s3"].SetValue(this.MapDepth);

    //    this.SpriteBatch.Draw(this.RenderBeforeFog, this.RenderBeforeFog.Bounds, gd.Viewport.Bounds, Color.White);
    //    fx.CurrentTechnique = fx.Techniques["ApplyFog"];
    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    this.SpriteBatch.Flush();

    //    ///test
    //    ///i moved this here from ingame.cs's draw method
    //    var sb = new SpriteBatch(gd);
    //    gd.SetRenderTarget(this.FinalScene);

    //    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
    //    map.DrawInterface(sb, viewport);
    //    sb.End();
    //    ///

    //    // draw final scene to backbuffer
    //    RenderTarget2D[] targets = new RenderTarget2D[] {
    //        this.FinalScene,
    //        this.RenderBeforeFog,
    //        this.FogBeforeFog,
    //        this.WaterRender,
    //        this.WaterDepth,
    //        this.WaterLight,
    //        this.WaterFog,
    //        this.WaterComposite,
    //        this.MapRender,
    //        this.MapDepth,
    //        this.MapLight,
    //        this.TextureFogWater };
    //    this.RenderTargets = targets.ToArray();
    //    gd.SetRenderTarget(null);


    //    fx.Parameters["s"].SetValue(this.RenderTargets[this.RenderIndex]);
    //    fx.CurrentTechnique = fx.Techniques["Normal"];

    //    fx.CurrentTechnique.Passes["Pass1"].Apply();
    //    //this.SpriteBatch.Draw(this.FinalScene, this.FinalScene.Bounds, gd.Viewport.Bounds, Color.White);
    //    var vp = gd.Viewport.Bounds;
    //    //vp.Width = (int)(vp.Width / this.Zoom);
    //    //vp.Height = (int)(vp.Height / this.Zoom);
    //    var fc = this.FinalScene.Bounds;
    //    //fc.Width = (int)(fc.Width / 4);
    //    //fc.Height = (int)(fc.Height / 4);
    //    this.SpriteBatch.Draw(this.FinalScene, fc, vp, Color.White);

    //    /// added this here to draw the final scene with depth, but i have to change the shader to read depth from the depth texture
    //    //gd.DepthStencilState = DepthStencilState.Default;

    //    this.SpriteBatch.Flush();

    //    // draw ui and other elements
    //    map.DrawWorld(this.SpriteBatch, viewport);
    //    this.SpriteBatch.Flush();
    //}




    RenderContext _currentCtx;
    private RenderContext BeginContext(MapView viewport)
    {
        var ctx = new RenderContext
        {
            View = viewport,
            Map = viewport.Map,
            Camera = viewport.Camera,
            Origin = viewport.Camera.Position - new Vector2(viewport.Width, viewport.Height) / 2 / viewport.Camera.Zoom,
            Renderer = this,
            Viewport = viewport.Viewport
        };
        this._currentCtx = ctx;
        return ctx;
    }

    private void SetBlockTextures()
    {
        var fx = Effect;
        fx.Parameters["s"].SetValue(Block.Atlas.Texture);
        fx.Parameters["s2"].SetValue(Block.Atlas.NormalTexture);
        fx.Parameters["s3"].SetValue(Block.Atlas.DepthTexture);
    }

    //private void DrawEntitiesOnly(RenderContext ctx, SceneState scene, List<Entity> objs)
    //{
    //    foreach (var obj in objs)
    //    {
    //        if (obj.Global.Z > this.MaxDrawZ + 1)
    //            continue;

    //        // TODO: check bounding box intersection instead of single point to avoid entity pop-in
    //        var bounds = obj.GetScreenBounds(ctx); // TODO: cache bounds?
    //        if (!ctx.Viewport.Intersects(bounds))
    //            continue;

    //        obj.Draw(this.SpriteBatch, ctx);
    //        scene.ObjectsDrawn.Add(obj);
    //    }
    //}
    private List<Entity> CullEntities(RenderContext ctx, IEnumerable<Entity> objs)
    {
        var result = new List<Entity>();

        foreach (var obj in objs)
        {
            if (obj.Global.Z > this.MaxDrawZ + 1)
                continue;

            var bounds = obj.GetScreenBounds(ctx);
            if (!ctx.Viewport.Intersects(bounds))
                continue;

            result.Add(obj);
        }

        return result;
    }
    private void DrawEntitiesInternal(RenderContext ctx, SceneState scene, List<Entity> entities)
    {
        foreach (var obj in entities)
        {
            obj.Draw(this.SpriteBatch, ctx);
            scene.ObjectsDrawn.Add(obj);
        }
    }
    private void SortEntities(MapView view, List<Entity> entities)
        => entities.Sort((a, b) =>
            view.GetDrawDepth(a.Global).CompareTo(view.GetDrawDepth(b.Global)));
    public void NewDraw(RenderTarget2D target, MapView viewport, GraphicsDevice gd, EngineArgs a, SceneState scene, ToolManager toolManager)
    {


        this.MapRender ??= new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);
        this.MapDepth ??= new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);
        this.MapLight ??= new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);
        //this.MapNormals ??= new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);

        var fx = Effect;// Game1.Instance.Content.Load<Effect>("blur");
        var map = viewport.Map;
        var camera = viewport.Camera;
        var zoom = camera.Zoom;

        var ctx = BeginContext(viewport);

        fx.Parameters["BlockWidth"].SetValue(Block.Width + 2 * BordersEffect.Thickness);
        fx.Parameters["BlockHeight"].SetValue(Block.Height + 2 * BordersEffect.Thickness);
        fx.Parameters["AtlasWidth"].SetValue(Block.Atlas.Texture.Width);
        fx.Parameters["AtlasHeight"].SetValue(Block.Atlas.Texture.Height);
        fx.Parameters["Viewport"].SetValue(new Vector2(target.Width, target.Height));
        fx.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);
        fx.Parameters["Zoom"].SetValue(zoom);
        float borderPx = 1;
        fx.Parameters["BorderResolution"].SetValue(new Vector2(borderPx / target.Width, borderPx / target.Height) * zoom);
        Color ambientColor = Color.Lerp(Color.White, map.GetAmbientColor(), 0);
        Vector4 ambient = ambientColor.ToVector4();
        fx.Parameters["AmbientLight"].SetValue(ambient);

        gd.DepthStencilState = DepthStencilState.Default;

        gd.SamplerStates[0] = SamplerState.PointClamp;
        gd.SamplerStates[1] = SamplerState.PointClamp;
        gd.SamplerStates[2] = SamplerState.PointClamp;
        gd.SamplerStates[3] = SamplerState.PointClamp;
        gd.SamplerStates[4] = SamplerState.PointClamp;

        MySpriteBatch mySB = new MySpriteBatch(gd);

        // use new technique to draw both color and light in one pass in multiple rendertargets
        this.DrawBlocks(ctx, gd, a, fx, mySB);

        // combine scenes
        gd.SetRenderTarget(target);
        this.DrawScene(target, gd, fx, mySB);

        // draw objects
        this.DrawEntities(ctx, gd, scene, fx, mySB);

        // draw entity shadows
        this.DrawEntityShadows(gd, fx, mySB);

        // draw block selection, using shadow shader for projected textures
        this.DrawBlockSelection(toolManager, fx, mySB);

        this.DrawMouseoverEntity(ctx, fx, mySB);
    }

    private void DrawBlocks(RenderContext ctx, GraphicsDevice gd, EngineArgs a, Effect fx, MySpriteBatch mySB)
    {
        gd.SetRenderTargets(this.MapRender, this.MapLight, this.MapDepth);
        gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(1f, 1f, 1f, 0), 1, 1);
        fx.CurrentTechnique = fx.Techniques["Combined"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        this.SetBlockTextures();
        this.DepthNear = float.MinValue;
        this.DepthFar = float.MaxValue;
        ctx.Map.DrawBlocks(mySB, ctx, a);
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

    private void DrawEntities(RenderContext ctx, GraphicsDevice gd, SceneState scene, Effect fx, MySpriteBatch mySB)
    {
        fx.CurrentTechnique = fx.Techniques["Entities"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
        fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);
        ctx.Map.DrawObjects(mySB, ctx, scene);
        mySB.Flush();
    }
    private void DrawMouseoverEntity(RenderContext ctx, Effect fx, MySpriteBatch mySB)
    {
        GameObject mouseover = Controller.Instance.Mouseover.Object as GameObject;
        fx.CurrentTechnique = fx.Techniques["Default"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        if (mouseover is not null)
            mouseover.DrawMouseover(mySB, ctx);
        mySB.Flush();
    }
    private void DrawBlockSelection(ToolManager toolManager, Effect fx, MySpriteBatch mySB)
    {
        fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        toolManager.DrawBeforeWorld(mySB, this._currentCtx);
        mySB.Flush();
    }
    private void DrawEntityShadows(GraphicsDevice gd, Effect fx, MySpriteBatch mySB)
    {
        fx.CurrentTechnique = fx.Techniques["EntityShadows"];
        gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        SpriteComp.DrawShadows(mySB, this._currentCtx);
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
            this.RenderTargets[this.RenderIndex].Name.ToConsole();
        }
    }

    //public void ZoomIncrease()
    //{
    //    this.ZoomNext *= 2;
    //    this.ZoomNext = MathHelper.Clamp(this.ZoomNext, this.ZoomMin, this.ZoomMax);

    //}
    //public void ZoomDecrease()
    //{
    //    this.ZoomNext /= 2;
    //    this.ZoomNext = MathHelper.Clamp(this.ZoomNext, this.ZoomMin, this.ZoomMax);
    //}
    //public void ZoomReset()
    //{
    //    this.ZoomNext = InitialZoom;
    //}

    //public float GetFarDepth(MapBase map)
    //{
    //    var size = map.GetSizeInChunks() * Chunk.Size;// -1;
    //    return (int)this.Rotation switch
    //    {
    //        0 => Vector3.Zero.GetDrawDepth(map, this),
    //        1 => new Vector3(0, size, 0).GetDrawDepth(map, this),
    //        2 => new Vector3(size, size, 0).GetDrawDepth(map, this),
    //        3 => new Vector3(size, 0, 0).GetDrawDepth(map, this),
    //        _ => 0,
    //    };
    //}
    //public float GetNearDepth(MapBase map)
    //{
    //    var size = map.GetSizeInChunks() * Chunk.Size;// -1;
    //    return (int)this.Rotation switch
    //    {
    //        0 => new Vector3(size, size, 0).GetDrawDepth(map, this),
    //        1 => new Vector3(size, 0, 0).GetDrawDepth(map, this),
    //        2 => Vector3.Zero.GetDrawDepth(map, this),
    //        3 => new Vector3(0, size, 0).GetDrawDepth(map, this),
    //        _ => 0,
    //    };
    //}

    //public void UpdateMaxDrawLevel(MapBase map)
    //{
    //    this.MaxDrawZ = this.GetMaxDrawLevel(map);
    //}
    //public int GetMaxDrawLevel(MapBase map)
    //{
    //    var actor = map.Net.GetPlayer()?.ControllingEntity;
    //    var value = (this.HideTerrainAbovePlayer && (actor is not null)) ? (int)actor.Transform.Global.RoundXY().Z + 2 + this.HideTerrainAbovePlayerOffset : this.DrawLevel;
    //    value = Math.Min(MapBase.MaxHeight - 1, Math.Max(0, value));
    //    return value;
    //}
    internal void ToggleHideBlocksAbove()
    {
        this.HideTerrainAbovePlayer = !this.HideTerrainAbovePlayer;
        if (this.HideTerrainAbovePlayer)
            this.HideTerrainAbovePlayerOffset = 0;
    }

    //internal void AdjustDrawLevel(int p)
    //{
    //    if (!this.HideTerrainAbovePlayer)
    //        this.DrawLevel = Math.Min(MapBase.MaxHeight - 1, Math.Max(0, this.DrawLevel + p));
    //    else
    //        this.HideTerrainAbovePlayerOffset += p;
    //}

    public void RecreateRenderTargets()
    {
        //int w = this.Width, h = this.Height;
        int w = this._lastWidth, h = this._lastHeight;
        var gfx = Game1.Instance.GraphicsDevice;

        this.MapRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "MapRender" };
        this.MapDepth = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "MapDepth" };
        this.MapNormals = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "MapNormals" };
        this.MapLight = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "MapLight" };
        this.TextureFogWater = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "TextureFogWater" };
        this.MapComposite = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "MapComposite" };

        this.RenderBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "RenderBeforeFog" };
        this.LightBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "LightBeforeFog" };
        this.DepthBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "DepthBeforeFog" };
        this.FogBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "FogBeforeFog" };

        this.FinalScene = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents) { Name = "FinalScene" };

        this.WaterRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "WaterRender" };
        this.WaterDepth = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "WaterDepth" };
        this.WaterLight = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "WaterLight" };
        this.WaterFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "WaterFog" };
        this.WaterComposite = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents) { Name = "WaterComposite" };

    }
    
    public void DrawGridCells(MySpriteBatch sb, Color col, IEnumerable<IntVec3> globals)
    {
        foreach (var pos in globals)
            this.DrawGridCell(sb, col, pos);
    }
    public void DrawGridCell(MySpriteBatch sb, Color col, IntVec3 global)
    {
        var ctx = this._currentCtx;
        if (global.Z > ctx.View.Settings.DrawLevel + 1)
            return;
        var view = ctx.View;
        var bounds = view.GetScreenBounds(global, Block.Bounds);
        var pos = new Vector2(bounds.X, bounds.Y);
        //var depth = global.GetDrawDepth(camera);
        var depth = view.GetDrawDepth(global);
        var highlight = Block.FaceHighlights[IntVec3.UnitZ];
        sb.Draw(highlight.Atlas.Texture, pos, highlight.Rectangle, 0, Vector2.Zero, view.Zoom, col, SpriteEffects.None, depth);
    }
    [Obsolete]
    public void DrawGridBlock(MySpriteBatch sb, Color col, IntVec3 global)
    {
        var ctx = this._currentCtx;
        var view = ctx.View;

        if (global.Z > view.Settings.DrawLevel)
            return;
        //var bounds = ctx.Camera.GetScreenBounds(global, Block.Bounds);
        var bounds = view.GetScreenBounds(global, Block.Bounds);
        var pos = new Vector2(bounds.X, bounds.Y);
        //var depth = global.GetDrawDepth(Engine.Map, this);
        var depth = view.GetDrawDepth(global);
        sb.Draw(Sprite.Atlas.Texture, pos, Sprite.BlockHighlight.AtlasToken.Rectangle, 0, Vector2.Zero, view.Zoom, col * .5f, SpriteEffects.None, depth);
    }
    public void DrawGridBlock(MySpriteBatch sb, AtlasDepthNormals.Node.Token sprite, Color col, IntVec3 global)
    {
        var ctx = this._currentCtx;
        if (global.Z > ctx.View.Settings.DrawLevel)
            return;
        var view = ctx.View;
        sprite.Atlas.Begin(Effect); // this was commented out
        var bounds = view.GetScreenBounds(global, Block.Bounds);
        var pos = new Vector2(bounds.X, bounds.Y);
        var depth = view.GetDrawDepth(global);
        sb.Draw(sprite.Atlas.Texture, pos, sprite.Rectangle, 0, Vector2.Zero, view.Zoom, col, SpriteEffects.None, depth);
    }
    public void DrawGridBlocks(MySpriteBatch sb, IEnumerable<IntVec3> positions, Color col)
    {
        Sprite.Atlas.Begin(Effect);
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
        sprite.Atlas.Begin(Effect);
        foreach (var pos in positions)
            this.DrawGridBlock(sb, sprite, col, pos);
        sb.Flush();
    }
    public void DrawBlockMouseover(MySpriteBatch sb, Vector3 global, Color color)
    {
        var ctx = this._currentCtx;
        if (global.Z > ctx.View.Settings.DrawLevel)
            return;
        var map = ctx.Map;
        var camera = ctx.Camera;
        var view = ctx.View;
        var bounds = Block.Bounds;
        view.GetEverything(global, bounds, out float cd, out Rectangle screenBounds, out Vector2 screenLoc);
        //var scrbnds = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, bounds, Vector2.Zero);
        var scrbnds = view.GetScreenBoundsVector4(global.X, global.Y, global.Z, bounds, Vector2.Zero);
        screenLoc = new Vector2(scrbnds.X, scrbnds.Y);
        //cd = global.GetDrawDepth(view);
        cd = view.GetDrawDepth(global);

        Block.Atlas.Begin(Effect);
        var c = color * .5f;
        var zoom = new Vector2(camera.Zoom);
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
    public void DrawBlockMouseover(MySpriteBatch sb, InteractionTarget target, Color color)
    {
        var ctx = this._currentCtx;
        var global = target.Global;
        if (global.Z > ctx.View.Settings.DrawLevel)
            return;
        var map = ctx.Map;
        var camera = ctx.Camera;
        var view = ctx.View;
        Rectangle bounds = Block.Bounds;
        view.GetEverything(global, bounds, out float cd, out Rectangle screenBounds, out Vector2 screenLoc);
        //var scrbnds = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, bounds, Vector2.Zero);
        var scrbnds = ctx.View.GetScreenBoundsVector4(global.X, global.Y, global.Z, bounds, Vector2.Zero);
        screenLoc = new Vector2(scrbnds.X, scrbnds.Y);
        //cd = global.GetDrawDepth(map, camera);
        cd = view.GetDrawDepth(global);

        Block.Atlas.Begin(Effect);
        var c = color * .5f;
        var zoom = new Vector2(camera.Zoom);
        // this draws the back faces highlight
        //sb.Draw(Block.Atlas.Texture, screenLoc, Block.BlockHighlightBack.Rectangle, 0, Vector2.Zero, zoom,
        //    Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);

        // this code draw individual faces instead of the whole highlight
        //if(target.Face == Vector3.UnitX)
        //sb.Draw(Block.Atlas.Texture, screenLoc, Block.FaceHighlights[-IntVec3.UnitX].Rectangle, 0, Vector2.Zero, zoom,
        //  Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, global.West().GetDrawDepth(map, this));
        //else if(target.Face == Vector3.UnitY)
        //    sb.Draw(Block.Atlas.Texture, screenLoc, Block.FaceHighlights[-IntVec3.UnitY].Rectangle, 0, Vector2.Zero, zoom,
        //  Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, global.North().GetDrawDepth(map, this));
        //else if (target.Face == Vector3.UnitZ)
        //    sb.Draw(Block.Atlas.Texture, screenLoc, Block.FaceHighlights[IntVec3.UnitZ].Rectangle, 0, Vector2.Zero, zoom,
        //              Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, global.Below().GetDrawDepth(map, this));

        sb.Draw(Block.Atlas.Texture, screenLoc, Block.FaceHighlights[target.Face].Rectangle, 0, Vector2.Zero, zoom,
        Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);// (global + target.Face).GetDrawDepth(map, this));

        // this draws the front faces highlight
        //sb.Draw(Block.Atlas.Texture, screenLoc, Block.BlockHighlight.Rectangle, 0, Vector2.Zero, zoom,
        //    Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);
        sb.Flush(); // flush here because i might have to switch textures in an overriden tool draw call
    }
    //internal bool IsDrawable(MapBase map, Vector3 global)
    //{
    //    return global.Z <= this.GetMaxDrawLevel(map) + 1;
    //}

   

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

    private FrameData SetupFrame(
    MapView view,
    GraphicsDevice gd,
    EngineArgs a,
    SceneState scene,
    ToolManager toolManager)
    {
        var map = view.Map;
        var camera = view.Camera;
        var zoom = view.Zoom;
        var cameraPos = view.Position;
        var w = view.Width;
        var h = view.Height;

        RenderContext ctx = BeginContext(view);

        var fx = Effect;

        var viewportDimensions = new Vector2(w, h);

        SetupGlobalShaderParams(zoom, viewportDimensions, fx);
        SetupOutlines(zoom, w, h, fx);

        SetupFogAndLight(toolManager, map, cameraPos, ctx, fx,
            out Vector4 fogColor,
            out float fog,
            out Vector2 fogOffset);

        SetupFollowingEntityParams(view, camera, w, h, fx,
            out double rotCos,
            out double rotSin);

        SetupVisibility(view, zoom);

        gd.DepthStencilState = DepthStencilState.Default;

        gd.SamplerStates[0] = SamplerState.PointClamp;
        gd.SamplerStates[1] = SamplerState.PointClamp;
        gd.SamplerStates[2] = SamplerState.PointClamp;
        gd.SamplerStates[3] = SamplerState.PointClamp;
        gd.SamplerStates[5] = SamplerState.PointClamp;

        EnsureSpritebatches(gd);

        var clearcol = new Color(1f, 1f, 1f, 0);

        SetupRenderTargets(gd, clearcol);

        SetBlockTextures();

        DepthNear = float.MinValue;
        DepthFar = float.MaxValue;

        fx.Parameters["RotCos"].SetValue((float)rotCos);
        fx.Parameters["RotSin"].SetValue((float)rotSin);

        SetupPlayerShaderParams(map, view, fx, ctx);

        var visibleChunks = GetVisibleChunks(view, map);

        var visibleEntities = CullEntities(ctx, map.Entities);
        SortEntities(ctx.View, visibleEntities);

        return new FrameData
        {
            Ctx = ctx,
            VisibleChunks = visibleChunks,
            VisibleEntities = visibleEntities,
            FogColor = fogColor,
            FogOffset = fogOffset,
            Fog = fog,
            RotCos = (float)rotCos,
            RotSin = (float)rotSin,
            ClearColor = clearcol
        };
    }
    private void DrawOpaqueStage(GraphicsDevice gd, FrameData frame, MapView view)
    {
        var map = view.Map;
        var fx = Effect;

        PrepareShaderNew(view, "Chunks");
        //Effect.Parameters["HideWalls"].SetValue(Engine.HideWalls);
        var visibleChunks = GetVisibleChunks(view, map);

        DrawOpaquePass(frame.Ctx, visibleChunks);

        fx.CurrentTechnique.Passes["Pass1"].Apply();
        SpriteBatch.Flush();
    }

    private void DrawOverlayStage(
    ToolManager toolManager,
    UIManager ui,
    MapBase map,
    FrameData frame)
    {
        DrawWorldOverlayStage(
            toolManager,
            ui,
            map,
            frame.Ctx,
            frame.VisibleEntities
        );

        SpriteBatch.Flush();
    }

    private void DrawTransparentStage(
    GraphicsDevice gd,
    FrameData frame,
    MapView view)
    {
        var map = view.Map;

        DrawTransparentPass(
            gd,
            view,
            frame.Ctx,
            frame.VisibleChunks,
            frame.ClearColor);

        SpriteBatch.Flush();
    }
    private void DrawCompositionStage(
        GraphicsDevice gd,
        FrameData frame,
        MapView view)
    {
        var fogColor = frame.FogColor;
        var fogOffset = frame.FogOffset;

        ComposeMapPass(gd, view, fogColor, fogOffset);
        ComposeWaterPass(gd, view);
    }

    private void DrawEntityStage(
    GraphicsDevice gd,
    MapBase map,
    FrameData frame,
    SceneState scene)
    {
        DrawEntityStage(gd, map, frame.Ctx, scene, frame.VisibleEntities);

        SpriteBatch.Flush();
    }

    private void DrawParticleStage(FrameData frame)
    {
        DrawParticleStage(frame.Ctx.Map, frame.Ctx);

        SpriteBatch.Flush();
    }

    private void DrawHighlightStage(
    ToolManager toolManager,
    FrameData frame)
    {
        var fx = Effect;
        var sb = SpriteBatch;

        // Block highlight
        fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
        fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);

        fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();

        toolManager.DrawAfterWorld(sb, frame.Ctx);
        sb.Flush();

        // Entity mouseover highlight
        fx.CurrentTechnique = fx.Techniques["EntityMouseover"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();

        if (toolManager.ActiveTool?.Target?.Object is GameObject mouseover && mouseover.Exists)
            mouseover.DrawMouseover(sb, frame.Ctx);

        sb.Flush();
    }
    SpriteBatch UISpritebatch;
    private void DrawUI(MapView view, GraphicsDevice gd)
    {
        var sb = this.UISpritebatch;// new SpriteBatch(gd);

        gd.SetRenderTarget(this.FinalScene);

        sb.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.DepthRead,
            RasterizerState.CullNone);

        view.Map.DrawInterface(sb, view);

        sb.End();
    }

    private void PresentStage(
    GraphicsDevice gd,
    MapView view,
    FrameData frame)
    {
        var fx = Effect;
        var map = view.Map;

        RenderTarget2D[] targets =
        {
        this.FinalScene,
        this.MapNormals,
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
        this.TextureFogWater
    };

        this.RenderTargets = targets;
        gd.SetRenderTarget(null);

        fx.Parameters["s"].SetValue(this.RenderTargets[this.RenderIndex]);
        fx.CurrentTechnique = fx.Techniques["Normal"];
        fx.CurrentTechnique.Passes["Pass1"].Apply();

        var vp = gd.Viewport.Bounds;
        var fc = this.FinalScene.Bounds;

        this.SpriteBatch.Draw(this.FinalScene, fc, vp, Color.White);
        this.SpriteBatch.Flush();

        map.DrawWorld(this.SpriteBatch, view);
        this.SpriteBatch.Flush();
    }

    public void ApplyChunkTransform(Chunk chunk, MapView view)
    {
        view.Iso(
            chunk.MapCoords.X * Chunk.Size,
            chunk.MapCoords.Y * Chunk.Size,
            0,
            out float x,
            out float y);

        view.Rotate(
            chunk.MapCoords.X,
            chunk.MapCoords.Y,
            out int rotx,
            out int roty);

        var world = Matrix.CreateTranslation(
            new Vector3(x, y, (rotx + roty) * Chunk.Size)
        );

        Effect.Parameters["World"].SetValue(world);
    }
}
