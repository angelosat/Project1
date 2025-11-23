using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    public class Sprite : Inspectable, IDisposable
    {
        static public AtlasWithDepth Atlas = new("Entities");

        public Dictionary<string, Sprite> Overlays = new();
        static readonly string Path = "Graphics/Items/";
        public string AssetPath => Path + this.Name;
        static readonly Dictionary<string, Sprite> Registry = new();

        public enum OrientationType { South, West, North, East };
        public string Name;
        public string OverlayName = "";
        Texture2D _Texture;
        public Texture2D Texture
        {
            get
            {
                if (AtlasToken != null)
                    return Atlas.Texture;
                return _Texture;
            }
            set 
            {
                _Texture = value; 
            }
        }
        public Rectangle[][] SourceRects;
        public Vector2 OriginGround;
        public int OriginY;
        public MouseMap MouseMap;
        Color _Tint = Color.Transparent;
        public Color Tint
        {
            get
            {
                return this._Tint;
            }
            set
            {
                this._Tint = value;
            }
        }
        public int WhiteSpace { get; set; }

        public MaterialDef Material;
        float _Shininess = 0;
        public float Shininess
        {
            get
            {
                if (this.Material != null)
                    return this.Material.Type.Shininess;
                return this._Shininess;
            }
            set
            {
                this._Shininess = value;
            }
        }
        public Rectangle[][] Highlights;
        public float Alpha = 1;
        public Color[] ColorArray;

        /// <summary>
        /// this is not currently used. maybe use it as a point of origin for when the sprite is attached to a parent bone? and have the origin field seperately for when it's not attached?
        /// </summary>
        public Vector2 Joint;
        public int Variation = 0, Orientation = 0;
        public Vector2 AtlasCoords;
        public AtlasWithDepth.Node.Token AtlasToken;
        public void Dispose()
        {
            if (this.Texture is not null)
                this.Texture.Dispose();
        }

        static Sprite _default = new("default", new Vector2(16, 24), new Vector2(16, 24));
        static public Sprite Default => _default;

        public static Sprite Shadow;
        static public Dictionary<Vector3, Sprite> BlockFaceHighlights = new();
        static public Sprite BlockHighlight, BlockHightlightBack;
        static public readonly Texture2D CubeDepth = Game1.Instance.Content.Load<Texture2D>("Graphics/cubedepth9");
        static public readonly Texture2D HalfCubeDepth = Game1.Instance.Content.Load<Texture2D>("Graphics/cubehalfdepth9");

        static public void Initialize()
        {
            Shadow = new Sprite("isoShadow", new Vector2(16, 8)) { Tint = Color.White };
            BlockFaceHighlights[Vector3.UnitX] = new Sprite("blocks/highlightwest", "blockDepthFar", Block.OriginCenter);
            BlockFaceHighlights[Vector3.UnitY] = new Sprite("blocks/highlightnorth", "blockDepthFarLeft", Block.OriginCenter);
            BlockFaceHighlights[Vector3.UnitZ] = new Sprite("blocks/highlightdown", "blockDepthFarRight", Block.OriginCenter);
            BlockHighlight = new Sprite("blocks/highlightfull", Block.BlockDepthMap) { OriginGround = Block.OriginCenter };
            //BlockHightlightBack = new Sprite("blocks/highlightfullback", Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepth09back")) { OriginGround = Block.OriginCenter };
            BlockHightlightBack = new Sprite("blocks/highlightfullback", Block.BlockDepthMapFar) { OriginGround = Block.OriginCenter };
            Atlas.Initialize();
        }
        
        public Sprite(Sprite toClone)
        {
            this.Texture = toClone.Texture;
            this.SourceRects = toClone.SourceRects;
            this.OriginGround = toClone.OriginGround;
            this.Joint = toClone.Joint;
            this.MouseMap = toClone.MouseMap;
            this.Highlights = toClone.Highlights;
            this.Name = toClone.Name;
            this.Alpha = toClone.Alpha;
            this.AtlasToken = toClone.AtlasToken;
            this.ColorArray = toClone.ColorArray;
            this.Tint = toClone.Tint;
            this.Shininess = toClone.Shininess;
            this.OverlayName = toClone.OverlayName;
            this.Material = toClone.Material;
            this.WhiteSpace = toClone.WhiteSpace;
            this.OriginY = toClone.OriginY;
            foreach (var ol in toClone.Overlays)
                this.Overlays.Add(ol.Key, new Sprite(ol.Value));
        }
        public Sprite(Texture2D texture, Rectangle[][] sourcerect, Vector2 origin, MouseMap mousemap = null, Rectangle[][] highlights = null)
        {
            this.Highlights = highlights;
            this.Name = texture.Name;
            Registry[this.AssetPath] = this;

            Texture = texture;
            SourceRects = sourcerect;
            Joint = OriginGround = origin;
            if (mousemap != null)
                MouseMap = mousemap;
            else
                MouseMap = new MouseMap(texture, sourcerect);
            Alpha = 1;
            ColorArray = CreateColorArray(this);
        }
        public Sprite(string assetName) : this(assetName, Vector2.Zero) { }
        public Sprite(string assetName, string depthMap, Vector2 origin)
        {
            this.Name = assetName;
            Registry[this.AssetPath] = this;

            OriginGround = origin + Vector2.One * Borders.Thickness;

            Texture2D texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + assetName);
            this.SourceRects = new Rectangle[][] { new Rectangle[] { texture.Bounds } };

            MouseMap = new MouseMap(texture, texture.Bounds);

            this.Texture = Atlas.Texture;
            AtlasToken = Atlas.Load(assetName, depthMap);
            this.ColorArray = this.AtlasToken.ColorArray;

            Joint = new Vector2(SourceRects.First().First().Width / 2, SourceRects.First().First().Height / 2);
        }
        public Sprite(string assetName, float layer)
        {
            this.Name = assetName;
            Registry[this.AssetPath] = this;

            OriginGround = Vector2.Zero;

            Texture2D texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + assetName);
            this.SourceRects = new Rectangle[][] { new Rectangle[] { texture.Bounds } };

            MouseMap = new MouseMap(texture, texture.Bounds);

            this.Texture = Atlas.Texture;
            AtlasToken = Atlas.Load(assetName, layer);
            this.ColorArray = this.AtlasToken.ColorArray;
            Joint = new Vector2(SourceRects.First().First().Width / 2, SourceRects.First().First().Height / 2);
        }
        public Sprite(string assetName, Texture2D depthMap)
        {
            this.Name = assetName;
            Registry[this.AssetPath] = this;
            OriginGround = Vector2.Zero;

            Texture2D texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + assetName);
            this.SourceRects = new Rectangle[][] { new Rectangle[] { texture.Bounds } };

            MouseMap = new MouseMap(texture, texture.Bounds);

            this.Texture = Atlas.Texture;
            AtlasToken = Atlas.Load(assetName, depthMap);
            this.ColorArray = this.AtlasToken.ColorArray;
            Joint = new Vector2(SourceRects.First().First().Width / 2, SourceRects.First().First().Height / 2);
        }
        public Sprite(string assetName, Vector2 origin, MouseMap mousemap = null, Rectangle[][] highlights = null, float alpha = 1)
        {
            this.Name = assetName;
            Registry[this.AssetPath] = this;
            this.Alpha = alpha;
            OriginGround = origin + Vector2.One * Borders.Thickness;

            Texture2D texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + assetName);
            this.SourceRects = new Rectangle[][] { new Rectangle[] { texture.Bounds } };
            if (mousemap != null)
                MouseMap = mousemap;
            else
                MouseMap = new MouseMap(texture, texture.Bounds);

            this.Texture = Atlas.Texture;
            AtlasToken = Atlas.Load(assetName);
            this.ColorArray = this.AtlasToken.ColorArray;
            Joint = new Vector2(SourceRects.First().First().Width / 2, SourceRects.First().First().Height / 2);
        }
        public Sprite(string assetName, Vector2 origin, Vector2 joint, MouseMap mousemap = null, Rectangle[][] highlights = null, float alpha = 1)
        {
            this.Name = assetName;
            Registry[this.AssetPath] = this;
            this.Alpha = alpha;
            OriginGround = origin + Vector2.One * Borders.Thickness;
            
            Texture2D texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + assetName);
            this.SourceRects = new Rectangle[][] { new Rectangle[] { texture.Bounds } };
            if (mousemap != null)
                MouseMap = mousemap;
            else
                MouseMap = new MouseMap(texture, texture.Bounds);

            this.Texture = Atlas.Texture;
            AtlasToken = Atlas.Load(assetName);
            this.ColorArray = this.AtlasToken.ColorArray;
            Joint = joint + Vector2.One * Borders.Thickness;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Name);
        }
        public static Sprite LoadNew(string assetName)
        {
            return Registry[Path + assetName];
        }
        public static Sprite Load(string assetFullName)
        {
            return Registry[assetFullName];
        }
        public static Sprite Load(BinaryReader r)
        {
            return Registry[Path + r.ReadString()];
        }
        public override string ToString()
        {
            return this.AssetPath;
        }
        /// <summary>
        /// Creates an array used for hit testing
        /// </summary>
        static public Color[] CreateColorArray(Sprite sprite)
        {
            Rectangle source = sprite.SourceRects[0][0];
            Color[] spriteMap = new Color[source.Width * source.Height];
            sprite.Texture.GetData(0, source, spriteMap, 0, source.Width * source.Height);
            return spriteMap;
        }
        public Rectangle GetSourceRect()
        {
            return this.AtlasToken.Rectangle;
        }

        public Rectangle GetBounds()
        { 
            return new Rectangle(
                -(int)OriginGround.X, 
                -(int)OriginGround.Y, 
                this.AtlasToken.Rectangle.Width + 2 * Graphics.Borders.Thickness, 
                this.AtlasToken.Rectangle.Height + 2 * Graphics.Borders.Thickness
                ); 
        }
        public Sprite SetGroundContact(Vector2 factor)
        {
            var rect = this.SourceRects.First().First();
            this.OriginGround = new Vector2(factor.X * rect.Width, factor.Y * rect.Height);
            return this;
        }

        public Sprite AddOverlay(string overlayName, Sprite overlay)
        {
            this.Overlays.Add(overlayName, overlay);
            return this;
        }
        public Sprite RemoveOverlay(string overlayName)
        {
            this.Overlays.Remove(overlayName);
            return this;
        }
        public List<Sprite> GetOverlays()
        {
            List<Sprite> list = new List<Sprite>() { this };
            foreach (var child in this.Overlays.Values)
            {
                list.AddRange(child.GetOverlays());
            }
            return list;
        }

        public bool HitTest(float zoom, Rectangle bounds, Rectangle hitRect)
        {
            if (!bounds.Intersects(hitRect))
                return false;

            int xx = (int)((hitRect.X - bounds.X) / zoom);
            int yy = (int)((hitRect.Y - bounds.Y) / zoom);

            // TODO: fix face detection
            // TODO: have the color array generated on creation of sprite and cache it
            Color[] spriteMap = this.ColorArray;
            Color c = spriteMap[yy * Texture.Width + xx];
            if (c.A == 0)
                return false;
            return true;
        }

        public void Save(SaveTag tag, string name)
        {
            tag.Add(new SaveTag(SaveTag.Types.String, name, this.Name));
        }
        public static Sprite Load(SaveTag tag, string name)
        {
            return Registry[Path + (string)tag[name].Value];
        }
        public static Sprite Load(SaveTag tag)
        {
            var assetName = Path + (string)tag.Value;
            //return Registry[assetName];
            if(!Registry.TryGetValue(Path + (string)tag.Value, out var sprite))
                Log.Warning($"Sprite \"{assetName}\" doesn't exist");
            return sprite;
        }

        public void Draw(MySpriteBatch sb, Vector2 screenPos, Color color, float rotation, Vector2 origin, float scale, SpriteEffects sprFx, float depth)
        {
            var c = color.Multiply(this.Tint) * this.Alpha;
            if (this.AtlasToken != null)
                sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, scale, c, sprFx, depth);
            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, screenPos, color, rotation, origin, scale, sprFx, depth);
        }
        public void Draw(MySpriteBatch sb, Vector2 screenPos, Color sky, Color block, Color tint, Color fog, float rotation, Vector2 origin, float scale, SpriteEffects sprFx, float depth)
        {
            var matcol = this.Material != null ? new Color(this.Material.Color.R, this.Material.Color.G, this.Material.Color.B, (byte)(this.Material.Type.Shininess * 255)) : new Color(1f,1f,1f,0f);
            var t = tint;
            if (this.AtlasToken != null)
                sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, new Vector2(scale), sky, block, matcol, t, fog, sprFx, depth);

            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, screenPos, sky, block, tint, fog, rotation, origin, scale, sprFx, depth);
        }
        public void Draw(MySpriteBatch sb, Vector2 screenPos, MaterialDef material, Color sky, Color block, Color tint, Color fog, float rotation, Vector2 origin, float scale, SpriteEffects sprFx, float depth)
        {
            if (material == null) // TEMPORARY UNTIL I REMOVE MATERIAL FROM SPRITE
                material = this.Material;
            var matcol = material != null ? new Color(material.Color.R, material.Color.G, material.Color.B, (byte)(material.Type.Shininess * 255)) : new Color(1f, 1f, 1f, 0f);
            var t = tint;
            if (this.AtlasToken != null)
                sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, new Vector2(scale), sky, block, matcol, t, fog, sprFx, depth);
            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, screenPos, sky, block, tint, fog, rotation, origin, scale, sprFx, depth);
        }
        public void Draw(MySpriteBatch sb, CharacterColors overlayColors, Vector2 screenPos, MaterialDef material, Color sky, Color block, Color tint, Color fog, float rotation, Vector2 origin, float scale, SpriteEffects sprFx, float depth)
        {
            if (material == null) // TEMPORARY UNTIL I REMOVE MATERIAL FROM SPRITE
                material = this.Material;
            var matcol = material != null ? new Color(material.Color.R, material.Color.G, material.Color.B, (byte)(material.Shine * 255)) : new Color(1f, 1f, 1f, 0f);
            overlayColors.TryGetColor(this.OverlayName, ref matcol); 
            var t = tint;
            if (this.AtlasToken != null)
                sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, new Vector2(scale), sky, block, matcol, t, fog, sprFx, depth);
            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, overlayColors, screenPos, null, sky, block, tint, fog, rotation, origin, scale, sprFx, depth);
        }
        public void Draw(MySpriteBatch sb, Vector2 screenPos, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects sprFx, float depth)
        {
            sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, scale, Color.White, sprFx, depth);
            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, screenPos, color, rotation, origin, scale, sprFx, depth);
        }
        public void Draw(SpriteBatch sb, Vector2 screenPos, Color color, float rotation, Vector2 origin, float scale, SpriteEffects sprFx, float depth)
        {
            if (this.AtlasToken != null)
                sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, color.Multiply(this.Tint), rotation, origin, scale, sprFx, depth);
            else
                if (this.Texture is not null)
                sb.Draw(this.Texture, screenPos, this.SourceRects.First().First(), color.Multiply(this.Tint), rotation, origin, scale, sprFx, depth);
            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, screenPos, color, rotation, origin, scale, sprFx, depth);
        }
    }
}
