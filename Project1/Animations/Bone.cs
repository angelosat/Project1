using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Animations;
using System.IO;
using System.Reflection.Metadata.Ecma335;

namespace Start_a_Town_
{
    public partial class Bone : Inspectable, ISaveable, ISerializableNew
    {
        public enum States { Unstarted, Stopped, Running, Finished, Manual }
        public override string Label => this.Def.Label;
        public States State;

        public Joint Joint;
        public Dictionary<BoneDef, Joint> Joints = new();
        public Bone AddJoint(BoneDef type, Joint joint)
        {
            this.Joints.Add(type, joint);
            joint.Parent = this;
            return this;
        }
        public Joint AddJoint(BoneDef type)
        {
            var joint = new Joint();
            this.Joints.Add(type, joint);
            joint.Parent = this;
            return joint;
        }
        public Bone AddJoint(Vector2 jointPos, Bone bone)
        {
            var joint = new Joint(jointPos);
            joint.SetBone(bone);
            joint.Parent = this;
            this.Joints.Add(bone.Def, joint);
            return this;
        }
        public Bone AddJoint(Bone bone)
        {
            return this.AddJoint(Vector2.Zero, bone);
        }
        public Joint GetJoint(BoneDef type)
        {
            return this.Joints.GetValueOrDefault(type);
        }

        //public Bone AddBone(Bone bone)
        //{
        //    var joint = this.GetJoint(bone.Def);
        //    joint.SetBone(bone);
        //    return this;
        //}

        public Bone Parent;
        public MaterialDef Material;

        public Color Tint = Color.White;

        /// <summary>
        /// The coordinates on the bone's parent (relative to it's parent sprite origin) where the current bone attaches to.
        /// </summary>
        public Vector2 ParentJoint;
        public List<Sprite> Orientations = new();

        public void SetOrientations(Sprite sprite1)
        {
            this.Orientations.Add(sprite1);
        }
        public void SetOrientations(Sprite sprite1, Sprite sprite2)
        {
            this.Orientations.Add(sprite1);
            this.Orientations.Add(sprite2);
        }
        public void SetOrientations(Sprite sprite1, Sprite sprite2, Sprite sprite3, Sprite sprite4)
        {
            this.Orientations.Add(sprite1);
            this.Orientations.Add(sprite2);
            this.Orientations.Add(sprite3);
            this.Orientations.Add(sprite4);
        }
        public Sprite GetSprite(Camera cam)
        {
            var index = (int)(cam.Zoom % this.Orientations.Count);
            return this.Orientations[index];
        }
        public Sprite GetSprite(int orientation)
        {
            if (this.Orientations.Count == 0)
                return null;
            var index = (int)(orientation % this.Orientations.Count);
            return this.Orientations[index];
        }

        public Sprite Sprite
        {
            get => this.SpriteFunc?.Invoke() ?? this.Orientations.FirstOrDefault();
            set
            {
                this.Orientations.Clear();
                this.Orientations.Add(value);
            }
        }
        public Func<Sprite> SpriteFunc;

        public Func<GameObject, GameObjectSlot> SlotFunc;

        GameObjectSlot SpriteSlot;
        public Keyframe RestingFrame;

        public SortedDictionary<float, List<AnimationClip>> Layers;
        public float Angle = 0;
        public Vector2 Offset = Vector2.Zero;

        float _Scale = 1;
        public float Scale
        {
            get { return this.ScaleFunc?.Invoke() ?? this._Scale; }
            set { this._Scale = value; }
        }
        public Func<float> ScaleFunc;


        public BoneDef Def;
        public float Order;
        bool Enabled = true;

        public void SetEnabled(bool value, bool passToChildren)
        {
            this.Enabled = value;
            if (!passToChildren)
                return;
            foreach (var c in this.Joints.Values)
                if (c.Bone != null)
                    c.Bone.SetEnabled(value, passToChildren);
        }

        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }
        static readonly DescendingComparer<float> Descending = new();
        Bone()
        {
            this.Order = 0;
            this.Angle = 0;
            this.Offset = Vector2.Zero;
            this.Layers = new SortedDictionary<float, List<AnimationClip>>(Descending);
            this.RestingFrame = new Keyframe(10, Vector2.Zero, 0, Interpolation.Sine);
        }

