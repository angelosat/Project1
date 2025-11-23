using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using System;

namespace Start_a_Town_.UI
{
    public class Icon
    {
        public static readonly Icon X = new(UIManager.IconX);
        public static readonly Icon ArrowUp = new(UIManager.ArrowUp);
        public static readonly Icon ArrowDown = new(UIManager.ArrowDown);
        public static readonly Icon ArrowLeft = new(UIManager.ArrowLeft);
        public static readonly Icon ArrowRight = new(UIManager.ArrowRight);
        public static readonly Icon Cursor = new(UIManager.MousePointer);
        public static readonly Icon CursorGrayscale = new(UIManager.MousePointerGrayscale);
        public static readonly Icon Construction = new(UIManager.SpriteConstruction);
        public static readonly Icon Cross = new(UIManager.Icons16x16, 0, 16);
        public static readonly Icon Replace = new(UIManager.Icons16x16, 3, 16);
        public static readonly Icon Dice = new(UIManager.Icons16x16, 1, 16);

        public Color Tint = Color.White;
        public Texture2D SpriteSheet;
        public Rectangle SourceRect;
        public IAtlasNodeToken AtlasToken { get; set; }
        public int Width => this.SourceRect.Width;
        public int Height => this.SourceRect.Height;

        public Icon(Texture2D spritesheet, uint index, int size)
        {
            this.SpriteSheet = spritesheet;
            this.SourceRect = new Rectangle((int)index % (this.SpriteSheet.Width / size) * size, ((int)index / (this.SpriteSheet.Width / size)) * size, size, size);
        }
        [Obsolete]
        public Icon(string assetName)
        {
            /// TODO why load it in the sprite atlas???
            this.AtlasToken = Sprite.Atlas.Load(assetName);
        }
        public Icon(Texture2D spritesheet, Rectangle source)
        {
            this.SpriteSheet = spritesheet;
            this.SourceRect = source;
        }
        public Icon(Sprite sprite)
        {
            sprite ??= Sprite.Default;
            if (sprite.AtlasToken != null)
            {
                this.AtlasToken = sprite.AtlasToken;
                this.SpriteSheet = Sprite.Atlas.Texture;
                this.SourceRect = sprite.AtlasToken.Rectangle;
            }
            else
            {
                this.SpriteSheet = sprite.Texture;
                this.SourceRect = sprite.SourceRects[0][0];
            }
            this.Tint = sprite.Tint;
        }
        public Icon(IAtlasNodeToken token)
        {
            this.AtlasToken = token;
            this.SpriteSheet = token.Atlas.Texture;
            this.SourceRect = token.Rectangle;
        }
        public override string ToString()
        {
            return "Sprite sheet: " + this.SpriteSheet.Name;
        }


        public void Draw(SpriteBatch sb, Vector2 loc, Rectangle? sourceRect)
        {
            if (this.AtlasToken is null)
                sb.Draw(this.SpriteSheet, loc, sourceRect.HasValue ? Rectangle.Intersect(this.SourceRect, new Rectangle(this.SourceRect.X + sourceRect.Value.X, this.SourceRect.Y + sourceRect.Value.Y, sourceRect.Value.Width, sourceRect.Value.Height)) : this.SourceRect, Color.White);
            else
            {
                sb.Draw(this.AtlasToken.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White);
            }
        }
        public void Draw(SpriteBatch sb, Vector2 loc, Vector2 originPercentage)
        {
            if (this.AtlasToken is null)
                sb.Draw(this.SpriteSheet, new Vector2((int)loc.X, (int)loc.Y), this.SourceRect, Color.White, 0, new Vector2(this.SourceRect.Width * originPercentage.X, this.SourceRect.Height * originPercentage.Y), 1, SpriteEffects.None, 0);
            else
            {
                var sourceRect = this.AtlasToken.Rectangle;
                sb.Draw(this.AtlasToken.Atlas.Texture, loc.ToFloored(), sourceRect, Color.White, 0, new Vector2(sourceRect.Width * originPercentage.X, sourceRect.Height * originPercentage.Y), 1, SpriteEffects.None, 0);
            }
        }

        public void Draw(SpriteBatch sb, Vector2 loc, float scale = 1, float alpha = 1)
        {
            this.Draw(sb, loc, Color.White * alpha, scale);
        }
        public void Draw(SpriteBatch sb, Vector2 loc, Color color, float scale = 1)
        {
            if (this.AtlasToken is null)
                sb.Draw(this.SpriteSheet, loc, this.SourceRect, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            else
                sb.Draw(this.AtlasToken.Atlas.Texture, loc, this.AtlasToken.Rectangle, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
        /// <summary>
        /// Draws near mouse
        /// </summary>
        /// <param name="sb"></param>
        public void Draw(SpriteBatch sb)
        {
            this.Draw(sb, UI.UIManager.Mouse + new Vector2(16, 0));
        }

        public void DrawFloating(SpriteBatch sb, Camera camera, GameObject entity, float scale = .5f)
        {
            var bounds = entity.GetSprite().GetBounds();
            var offset = MapBase.IconOffset;
            scale *= camera.Zoom;
            var pos = camera.GetScreenPosition(entity.Global) - new Vector2(this.SourceRect.Width, this.SourceRect.Height) * scale / 2; ;
            pos.Y -= bounds.Height * camera.Zoom;
            pos.Y += offset * this.SourceRect.Height / 4 * scale;
            this.Draw(sb, pos, scale, alpha: .5f);
        }
        public void DrawFloating(SpriteBatch sb, Camera camera, Vector3 global, float scale = .5f)
        {
            var bounds = Block.Bounds;
            var offset = -1 + 2*UIManager.FlashingValue;
            //var offset = MapBase.IconOffset;
            scale *= camera.Zoom;
            var pos = camera.GetScreenPosition(global) - new Vector2(this.SourceRect.Width, this.SourceRect.Height) * scale / 2; ;
            pos.Y -= (bounds.Height / 2) * camera.Zoom ;
            pos.Y += offset * this.SourceRect.Height / 4 * scale;
            this.Draw(sb, pos, scale, alpha: .5f);
        }
    }
}
