using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Input;
using Project1.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Base;
using Project1.Core.Rendering;
using Project1.Core.Materials;
using Project1.Core.Legacy;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Core.Entities.ColorCustomization;
using Project1.Core.Animations;
using Project1.Core.Components;
using Project1.Core.Graphics;
using Project1.Framework.UI;
using Project1.Framework.IO;

namespace Project1.Core.Entities
{
    public class SpriteComp : EntityComp
    {
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

        public SpriteComp Initialize(Bone bodySprite, Sprite fullSprite)
        {
            this.Sprite = fullSprite;
            this.Body = bodySprite;
            this.DefaultBody = this.Body;
            Variation = 0;
            Orientation = 0;
            return this;
        }
        public SpriteComp Initialize(Sprite fullSprite)
        {
            this.Sprite = fullSprite;
            this.Body = Bone.Create(BoneDefOf.Item, fullSprite);
            this.DefaultBody = this.Body;

            Variation = 0;
            Orientation = 0;
            return this;
        }
   

        internal override void ApplyMaterials(Entity parent, Dictionary<string, MaterialDef> ingredients)
        {
            var def = parent.Def;
            this.Materials.Clear();
            foreach (var i in def.CraftingProperties.Reagents)
            {
                this.SetMaterial(i.Key, ingredients[i.Value.LabelReadable]);
            }
        }
        public override void SetMaterial(MaterialDef mat)
        {
            this.Materials[this.Body.Def] = mat;
            this.SetMaterial(this.Body.Def, mat);
        }

        /// <summary>
        /// problem with mousemap! (color map)
        /// hit test is done against the default sprite!!!
        /// </summary>
        /// <param name="rootBone"></param>
        public SpriteComp(Bone rootBone)
            : this()
        {
            this.Body = rootBone.Clone();
            this.DefaultBody = this.Body;
            this.CachedMinimumRectangle = this.Body.GetMinimumRectangle();
            this.Customization = new CharacterColors(this.Body).Randomize();
        }
        [Obsolete]
        public SpriteComp(Sprite fullSprite)
            : this()
        {
            this.Sprite = fullSprite;
            this.Body = Bone.Create(BoneDefOf.Item, fullSprite);
            this.DefaultBody = this.Body;
            Variation = 0;
            Orientation = 0;
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
            GameObject parent,
            Camera camera
            )
        {
            //this.Body.Sprite = ItemContent.AxeFull;
            if (this.Hidden)
                return;
            Rectangle spriteBounds = this.Sprite.GetBounds();
            Vector3 global = parent.Transform.Global;
            Rectangle bounds = camera.GetScreenBounds(global, spriteBounds);
            var map = parent.Net.Map;

            var source = Sprite.AtlasToken.Rectangle;
            var shaderRect = new Vector4(source.X / (float)Sprite.Texture.Width, source.Y / (float)Sprite.Texture.Height, source.Width / (float)Sprite.Texture.Width, source.Height / (float)Sprite.Texture.Height);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);

