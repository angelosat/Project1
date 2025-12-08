using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Animations;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    public class SpriteComponent : EntityComponent
    {
        public override string Name { get; } = "Sprite";
        readonly static bool PreciseAlphaHitTest = false;

        static public bool ShadowsEnabled = true;
        readonly static List<Shadow> ShadowList = new();
        public Bone Body, DefaultBody;
        public List<Animation> Animations = new();
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

        public CharacterColors Customization = new();
        readonly Dictionary<BoneDef, Bone.Props> BoneProps = new();
        Dictionary<BoneDef, MaterialDef> Materials = new();
        public SpriteComponent SetMaterial(BoneDef t, MaterialDef m)
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

        public SpriteComponent()
        {
            this.Hidden = false;
            this.Sprite = Sprite.Default;
            this.Body = Bone.Create(BoneDefOf.Torso, Sprite);
            this.DefaultBody = this.Body;

            this.Flash = false;
            this.Offset = Vector3.Zero;
            this.OffsetTimer = 1d;
            this.Variation = 0;
            this.Orientation = 0;
        }
    
        
        public SpriteComponent(Bone bodySprite, Texture2D texture, Rectangle[][] sourcerect, Vector2 origin, MouseMap mousemap = null)
            : this()
        {
            this.Body = bodySprite;
            this.DefaultBody = this.Body;

            Sprite = new Sprite(texture, sourcerect, origin, mousemap);
            Variation = 0;
            Orientation = 0;
        }
        public SpriteComponent(Texture2D texture, Rectangle[][] sourcerect, Vector2 origin, MouseMap mousemap = null)
            : this()
        {
            this.Sprite = new Sprite(texture, sourcerect, origin, mousemap);
            this.Body = Bone.Create(BoneDefOf.Torso, this.Sprite);
            this.DefaultBody = this.Body;

            Sprite = new Sprite(texture, sourcerect, origin, mousemap);
            Variation = 0;
            Orientation = 0;
        }
        public SpriteComponent(Bone bodySprite, Sprite fullSprite)
            : this()
        {
            this.Sprite = fullSprite ?? bodySprite.Sprite;
            this.Body = bodySprite.Clone();
            this.DefaultBody = this.Body;
            this.CachedMinimumRectangle = this.Body.GetMinimumRectangle();
            this.Customization = new CharacterColors(this.Body).Randomize();
            Variation = 0;
            Orientation = 0;
        }
        public SpriteComponent(ItemDef def)
              : this()
        {
            this.Body = def.Body.Clone();
            this.Body.Material = def.DefaultMaterial;
            this.CachedMinimumRectangle = this.Body.GetMinimumRectangle();

            this.Sprite = def.DefaultSprite ?? def.Body.Sprite;
            this.DefaultBody = this.Body;

            InitMaterials(def);

            this.Customization = new CharacterColors(this.Body).Randomize();
            Variation = 0;
            Orientation = 0;
        }
        public SpriteComponent Initialize(Bone bodySprite, Sprite fullSprite)
        {
            this.Sprite = fullSprite;
            this.Body = bodySprite;
            this.DefaultBody = this.Body;
            Variation = 0;
            Orientation = 0;
            return this;
        }
        public SpriteComponent Initialize(Sprite fullSprite)
        {
            this.Sprite = fullSprite;
            this.Body = Bone.Create(BoneDefOf.Item, fullSprite);
            this.DefaultBody = this.Body;

            Variation = 0;
            Orientation = 0;
            return this;
        }
        private void InitMaterials(ItemDef def)
        {
            var queue = new Queue<Bone>();
            queue.Enqueue(def.Body);
            while(queue.Any())
            {
                var current = queue.Dequeue();
                this.SetMaterial(current.Def, def.DefaultMaterial);
                foreach (var j in current.Joints.Values)
                    if(j.Bone != null)
                    queue.Enqueue(j.Bone);
            }
        }
        
        internal override void Initialize(Entity parent, Dictionary<string, MaterialDef> ingredients)
        {
            var def = parent.Def;
            this.Materials.Clear();
            foreach (var i in def.CraftingProperties.Reagents)
            {
                this.SetMaterial(i.Key, ingredients[i.Value.Label]);
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
        public SpriteComponent(Bone rootBone)
            : this()
        {
            this.Body = rootBone.Clone();
            this.DefaultBody = this.Body;
            this.CachedMinimumRectangle = this.Body.GetMinimumRectangle();
            this.Customization = new CharacterColors(this.Body).Randomize();
        }
        [Obsolete]
        public SpriteComponent(Sprite fullSprite)
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
            var parent = this.Parent;
            if (this.Body == null)
                this.Body = this.DefaultBody;
            var nextAnimations = new List<Animation>();
            foreach (var ani in Animations)
            {
                ani.Update(parent);
                if (!(ani.State == AnimationStates.Removed && ani.Weight <= 0))
                    nextAnimations.Add(ani);
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
                body.DrawTreeAnimationDeltas(parent as Entity, this.Customization, this.Animations, sb, finalpos, skyColor, blockColor, this.Tint, fog, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);
            }

            // DRAW SHADOW
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

            this.DrawShadow(camera, spriteBounds, parent);
        }
      
        public override void MakeChildOf(GameObject parent)
        {
            Body.MakeChildOf(parent);
        }

        static public void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, GameObject obj)
        {
            if (!obj.TryGetComponent<SpriteComponent>("Sprite", out var spriteComp))
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
            var mouseovertint =  new Color(1f, 1f, 1f, 0.5f);

            this.Body.DrawTreeAnimationDeltas(parent as Entity, this.Customization, this.Animations, sb, loc + (this.Body.OriginGroundOffset) * camera.Zoom, Color.White, Color.White, mouseovertint, Color.Transparent, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, .99f);

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
            if(bounds.Intersects(new Vector2(Controller.Instance.MouseRect.X, Controller.Instance.MouseRect.Y)))
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
                if (map.TryGetCell(new Vector3(global.X, global.Y, z), out var cellShadow) && cellShadow.Block != BlockDefOf.Air)
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

        public override object Clone()
        {
            var comp = new SpriteComponent(this.Body, this.Sprite)
            {
                Materials = this.Materials.ToDictionary(t => t.Key, t => t.Value)
            };
            return comp;
        }

        static public bool HasOrientation(GameObject obj)
        {
            SpriteComponent spriteComp = obj.GetComponent<SpriteComponent>("Sprite");
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
            var pos = camera.GetScreenPosition(parent.Global) - new Vector2(UI.Icon.Cross.SourceRect.Width, UI.Icon.Cross.SourceRect.Height) * zoom / 2; ;// -new Vector2(UI.Icon.Cross.SourceRect.Width / 2, rect.Height * camera.Zoom);
            pos.Y -= UI.Icon.Cross.SourceRect.Height/2;
            UI.Icon.Cross.Draw(sb, pos, zoom);
        }
       
        internal override List<SaveTag> Save()
        {
            var list = new List<SaveTag>() { 
                new SaveTag(SaveTag.Types.Int, "Variation", (int)Variation),
                new SaveTag(SaveTag.Types.Int, "Orientation", (int)Orientation),
                this.Body.Save("Body")
            };
            return list;
        }
        internal override void Load(GameObject parent, SaveTag compTag)
        {
            this.Customization = new CharacterColors(this.Body).Randomize();

            compTag.TryGetTag("Body", t =>
            {
                this.Body.Load(t);
            });

            if(this.Body.Material == null)
            {
                this.Body.Material = parent.Def.DefaultMaterial;
                Log.WriteToFile($"{parent.DebugName}'s body material was null, defaulting to {parent.Def.DefaultMaterial?.DebugName}");
            }
        }
        public override void Write(IDataWriter w)
        {
            this.Customization.Write(w);
            this.Body.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.Customization = new CharacterColors(r);
            this.Body.Read(r);
        }

        public static Bone GetRootBone(GameObject parent)
        {
            if (parent == null)
                return null;
            return parent.GetComponent<SpriteComponent>().Body;
        }

        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            foreach (var b in this.Body.GetAllBones())
            {
                var mat = b.Material;
                tooltip.AddControlsBottomLeft(new Label(string.Format("{0}: {1}", b.Def.Label, mat?.Name ?? "undefined")) { TextColor = mat?.Color ?? Color.Gray });
            }
        }

        class Props : ComponentProps
        {
            public override Type CompClass => typeof(SpriteComponent);
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
    }
}