        public Bone Clone(Bone parent = null)
        {
            Bone newnode = Create(this.Def, this.Sprite, parent, this.ParentJoint, this.Order);

            newnode.RestingFrame = this.RestingFrame;
            newnode.SlotFunc = this.SlotFunc;
            newnode.Offset = this.Offset;
            newnode.OriginGroundOffset = this.OriginGroundOffset;
            newnode.Tint = this.Tint;
            newnode.Material = this.Material;
            newnode.DrawMaterialColor = this.DrawMaterialColor;
            newnode.ScaleFunc = this.ScaleFunc;
            newnode.Material = this.Material;

            foreach (var joint in this.Joints)
            {
                var j = new Joint(joint.Value);
                newnode.AddJoint(joint.Key, j);
            }

            newnode.Orientations.Clear();
            foreach (var i in this.Orientations)
                newnode.Orientations.Add(new Sprite(i));
            return newnode;
        }
        [InspectorHidden]
        public Bone this[BoneDef type]
        {
            get { return this.Joints[type].Bone; }
        }

        public void AddChild(BoneDef type, Bone spriteNode)
        {
            spriteNode.Parent = this;
        }
        public Bone AddChild(Bone bone)
        {
            bone.Parent = this;
            return this;
        }
        public Bone(BoneDef type)
            : this()
        {
            this.Def = type;
        }
        public Bone(BoneDef type, Vector2 joint, float depth, params Bone[] children)
            : this(type)
        {
            this.Order = depth;
            this.ParentJoint = joint;
            foreach (var child in children)
            {
                child.Parent = this;
            }
        }
        public Bone(BoneDef type, Sprite sprite, Vector2 joint, float depth, params Bone[] children)
            : this(type, sprite)
        {
            this.Order = depth;
            this.ParentJoint = joint;
            foreach (var child in children)
            {
                child.Parent = this;
            }
        }

