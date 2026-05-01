using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.ColorCustomization;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Systems.Materials;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Project1.Core.Animations;

public sealed class SpriteComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Sprite;
    public override string Name { get; } = "Sprite";
    readonly static bool PreciseAlphaHitTest = false;

    static public bool ShadowsEnabled = true;
    readonly static List<Shadow> ShadowList = new();
    public Bone Body, DefaultBody;
    Dictionary<AnimationDef, Animation> Animations = [];
    public Sprite Sprite;
    public int Variation;
    public int Orientation;
    public bool Flash;
    public Vector3 Offset;
    public double OffsetTimer;
    public bool Shadow;
    public Sprite FullSprite;
    public bool Hidden;
    Rectangle CachedMinimumRectangle;
    public Rectangle GetSpriteBounds()
    {
        var offset = new Vector2(CachedMinimumRectangle.Width / 2, CachedMinimumRectangle.Height - this.Body.Sprite.OriginY);
        return new Rectangle(-(int)(offset.X), -(int)(offset.Y),
            this.CachedMinimumRectangle.Width,
            this.CachedMinimumRectangle.Height
            );
    }
    public Animation AddAnimation(AnimationDef def, int weight = 1)
    {
        if (!this.Animations.TryGetValue(def, out var ani))
        {
            ani = new Animation(def)
            {
                Entity = this.Owner
            };
            this.Animations.Add(def, ani);
        }
        ani.Restart();
        ani.Weight = weight;
        return ani;
    }
    public Animation AddAnimationOld(AnimationDef def, int weight = 1)
    {
        if (this.Animations.TryGetValue(def, out var existing))
        {
            if (existing.WeightChange >= 0 && existing.State != AnimationStates.Removed)
                throw new Exception(); // ANIMATION MIGHT STILL BE FADING OUT WHEN THE NEXT BEHAVIOR BEGINS AND ADDS THE SAME TYPE OF ANIMATION!
        }
        // legacy check
        if (this.Animations.Values.Any(a => a.Def == def))
            throw new Exception();

        var animation = new Animation(def);
        animation.Weight = weight;
        animation.Entity = this.Owner;
        this.Animations.Add(def, animation);
        return animation;
    }
    
    internal Animation GetAnimation(AnimationDef def)
    {
        return this.Animations[def];
    }
    public Animation CrossFade(AnimationDef def, bool preFade, int fadeLength)
    {
        return this.CrossFade(def, preFade, fadeLength, Interpolation.Lerp);
    }
    public Animation CrossFade(AnimationDef animdef, bool preFade, int fadeLength, Func<float, float, float, float> fadeInterpolation)
    {
        var animation = this.AddAnimation(animdef);
        animation.FadeIn(preFade, fadeLength, fadeInterpolation);
        return animation;
    }
    public CharacterColors Customization = new();
    readonly Dictionary<BoneDef, Bone.Props> BoneProps = new();
    Dictionary<BoneDef, MaterialDef> Materials = new();
    public SpriteComp SetMaterial(BoneDef t, MaterialDef m)
    {
        if (this.Body.TryFindBone(t, out Bone b))
            b.Material = m;

        this.Materials[t] = m;
        if (this.BoneProps.TryGetValue(t, out var p))
            p.Material = m;
        else
            this.BoneProps.Add(t, new Bone.Props() { Material = m });
        return this;
    }
    public MaterialDef GetMaterial(BoneDef t)
    {
        return this.Body.FindBone(t)?.Material;
    }
    public MaterialDef GetMaterial(Bone t)
    {
        return this.GetMaterial(t.Def);
    }
    public MaterialDef TryGetMaterial(BoneDef t)
    {
        return this.Body.TryFindBone(t, out var b) ? b.Material : null;
    }
    public bool TryGetMaterial(BoneDef t, out MaterialDef mat)
    {
        if (this.Body.TryFindBone(t, out var bone))
        {
            mat = bone.Material;
            return true;
        }
        {
            mat = null;
            return false;
        }
    }

    /// <summary>
    /// TODO: decide if i want multiplicate or additive blend for this
    /// for additive, the default value should be transparent, for multiplicative it should be white
    /// change effect accordingly
    /// </summary>
    public Color Tint = Color.White; //Color.Transparent;

    public SpriteComp()
    {
     
    }

    internal override void Resolve()
    {

        var def = this.Owner.Def;
        this.Body = this.DefaultBody = def.Body.Clone();
        this.Body.MakeChildOf(this.Owner);
        this.Body.Material = def.DefaultMaterial;
        this.CachedMinimumRectangle = this.Body.GetMinimumRectangle();

        this.Sprite = def.DefaultSprite ?? this.Body.Sprite;
        this.DefaultBody = this.Body;

        var queue = new Queue<Bone>();
        queue.Enqueue(this.Body);
        while (queue.Any())
        {
            var current = queue.Dequeue();
            this.SetMaterial(current.Def, def.DefaultMaterial);
            foreach (var j in current.Joints.Values)
                if (j.Bone != null)
                    queue.Enqueue(j.Bone);
        }


        this.Customization = new CharacterColors(this.Body).Randomize();
        Variation = 0;
        Orientation = 0;

    }
    
    public override void SetMaterial(MaterialDef mat)
    {
        this.Materials[this.Body.Def] = mat;
        this.SetMaterial(this.Body.Def, mat);
    }
    
    public override void Tick()
    {
        var parent = this.Owner;
        if (this.Body == null)
            this.Body = this.DefaultBody;
       
        var nextAnimations = new Dictionary<AnimationDef, Animation>();
        foreach (var ani in Animations.Values)
        {
            ani.Tick(parent);
            if (!(ani.State == AnimationStates.Removed && ani.Weight <= 0))
                nextAnimations.Add(ani.Def, ani);
        }
        this.Animations = nextAnimations;
    }

    public Vector3 GetOffset()
    {
        double t = Math.Sin(OffsetTimer * 2 * Math.PI);
        return (float)t * Offset;
    }

    static public Vector3 GetOffset(Vector3 offset, double offsetTimer)
    {
        double t = Math.Sin(offsetTimer * 2 * Math.PI);
        return (float)t * offset;
    }

    public float _Angle;
    public override void Draw(
        MySpriteBatch sb,
        RenderContext ctx
        )
    {
        //this.Body.Sprite = ItemContent.AxeFull;
        if (this.Hidden)
            return;
        var parent = this.Owner;
        var camera = ctx.Camera;
        var spriteBounds = this.Sprite.GetBounds(); // TODO: cache this
        var global = parent.Transform.Global;
        //Rectangle bounds = camera.GetScreenBounds(global, spriteBounds);
        var map = parent.Map;
        var view = ctx.View;
        var source = Sprite.AtlasToken.Rectangle;
        var shaderRect = new Vector4(source.X / (float)Sprite.Texture.Width, source.Y / (float)Sprite.Texture.Height, source.Width / (float)Sprite.Texture.Width, source.Height / (float)Sprite.Texture.Height);
        Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);

        //float depth = global.GetDrawDepth(map, camera);
        float depth = view.GetDrawDepth(global);
        var body = this.Body;
        // TODO: slow?
        if (Flash)
        {
            Game1.Instance.Effect.Parameters["Overlay"].SetValue(new Vector4(10, 0, 0, 0.5f));
            Game1.Instance.Effect.Parameters["Overlay"].SetValue(new Vector4(1, 1, 1, 1));
            Flash = false;
        }
        else
        {
            Vector2 direction = parent.Transform.Direction;
            //Vector2 finalDir = Coords.Rotate(camera, direction);
            var finalDir = view.Rotate(direction);
            SpriteEffects sprfx = (finalDir.X - finalDir.Y) < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            parent.Map.GetLight(parent.Global.RoundXY(), out byte skylight, out byte blocklight);
            var skyColor = map.GetAmbientColor() * ((skylight + 1) / 16f); //((skylight) / 15f);
            skyColor.A = 255;
            var blockColor = Color.Lerp(Color.Black, Color.White, (blocklight) / 15f);
            var fog = ctx.Renderer.GetFogColorNew((int)parent.Global.Z);
            var test = ctx.GetScreenBoundsVector4(global.X, global.Y, global.Z, new Rectangle(0, 0, 0, 0), Vector2.Zero);
            var finalpos = new Vector2(test.X, test.Y) + (body.OriginGroundOffset * camera.Zoom);
            body.DrawTreeAnimationDeltas(parent, this.Customization, this.Animations.Values, sb, finalpos, skyColor, blockColor, this.Tint, fog, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);
        }

        // DRAW SHADOW
        Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

        this.RegisterShadow(ctx, spriteBounds, parent);
    }

    //static public void DrawPreview(SpriteBatch sb, MapView viewport, Vector3 global, GameObject obj)
    //{
    //    if (!obj.TryGetComponent<SpriteComp>("Sprite", out var spriteComp))
    //        return;
    //    Rectangle bounds;
    //    Vector2 screenLoc;
    //    bounds = viewport.Camera.GetScreenBounds(global, spriteComp.Sprite.GetBounds());
    //    screenLoc = new Vector2(bounds.X, bounds.Y);

    //    sb.Draw(spriteComp.Sprite.Texture, screenLoc,
    //        spriteComp.Sprite.SourceRects[0][spriteComp.Orientation], Color.White * 0.5f,
    //        0, Vector2.Zero, viewport.Camera.Zoom, SpriteEffects.None, 0);
    //    Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
    //}

    public override void DrawMouseover(MySpriteBatch sb, RenderContext ctx)
    {
        if (this.Hidden)
            return;
        var camera = ctx.Camera;
        var parent = this.Owner;
        var view = ctx.View;
        Vector2 loc = ctx.View.GetScreenPositionFloat(parent.Global);

        Vector2 direction = parent.Transform.Direction;
        //Vector2 finalDir = Coords.Rotate(camera, direction);
        var finalDir = view.Rotate(direction);
        SpriteEffects sprfx = (finalDir.X - finalDir.Y) < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var mouseovertint = new Color(1f, 1f, 1f, 0.5f);

        this.Body.DrawTreeAnimationDeltas(parent as Entity, this.Customization, this.Animations.Values, sb, loc + (this.Body.OriginGroundOffset) * camera.Zoom, Color.White, Color.White, mouseovertint, Color.Transparent, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, .99f);

        // TODO: handle case where root bone doesn't have a sprite, or draw whole bone tree instead
        ctx.Renderer.Effect.Parameters["s"].SetValue(Sprite.Atlas.Texture);
        sb.Flush();
    }

    static public void DrawHighlight(GameObject parent, SpriteBatch sb, MapView view)
    {
        var comp = parent.SpriteComp;
        var sprite = comp.Sprite;
        var source = sprite.AtlasToken.Rectangle;
        var global = parent.Global;
        var w = source.Width;
        var h = source.Height;
        var boundsVector4 = view.GetScreenBoundsVector4(global.X, global.Y, global.Z, new Rectangle(0, 0, w, h), comp.Body.OriginGroundOffset);
        var rect = boundsVector4.ToRectangle();
        rect.DrawHighlight(sb, .5f);
    }

    protected bool HitTest(Vector4 bounds, Rectangle src, Camera camera, out Vector3 face)
    {
        face = Vector3.Zero;
        if (bounds.Intersects(new Vector2(Controller.Instance.MouseRect.X, Controller.Instance.MouseRect.Y)))
        {
            if (!PreciseAlphaHitTest)
                return true;
            int xx = (int)((Controller.Instance.msCurrent.X - bounds.X) / camera.Zoom);
            int yy = (int)((Controller.Instance.msCurrent.Y - bounds.Y) / camera.Zoom);

            Color[] spriteMap = this.Sprite.ColorArray;
            Color c = spriteMap[yy * src.Width + xx];
            if (c.A == 0)
                return false;

            if (Sprite.MouseMap.Multifaceted)
                Sprite.MouseMap.HitTest(xx, yy, out face);

            return true;
        }
        return false;
    }
    public void HitTest(Entity parent, MapView view)
    {
        var camera = view.Camera;
        var source = this.GetSpriteBounds();// this.SpriteBounds;
        var global = parent.Global;
        //var boundsVector4 = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, source, Vector2.Zero, this.Body.Scale);// + Body.Sprite.WhiteSpace));
        var boundsVector4 = view.GetScreenBoundsVector4(global.X, global.Y, global.Z, source, Vector2.Zero, this.Body.Scale);// + Body.Sprite.WhiteSpace));

        if (HitTest(boundsVector4, source, camera, out Vector3 face))
        {
            //float depth = global.GetDrawDepth(parent.Map, camera);
            float depth = view.GetDrawDepth(global);
            Controller.TrySetMouseoverEntity(view, parent, face, depth);
        }
    }

    /// <summary>
    /// don't delete
    /// </summary>
    /// <param name="attacker"></param>
    /// <returns></returns>
    private bool OnHit(Entity attacker)
    {;
        Offset = this.Owner.Global - attacker.Global;
        Offset.Normalize();
        Offset /= 4;
        OffsetTimer = 0.25f;
        Flash = true;
        return true;
    }

    public override string ToString()
    {
        return this.Body.ToString();
    }

    public void RegisterShadow(RenderContext ctx, Rectangle spriteBounds, Entity parent)
    {
        var global = parent.Global;
        var map = parent.Map;
        int n = (int)global.RoundXY().Z;
        bool drawn = false;
        while (n >= 0 && !drawn)
        {
            var globalcheck = new Vector3(global.X, global.Y, n);
            if (map.TryGetCell(globalcheck, out Cell cellShadow) && cellShadow.Block.IsSolid(cellShadow))
            {
                var blockheight = Block.GetBlockHeight(map, globalcheck);
                if (ctx.View.CullingCheck(global.X, global.Y, n + 1, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out Rectangle shadowBounds))
                    ShadowList.Add(new Shadow(parent, new Vector3(global.X, global.Y, n + blockheight)));

                drawn = true;
            }
            n--;
        }
    }
    public static void DrawShadow(RenderContext ctx, Rectangle spriteBounds, MapBase map, GameObject parent, float depthNear, float depthFar)
    {
        var global = parent.Global;
        int z = (int)global.RoundXY().Z; //(int)global.Z; // 
        bool drawn = false;
        while (z >= 0 && !drawn)
        {
            if (map.TryGetCell(new Vector3(global.X, global.Y, z), out var cellShadow) && cellShadow.Block != BlockDefOf.Air.Block)
            {
                if (ctx.View.CullingCheck(global.X, global.Y, z + 1, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out _))
                    ShadowList.Add(new Shadow(parent, new Vector3(global.X, global.Y, z + 1)));
                drawn = true;
            }
            z--;
        }
    }
    static public void DrawShadows(MySpriteBatch sb, RenderContext ctx)
    {
        if (ShadowsEnabled)
        {
            var map = ctx.Map;
            var camera = ctx.Camera;
            var view = ctx.View;
            //foreach (Shadow shadow in ShadowList.OrderBy(foo => foo.Global.GetDrawDepth(map, camera)))
            foreach (Shadow shadow in ShadowList.OrderBy(foo => view.GetDrawDepth(foo.Global)))
                shadow.Draw(sb, ctx);
        }
          
        ShadowList.Clear();
    }
  
    internal override void CopyFrom(EntityComp comp)
    {
        var source = comp as SpriteComp;
        this.Sprite = source.Body.Sprite;
        this.Body = source.Body.Clone();
        this.DefaultBody = this.Body;
        this.CachedMinimumRectangle = this.Body.GetMinimumRectangle();
        this.Customization = new CharacterColors(this.Body).Randomize();
        Variation = 0;
        Orientation = 0;
        foreach (var anim in source.Animations.Values)
        {
            var newani = anim.Clone();
            newani.Entity = this.Owner;
            this.Animations.Add(anim.Def, newani);
        }
    }

    public override void DrawUI(SpriteBatch sb, MapView view)
    {
        DrawForbidden(sb, view);
        EntityTextManager.DrawStackSize(sb, view, this.Owner);
    }

    private void DrawForbidden(SpriteBatch sb, MapView view)
    {
        var parent = this.Owner;
        if (!parent.IsForbidden)
            return;
        if (view.Zoom <= .5f)
            return;
        var zoom = 1;
        var pos = view.GetScreenPosition(parent.Global) - new Vector2(Icon.Cross.SourceRect.Width, Icon.Cross.SourceRect.Height) * zoom / 2; ;// -new Vector2(UI.Icon.Cross.SourceRect.Width / 2, rect.Height * camera.Zoom);
        pos.Y -= Icon.Cross.SourceRect.Height / 2;
        Icon.Cross.Draw(sb, pos, zoom);
    }
    internal override void SaveExtra(SaveTag tag)
    {
        tag.Add(new SaveTag(SaveTag.Types.Int, "Variation", (int)Variation));
        tag.Add(new SaveTag(SaveTag.Types.Int, "Orientation", (int)Orientation));
        tag.Add(this.Body.Save("Body"));
        tag.Add(this.Animations.SaveValues("Animations"));
    }
    internal override void Load(GameObject parent, SaveTag compTag)
    {
        this.Customization = new CharacterColors(this.Body).Randomize();

        compTag.TryGetTag("Body", t =>
        {
            this.Body.Load(t);
        });

        if (this.Body.Material == null)
        {
            this.Body.Material = parent.Def.DefaultMaterial;
            Log.WriteToFile($"{parent.DebugName}'s body material was null, defaulting to {parent.Def.DefaultMaterial?.DebugName}");
        }
        this.Animations.LoadValuesWithInferredKeys(compTag["Animations"], v => v.Def);
        foreach (var a in this.Animations.Values)
            a.Entity = this.Owner;
    }
    public override void Write(IDataWriter w)
    {
        this.Customization.Write(w);
        this.Body.Write(w);
        w.WriteValues(this.Animations);
    }
    public override void Read(IDataReader r)
    {
        this.Customization = new CharacterColors(r);
        this.Body.Read(r);
        r.ReadValuesWithInferredKeys(this.Animations, a => a.Def);
        foreach (var a in this.Animations.Values)
            a.Entity = this.Owner;
    }

    public static Bone GetRootBone(GameObject parent)
    {
        if (parent == null)
            return null;
        return parent.GetComponent<SpriteComp>().Body;
    }

    public override void OnTooltipCreated(Control tooltip)
    {
        foreach (var b in this.Body.GetAllBones())
        {
            var mat = b.Material;
            tooltip.AddControlsBottomLeft(new Label($"{b.Def.LabelReadable}: {mat?.LabelReadable ?? "undefined"}") { TextColor = mat?.Color ?? Color.Gray });
        }
    }
    internal bool HasMatchingBody(GameObject otherItem)
    {
        var bones = this.Body.GetAllBones();
        var other = otherItem.Body;
        foreach (var b in bones)
        {
            if (!other.TryFindBone(b.Def, out var otherb))
                return false;
            if (b.Material != otherb.Material)
                return false;
        }
        return true;
    }
    public override Control GetParametrizer()
    {
        var parametrizableBones = this.Materials.Keys.Where(b => this.Owner.Def.CraftingProperties?.Reagents.ContainsKey(b) ?? false);
        var allMats = Def.Get<MaterialDef>();
        foreach (var bone in parametrizableBones)
        {
            MaterialDef currentlySelectedMaterial = null;
            var box = new GroupBox();
            var drop = new ComboBoxNewNew<MaterialDef>(allMats, 100, d => d.Name, b => currentlySelectedMaterial = b, () => currentlySelectedMaterial);
            box.AddControlsHorizontally(new Label(bone), drop);

        }
        return base.GetParametrizer();

    }

    internal void ToggleBone(BoneDef def, bool toggle, bool cascade)
    {
        this.Body.FindBone(def).SetEnabled(toggle, cascade);
        this.Owner.Map.Events.Post(new ActorBoneToggledEvent(this.Owner as Actor, def, toggle, cascade));
    }
    internal void OverrideRestingFrame(BoneDef def, Keyframe keyFrame)
    {
        this.Body.FindBone(def).RestingFrame = keyFrame;
        this.Owner.Map.Events.Post(new ActorRestingFrameOverridenEvent(this.Owner as Actor, def, keyFrame));
    }
}