            float depth = global.GetDrawDepth(map, camera);

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
                Vector2 finalDir = Coords.Rotate(camera, direction);
                SpriteEffects sprfx = (finalDir.X - finalDir.Y) < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                parent.Map.GetLight(parent.Global.RoundXY(), out byte skylight, out byte blocklight);
                var skyColor = map.GetAmbientColor() * ((skylight + 1) / 16f); //((skylight) / 15f);
                skyColor.A = 255;
                var blockColor = Color.Lerp(Color.Black, Color.White, (blocklight) / 15f);
                var fog = camera.GetFogColorNew((int)parent.Global.Z);
                var test = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, new Rectangle(0, 0, 0, 0), Vector2.Zero);
                var finalpos = new Vector2(test.X, test.Y) + (body.OriginGroundOffset * camera.Zoom);
                body.DrawTreeAnimationDeltas(parent as Entity, this.Customization, this.Animations.Values, sb, finalpos, skyColor, blockColor, this.Tint, fog, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);
            }

            // DRAW SHADOW
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

            this.DrawShadow(camera, spriteBounds, parent);
        }

        static public void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, GameObject obj)
        {
            if (!obj.TryGetComponent<SpriteComp>("Sprite", out var spriteComp))
                return;
            Rectangle bounds;
            Vector2 screenLoc;
            bounds = camera.GetScreenBounds(global, spriteComp.Sprite.GetBounds());
            screenLoc = new Vector2(bounds.X, bounds.Y);

            sb.Draw(spriteComp.Sprite.Texture, screenLoc,
                spriteComp.Sprite.SourceRects[0][spriteComp.Orientation], Color.White * 0.5f,
                0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
        }

        public override void DrawMouseover(MySpriteBatch sb, Camera camera, GameObject parent)
        {
            if (this.Hidden)
                return;

            Vector2 loc = camera.GetScreenPositionFloat(parent.Global);

            Vector2 direction = parent.Transform.Direction;
            Vector2 finalDir = Coords.Rotate(camera, direction);
            SpriteEffects sprfx = (finalDir.X - finalDir.Y) < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            var mouseovertint = new Color(1f, 1f, 1f, 0.5f);

            this.Body.DrawTreeAnimationDeltas(parent as Entity, this.Customization, this.Animations.Values, sb, loc + (this.Body.OriginGroundOffset) * camera.Zoom, Color.White, Color.White, mouseovertint, Color.Transparent, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, .99f);

            // TODO: handle case where root bone doesn't have a sprite, or draw whole bone tree instead
            camera.Effect.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            sb.Flush();
        }

        static public void DrawHighlight(GameObject parent, SpriteBatch sb, Camera camera)
        {
            var comp = parent.SpriteComp;
            var sprite = comp.Sprite;
            var source = sprite.AtlasToken.Rectangle;
            var global = parent.Global;
            var w = source.Width;
            var h = source.Height;
            var boundsVector4 = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, new Rectangle(0, 0, w, h), comp.Body.OriginGroundOffset);
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
        public void HitTest(GameObject parent, Camera camera)
        {
            var source = this.GetSpriteBounds();// this.SpriteBounds;
            var global = parent.Global;
            var boundsVector4 = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, source, Vector2.Zero, this.Body.Scale);// + Body.Sprite.WhiteSpace));

            if (HitTest(boundsVector4, source, camera, out Vector3 face))
            {
                float depth = global.GetDrawDepth(parent.Map, camera);
                Controller.TrySetMouseoverEntity(camera, parent, face, depth);
            }
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Attacked:
                    GameObject attacker = e.Parameters[0] as GameObject;
                    Offset = parent.Global - attacker.Global;
                    Offset.Normalize();
                    Offset /= 4;
                    OffsetTimer = 0.25f;
                    Flash = true;
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return this.Body.ToString();
        }

        public void DrawShadow(Camera camera, Rectangle spriteBounds, GameObject parent)
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
                    if (camera.CullingCheck(global.X, global.Y, n + 1, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out Rectangle shadowBounds))
                        ShadowList.Add(new Shadow(parent, new Vector3(global.X, global.Y, n + blockheight)));

                    drawn = true;
                }
                n--;
            }
        }
        public static void DrawShadow(Camera camera, Rectangle spriteBounds, MapBase map, GameObject parent, float depthNear, float depthFar)
        {
            var global = parent.Global;
            int z = (int)global.RoundXY().Z; //(int)global.Z; // 
            bool drawn = false;
            while (z >= 0 && !drawn)
            {
                if (map.TryGetCell(new Vector3(global.X, global.Y, z), out var cellShadow) && cellShadow.Block != BlockDefOf.Air.Worker)
                {
                    if (camera.CullingCheck(global.X, global.Y, z + 1, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out _))
                        ShadowList.Add(new Shadow(parent, new Vector3(global.X, global.Y, z + 1)));
                    drawn = true;
                }
                z--;
            }
        }
        static public void DrawShadows(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (ShadowsEnabled)
                foreach (Shadow shadow in ShadowList.OrderBy(foo => foo.Global.GetDrawDepth(map, camera)))
                    shadow.Draw(sb, map, camera);
            ShadowList.Clear();
        }
        public SpriteComp(SpriteComp source)
        {
            this.Sprite = source.Body.Sprite;
            this.Body = source.Body.Clone();
            this.DefaultBody = this.Body;
            this.CachedMinimumRectangle = this.Body.GetMinimumRectangle();
            this.Customization = new CharacterColors(this.Body).Randomize();
            Variation = 0;
            Orientation = 0;
            foreach (var anim in source.Animations.Values)
                this.Animations.Add(anim.Def, anim.Clone());
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
        public override object Clone()
        {
            return new SpriteComp(this);
        }

        static public bool HasOrientation(GameObject obj)
        {
            SpriteComp spriteComp = obj.GetComponent<SpriteComp>("Sprite");
            Sprite sprite = spriteComp.Sprite;
            return sprite.SourceRects.First().Length > 1;
        }

        public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            DrawForbidden(sb, camera, parent);
            EntityTextManager.DrawStackSize(sb, camera, parent);
        }

        private static void DrawForbidden(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (!parent.IsForbidden)
                return;
            if (camera.Zoom <= .5f)
                return;
            var zoom = 1;
            var pos = camera.GetScreenPosition(parent.Global) - new Vector2(Icon.Cross.SourceRect.Width, Icon.Cross.SourceRect.Height) * zoom / 2; ;// -new Vector2(UI.Icon.Cross.SourceRect.Width / 2, rect.Height * camera.Zoom);
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

        public override void OnTooltipCreated(GameObject parent, Control tooltip)
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
            var allMats = Def.GetDefs<MaterialDef>();
            foreach (var bone in parametrizableBones)
            {
                MaterialDef currentlySelectedMaterial = null;
                var box = new GroupBox();
                var drop = new ComboBoxNewNew<MaterialDef>(allMats, 100, d => d.Name, b => currentlySelectedMaterial = b, () => currentlySelectedMaterial);
                box.AddControlsHorizontally(new Label(bone), drop);

            }
            return base.GetParametrizer();

        }
    }
}