        public Bone(BoneDef type, Sprite sprite, float depth = 0)
            : this()
        {
            this.Def = type;
            this.Sprite = sprite == null ? sprite : new Sprite(sprite);
            this.ParentJoint = Vector2.Zero;
            this.Order = depth;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="joint"></param>
        /// <param name="depth">Depth in relation to parent bone.</param>
        public Bone(BoneDef type, Vector2 joint, float depth)
            : this()
        {
            this.Def = type;
            this.Order = depth;
            this.ParentJoint = joint;

        }

        static public Bone Create(BoneDef type, Sprite sprite, Bone parent, Vector2 joint, float depth, params Bone[] children)
        {
            Bone sprnd = new(type, sprite, joint, depth);
            if (parent != null)
                parent.AddChild(type, sprnd);

            return sprnd;
        }
        static public Bone Create(BoneDef type, Bone parent, Vector2 joint, float depth, params Bone[] children)
        {
            Bone sprnd = new(type, joint, depth);
            if (parent != null)
                parent.AddChild(type, sprnd);

            return sprnd;
        }
        static public Bone Create(BoneDef type, Sprite sprite)
        {
            return new Bone(type, sprite);
        }
        static public Bone Create(BoneDef type, Sprite sprite, Vector2 joint, float depth)
        {
            return new Bone(type, sprite) { Order = depth, ParentJoint = joint };
        }

        /// <summary>
        /// The difference vector between the sprite's origin and the ground, for root bones.
        /// </summary>
        public Vector2 OriginGroundOffset = Vector2.Zero;
        public bool DrawMaterialColor;

        private void Evaluate(List<Animation> animations, ref float availableWeight, ref float finalAngle, ref Vector2 finalOffset)
        {
            var byLayer = animations.OrderBy(a => a.Layer);
            foreach (var ani in byLayer.Reverse())
            {
                if (ani.Weight <= 0)
                {

                }
                else
                {
                    float dang = 0;
                    Vector2 doff = Vector2.Zero;
                    if (ani.TryGetValue(this.Def, ref doff, ref dang))
                    {
                        finalAngle += dang * ani.Weight * availableWeight;
                        finalOffset += doff * ani.Weight * availableWeight;
                        availableWeight -= ani.Weight;
                        availableWeight = Math.Max(0, availableWeight);
                    }
                }
            }
        }

        /// <summary>
        /// legacy method for ui drawing (slots)
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sb"></param>
        /// <param name="screenLoc"></param>
        /// <param name="sky"></param>
        /// <param name="block"></param>
        /// <param name="color"></param>
        /// <param name="fog"></param>
        /// <param name="angle"></param>
        /// <param name="scale"></param>
        /// <param name="sprFx"></param>
        /// <param name="alpha"></param>
        /// <param name="depth"></param>
        public void DrawTree(GameObject parent, SpriteBatch sb, Vector2 screenLoc, Color sky, Color block, Color color, Color fog, float angle, float scale, SpriteEffects sprFx, float alpha, float depth)
        {
            Sprite sprite = SpriteSlot == null ? this.Sprite : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);

            var nextangle = this.Angle + (this.Joint != null ? this.Joint.Angle : 0);
            nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
            var finalAngle = angle + nextangle;

            Vector2 finalpos = screenLoc + this.Offset * scale;
            var finalDepth = depth - this.Order;
            if (sprite != null)
            {
                Rectangle sourceRect = sprite.GetSourceRect();

                Vector2 origin = this.Parent == null ? sprite.OriginGround : sprite.Joint;
                origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X - 1 : origin.X);

                sprite.Draw(sb, finalpos, color, 0, Vector2.Zero, scale, SpriteEffects.None, finalDepth);
            }
            foreach (var child in this.Joints.Values)
            {
                var next = child.Bone;
                if (next == null)
                    continue;
                Vector2 nextLoc = child.Position;
                nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                nextLoc = Coords.Rotate(finalAngle, nextLoc);
                next.DrawTree(parent, sb, finalpos + nextLoc * scale, sky, block, color, fog, finalAngle, scale, sprFx, alpha, finalDepth);

            }
        }


        /// <summary>
        /// the good one
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sb"></param>
        /// <param name="screenLoc"></param>
        /// <param name="sky"></param>
        /// <param name="block"></param>
        /// <param name="tint"></param>
        /// <param name="fog"></param>
        /// <param name="angle"></param>
        /// <param name="scale"></param>
        /// <param name="sprFx"></param>
        /// <param name="alpha"></param>
        /// <param name="depth"></param>
        public void DrawGhost(MySpriteBatch sb, Vector2 screenLoc, Color sky, Color block, Color tint, Color fog, float angle, float zoom, int orientation, SpriteEffects sprFx, float alpha, float depth)
        {
            Sprite sprite = SpriteSlot == null ? this.GetSprite(orientation) : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);
            var nextangle = this.Angle;
            nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
            var finalAngle = angle + nextangle;
            var scale = zoom * this.Scale;

            Vector2 finalpos = screenLoc + (this.Offset) * scale; // inject origingoundeffect from calling method?
            var finalDepth = depth - this.Order;
            if (sprite != null)
            {
                Rectangle sourceRect = sprite.GetSourceRect();
                Vector2 origin = sprite.OriginGround;
                origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X + 1 : origin.X);
                var finalTint = tint;
                sprite.Draw(sb, finalpos, this.Material, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);
            }
            foreach (var ch in this.Joints)
            {
                var child = ch.Value;
                var next = child.Bone;
                if (next == null)
                    continue;
                Vector2 nextLoc = child.Position;
                nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                nextLoc = Coords.Rotate(finalAngle, nextLoc);
                var childAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - child.Angle) : child.Angle);
                next.DrawGhost(sb, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
            }
        }

        public void DrawTreeAnimationDeltas(Entity parent, CharacterColors customization, List<Animation> animations, MySpriteBatch sb, Vector2 screenLoc, Color sky, Color block, Color tint, Color fog, float angle, float zoom, int orientation, SpriteEffects sprFx, float alpha, float depth)
        {
            var material = this.Material;
            Sprite sprite = SpriteSlot == null ? this.GetSprite(orientation) : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);
            float aniAngle = 0;
            Vector2 aniOffset = Vector2.Zero;
            float weight = 1;
            this.Evaluate(animations, ref weight, ref aniAngle, ref aniOffset);
            var nextangle = this.Angle + aniAngle;
            nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
            var finalAngle = angle + this.RestingFrame.Angle + nextangle;
            var scale = zoom * this.Scale;
            Vector2 finalpos = screenLoc + (this.RestingFrame.Offset + this.Offset + aniOffset) * scale;
            var finalDepth = depth - this.Order;
            if (this.Enabled && sprite != null)
            {
                Rectangle sourceRect = sprite.GetSourceRect();
                Vector2 origin = sprite.OriginGround;
                origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X + 1 : origin.X);
                var finalTint = tint;
                sprite.Draw(sb, customization, finalpos, this.DrawMaterialColor ? material : null, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);
            }
            foreach (var child in this.Joints.Values)
            {
                if (!child.TryGetBone(out var next, parent))
                    continue;
                Vector2 nextLoc = child.Position;
                nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                nextLoc = Coords.Rotate(finalAngle, nextLoc);
                var childAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - child.Angle) : child.Angle);
                next.DrawTreeAnimationDeltas(parent, customization, animations, sb, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
            }
        }
        public void DrawGhost(GameObject parent, MySpriteBatch sb, Vector2 screenLoc, Color sky, Color block, Color tint, Color fog, float angle, float zoom, int orientation, SpriteEffects sprFx, float alpha, float depth)
        {
            Sprite sprite = SpriteSlot == null ? this.GetSprite(orientation) : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);

            var nextangle = this.Angle;

            nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
            var finalAngle = angle + nextangle;
            var scale = zoom * this.Scale;
            Vector2 finalpos = screenLoc + (this.Offset) * scale; // inject origingoundeffect from calling method?
            var finalDepth = depth - this.Order;
            if (sprite != null)
            {
                Rectangle sourceRect = sprite.GetSourceRect();
                Vector2 origin = sprite.OriginGround;
                origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X + 1 : origin.X);
                var finalTint = tint;
                sprite.Draw(sb, finalpos, this.DrawMaterialColor ? this.GetMaterial() : null, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);
            }
            foreach (var ch in this.Joints)
            {
                var child = ch.Value;
                var next = child.Bone;
                if (next == null)
                    continue;
                Vector2 nextLoc = child.Position;
                nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                nextLoc = Coords.Rotate(finalAngle, nextLoc);
                var childAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - child.Angle) : child.Angle);
                next.DrawGhost(parent, sb, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth);
            }
        }

        MaterialDef GetMaterial()
        {
            return this.Material;
        }

        float GetAngle()
        {
            return this.Angle + (Parent != null ? Parent.GetAngle() : 0);
        }
        Vector2 GetOffset()
        {
            return this.Offset + (Parent != null ? Parent.GetOffset() : Vector2.Zero);
        }
        Vector2 GetJoint()
        {
            return this.ParentJoint + (Parent != null ? Parent.GetJoint() : Vector2.Zero);
        }
        Vector2 GetOrigin()
        {
            return (Parent != null ? Parent.GetOrigin() : this.Sprite.OriginGround);
        }
       
        static public Rectangle GetBounds(SortedList<float, Bone> list)
        {
            var bounds = new Rectangle?();
            foreach (Bone node in list.Values)
            {
                Rectangle source = node.Sprite.SourceRects[0][0];
                Vector2 offset = node.GetJoint() - node.GetOffset() - node.Sprite.OriginGround - (node.Parent != null ? node.Parent.Sprite.OriginGround : Vector2.Zero);
                var nodeRect = new Rectangle((int)offset.X, (int)offset.Y, source.Width, source.Height);
                bounds = !bounds.HasValue ? nodeRect : Rectangle.Union(bounds.Value, nodeRect);
            }
            return bounds.Value;
        }

        internal IEnumerable<Bone> GetAllBones()
        {
            yield return this;
            foreach (var j in this.Joints.Values)
            {
                var b = j.Bone;
                if (b != null)
                {
                    foreach (var a in b.GetAllBones())
                        yield return a;
                }
            }
        }

        internal Joint FindJoint(BoneDef type)
        {
            if (this.Joints.TryGetValue(type, out Joint joint))
                return joint;
            foreach (var j in this.Joints)
            {
                if (j.Value.Bone == null)
                    continue;
                joint = j.Value.Bone.FindJoint(type);
                if (joint != null)
                    return joint;
            }
            return joint;
        }
        internal Bone FindBone(BoneDef type)
        {
            if (this.Def == type)
                return this;
            return this.FindJoint(type)?.Bone?.FindBone(type);
        }
        internal bool TryFindBone(BoneDef t, out Bone b)
        {
            if (this.Def == t)
                b = this;
            else
                b = this.FindJoint(t)?.Bone;
            return b != null;
        }
        internal List<Bone> GetChildren()
        {
            var list = new List<Bone>() { this };

            foreach (var child in this.Joints.Values)
            {
                if (child.Bone != null)
                    list.AddRange(child.Bone.GetChildren());
            }
            return list;
        }

        internal Bone SetSprite(Sprite sprite)
        {
            this.Sprite = sprite;
            return this;
        }

        internal void MakeChildOf(GameObject parent)
        {
            if (SlotFunc != null)
                SpriteSlot = SlotFunc(parent);
            foreach (var joint in this.Joints.Values)
            {
                if (joint.Bone != null)
                    joint.Bone.MakeChildOf(parent);

                joint.MakeChildOf(parent);
            }
        }

        internal void RenderNewererest(GameObject parent, RenderTarget2D texture)
        {
            // same as gameobject.drawicon
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;

            var rect = texture.Bounds;
            gd.SetRenderTarget(texture);
            gd.Clear(Color.Transparent);

            var fx = Game1.Instance.Content.Load<Effect>("blur");
            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.Parameters["Viewport"].SetValue(new Vector2(rect.Width, rect.Height));

            fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);

            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = this;
            var scale = 1;
            var loc = new Vector2((int)((rect.Width) / 2), texture.Height / 2);
            loc *= scale;
            body.DrawGhost(parent, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

            mysb.Flush();
        }
        internal Texture2D RenderIcon(Entity parent, float scale = 1)
        {
            var minrect = GetMinimumRectangle();
            // same as gameobject.drawicon
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            var w = (int)(minrect.Width * scale);
            var h = (int)(minrect.Height * scale);

            var renderTarget = new RenderTarget2D(gd, w, h);
            gd.SetRenderTarget(renderTarget);
            gd.Clear(Color.Transparent);

            var fx = Game1.Instance.Content.Load<Effect>("blur");


            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.Parameters["Viewport"].SetValue(new Vector2(w, h));

            /// IMPORTANT!!! maybe that's why the unitframes aren't drawing at the start of the game?
            fx.Parameters["FarDepth"].SetValue(0);
            fx.Parameters["NearDepth"].SetValue(1);
            ///
            gd.SamplerStates[0] = SamplerState.PointWrap;
            fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);

            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = this;
            var customization = parent.GetComponent<Components.SpriteComponent>().Customization;
            var grounddistancefromcenter = renderTarget.Height / 2 + OriginGroundOffset.Y * scale;
            var loc = new Vector2(renderTarget.Width / 2, renderTarget.Height / 2 + grounddistancefromcenter);
            body.DrawTreeAnimationDeltas(parent, customization, new List<Animation>() { new Animation(AnimationDef.Null) }, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

            mysb.Flush();
            var texture = new Texture2D(gd, w, h);
            var data = new Color[w * h];
            gd.SetRenderTarget(null);
            renderTarget.GetData(data);
            texture.SetData(data);
            return texture;
        }

        internal void RenderIcon(Entity parent, RenderTarget2D texture, CharacterColors overlayColors = null)
        {
            var minrect = GetMinimumRectangle();
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            var rect = texture.Bounds;
            gd.SetRenderTarget(texture);
            gd.Clear(Color.Transparent);
            var fx = Game1.Instance.Content.Load<Effect>("blur");
            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.Parameters["Viewport"].SetValue(new Vector2(rect.Width, rect.Height));

            gd.SamplerStates[0] = SamplerState.PointClamp;

            fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = this;
            var scale = texture.Bounds.Width / minrect.Width;// 1;
            var customization = overlayColors ?? parent.GetComponent<Components.SpriteComponent>().Customization;

            var grounddistancefromcenter = rect.Height / 2 + OriginGroundOffset.Y * scale;
            var loc = new Vector2(rect.Width / 2, rect.Height / 2 + grounddistancefromcenter);
            body.DrawTreeAnimationDeltas(parent, customization, new List<Animation>() { new Animation(AnimationDef.Null) }, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

            mysb.Flush();
        }

        public void RenderNewer(GameObject parent, RenderTarget2D texture)
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            var rect = texture.Bounds;
            gd.SetRenderTarget(texture);
            gd.Clear(Color.Transparent);
            var fx = Game1.Instance.Content.Load<Effect>("blur");
            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.Parameters["Viewport"].SetValue(new Vector2(rect.Width, rect.Height));

            fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = this;
            var scale = 1;
            var minrect = GetMinimumRectangle();

            var loc = new Vector2((int)((rect.Width) / 2), (int)((rect.Height - minrect.Height) / 2)) + new Vector2(0, this.Sprite.OriginGround.Y) - this.OriginGroundOffset * 2;
            loc *= scale;
            body.DrawGhost(parent, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

            mysb.Flush();
        }
        void GetRectangle(List<Rectangle> rects)
        {
            if (this.Sprite != null)
                rects.Add(this.GetRectangle(Vector2.Zero));

            foreach (var node in this.Joints)
                if (node.Value.Bone != null)
                    node.Value.Bone.GetRectangle(node.Value.Position, rects);
        }
        void GetRectangle(Vector2 offset, List<Rectangle> rects)
        {
            if (this.Sprite != null)
                rects.Add(this.GetRectangle(offset));

            foreach (var node in this.Joints)
                if (node.Value.Bone != null)
                    node.Value.Bone.GetRectangle(offset + node.Value.Position, rects);
        }
        public Rectangle GetMinimumRectangle()
        {
            var rects = new List<Rectangle>();
            this.GetRectangle(rects);

            var union = new Rectangle(0, 0, 0, 0);
            foreach (var rect in rects)
            {
                union = Rectangle.Union(union, rect);
            }
            return new Rectangle(0, 0, union.Width, union.Height);
        }
        Rectangle GetRectangle(Vector2 offset)
        {
            var pos = offset + this.Offset - this.Sprite.OriginGround;
            return new Rectangle((int)pos.X, (int)pos.Y, this.Sprite.AtlasToken.Rectangle.Width, this.Sprite.AtlasToken.Rectangle.Height);
        }
        public IEnumerable<Joint> GetAllBones(BoneDef type)
        {
            if (this.Joints.TryGetValue(type, out var found))
                yield return found;
            foreach (var j in this.Joints.Values)
                if (j.Bone != null)
                    foreach (var i in j.Bone.GetAllBones(type)) yield return i;
        }
        //public List<Joint> GetAllBones(BoneDef type)
        //{
        //    var list = new List<Joint>();
        //    if (this.Joints.TryGetValue(type, out Joint found))
        //        list.Add(found);
        //    foreach (var j in this.Joints.Values)
        //        if (j.Bone != null)
        //            list.AddRange(j.Bone.GetAllBones(type));
        //    return list;
        //}
        public Vector2 GetTotalOffset()
        {
            var parent = this.Parent;
            var offset = -this.Joint.Position;
            while (parent != null)
            {
                if (parent.Joint != null)
                {
                    offset -= parent.Joint.Position;
                    parent = parent.Joint.Parent;
                }
                else
                {
                    offset -= parent.OriginGroundOffset;
                    parent = null;
                }
            }
            return offset;
        }

        public void Write(IDataWriter w)
        {
            //w.Write(this.Material != null ? this.Material.ID : -1);
            w.Write(this.Material != null ? this.Material.Name : "");

            //this.Sprite.Write(w); // i decided to sync sprites as well instead of relying on initializing sprites after gameobject loading/syncing
            w.Write(this.Sprite?.Name ?? "");
            foreach (var j in this.Joints.Values)
                j.Bone?.Write(w);
        }

        public ISerializableNew Read(IDataReader r)
        {
            //this.Material = MaterialDef.GetMaterial(r.ReadInt32());
            if (r.ReadString() is string matName && !matName.IsNullEmptyOrWhiteSpace())
                this.Material = Start_a_Town_.Def.GetDef<MaterialDef>(matName);

            //this.Sprite = Sprite.Load(r); // i decided to sync sprites as well instead of relying on initializing sprites after gameobject loading/syncing
            this.Sprite = r.ReadString() is string spritename && !spritename.IsNullEmptyOrWhiteSpace() ? Sprite.LoadNew(spritename) : null;

            foreach (var j in this.Joints.Values)
                j.Bone?.Read(r);
            return this;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Material?.Save(tag, "Material");
            this.Sprite?.Save(tag, "Sprite");
            var tagjoints = new SaveTag(SaveTag.Types.Compound, "Joints");
            foreach (var joint in this.Joints)
            {
                if (joint.Value.Bone != null)
                {
                    var childTag = joint.Value.Bone.Save(joint.Key.Name);
                    tagjoints.Add(childTag);
                }
            }
            tag.Add(tagjoints);
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            //tag.TryGetTagValue<string>("Material", i => this.Material = Start_a_Town_.Def.GetDef<MaterialDef>(i));
            tag.TryLoadDef("Material", ref this.Material);
            tag.TryGetTag("Sprite", t => this.Sprite = Sprite.Load(t));
            tag.TryGetTag("Joints", t =>
            {
                foreach (var j in this.Joints)
                {
                    t.TryGetTag(j.Key.Name, jtag => j.Value.Bone.Load(jtag));
                }
            });
            return this;
        }
        
        public IEnumerable<Bone> Descendants => this.GetAllBones();
        public IEnumerable<Joint> Children => this.Joints.Values;
        public override string ToString() => this.Label;

        public static ISerializableNew Create(IDataReader r) => new Bone().Read(r);
    }
}
