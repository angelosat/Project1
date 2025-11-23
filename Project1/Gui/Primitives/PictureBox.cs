using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using System;

namespace Start_a_Town_.UI
{
    public class PictureBox : ButtonBase
    {
        //public Action<PictureBox> Animate = (box) => { };
        public Func<float> RotationFunc;

        Action<RenderTarget2D> _renderer;

        public Action<RenderTarget2D> Renderer
        {
            get => this._renderer;
            set
            {
                this._renderer = value;
                this.Sprite = new RenderTarget2D(Game1.Instance.GraphicsDevice, this.BoundsScreen.Width, this.BoundsScreen.Height);
            }
        }

        protected Texture2D _sprite;
        public Texture2D Sprite
        {
            get => this._sprite;
            set
            {
                this._sprite = value;
                this.Width = this.SourceRect.Width;
                this.Height = this.SourceRect.Height;
                this.Invalidate();
            }
        }

        Rectangle? _SourceRect;
        public Rectangle SourceRect
        {
            get => this._SourceRect.GetValueOrDefault(this.Sprite is null ? new Rectangle(0, 0, 0, 0) : this.Sprite.Bounds);
            set
            {
                this._SourceRect = value;
                this.Width = this.SourceRect.Width;
                this.Height = this.SourceRect.Height;
                this.Invalidate();
                switch (this.HAlignment)
                {
                    case Alignment.Horizontal.Center:
                        this.PictureOrigin = new Vector2(this.SourceRect.Width / 2, this.SourceRect.Height / 2);
                        break;
                    case Alignment.Horizontal.Right:
                        this.PictureOrigin.X = this.SourceRect.Width;
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Validate(bool cascade = false)
        {
            if (this.DrawAction == null)
            {
                if (this.Renderer != null)
                    this.Renderer(this.Sprite as RenderTarget2D);

                base.Validate(cascade);
                return;
            }
            else
            {
                var gd = Game1.Instance.GraphicsDevice;
                this.Texture = this.CreateTexture(gd);
                gd.SetRenderTarget(this.Texture);
                gd.Clear(Color.Transparent);
                this.DrawAction();
                gd.SetRenderTarget(null);
                this.Valid = true;
            }
        }
        public override void OnPaint(SpriteBatch sb)
        {
            sb.Draw(this.Sprite, Vector2.Zero, this.SourceRect, Color.White);
        }

        public override void Update()
        {
            //this.Animate(this);
            if (this.RotationFunc is not null)
                this.Rotation = this.RotationFunc();
            base.Update();
        }
        public Action DrawAction;
        public Vector2 PictureOrigin = new Vector2(0);

        protected override RenderTarget2D CreateTexture(GraphicsDevice gd)
        {
            return new RenderTarget2D(Game1.Instance.GraphicsDevice, this.SourceRect.Width, this.SourceRect.Height);
            //return new RenderTarget2D(Game1.Instance.GraphicsDevice, this.SourceRect.Width, this.SourceRect.Height, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
        }
        public Alignment.Horizontal HAlignment;

        public PictureBox() { }
        public PictureBox(Rectangle bounds, float scale)
        {
            this.Width = (int)(bounds.Width * scale);
            this.Height = (int)(bounds.Height * scale);
            this.SourceRect = new Rectangle(0, 0, this.Width, this.Height);
            this.Sprite = this.CreateTexture(Game1.Instance.GraphicsDevice);
        }
        public PictureBox(Vector2 size, Action<RenderTarget2D> renderer, Alignment.Horizontal halign = Alignment.Horizontal.Left, Alignment.Vertical valign = Alignment.Vertical.Top)
        {
            this.SourceRect = new Rectangle(0, 0, (int)size.X, (int)size.Y);
            this.Width = this.SourceRect.Width;
            this.Height = this.SourceRect.Height;
            this.Renderer = renderer;

            this.HAlignment = halign;
            switch (halign)
            {
                case Alignment.Horizontal.Center:
                    this.PictureOrigin = new Vector2(this.SourceRect.Width / 2, this.SourceRect.Height / 2);
                    break;
                case Alignment.Horizontal.Right:
                    this.PictureOrigin.X = this.SourceRect.Width;
                    break;
                default:
                    break;
            }
        }
        public PictureBox(Vector2 location, Texture2D sprite, Rectangle? sourcerect, Alignment.Horizontal halign = Alignment.Horizontal.Left, Alignment.Vertical valign = Alignment.Vertical.Top)
            : base(location)
        {
            this.Sprite = sprite;
            this.SourceRect = sourcerect.HasValue ? sourcerect.Value : sprite.Bounds;
            this.Width = this.SourceRect.Width;
            this.Height = this.SourceRect.Height;

            this.HAlignment = halign;
            switch (halign)
            {
                case Alignment.Horizontal.Center:
                    this.PictureOrigin = new Vector2(this.SourceRect.Width / 2, this.SourceRect.Height / 2);
                    break;
                case Alignment.Horizontal.Right:
                    this.PictureOrigin.X = this.SourceRect.Width;
                    break;
                default:
                    break;
            }
        }
        public PictureBox(Entity item, float scale)
            : this(item.Body.RenderIcon(item, scale))
        {
        }
        [Obsolete]
        public PictureBox(Bone bone, float scale)
        {
            this.Width = (int)(32 * scale);
            this.Height = (int)(32 * scale);
            this.SourceRect = new Rectangle(0, 0, this.Width, this.Height);
            this.DrawAction = () => GameObject.DrawIcon(bone, this.Width, this.Height, scale);
        }
        public PictureBox(IAtlasNodeToken token) : this(Vector2.Zero, token.Atlas.Texture, token.Rectangle)
        {
        }
        public PictureBox(Texture2D texture2D, Rectangle? rectangle = null, float scale = 1)
            : this(Vector2.Zero, texture2D, rectangle)
        {
            this.Width = (int)((rectangle?.Width ?? texture2D.Width) * scale);
            this.Height = (int)((rectangle?.Height ?? texture2D.Height) * scale);
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(this.Sprite, this.BoundsScreen, Color.White);

            base.Draw(sb);
            if (this.Active && this.MouseHover)
                sb.Draw(this.Texture, this.BoundsScreen, this.SourceRect, new Color(1, 1, 1, 0.5f), this.Rotation, this.PictureOrigin, SpriteEffects.None, 0);
        }

        internal void SetTexture(Atlas.Node.Token token)
        {
            this.SourceRect = token.Rectangle;
            this.Sprite = token.Atlas.Texture;
            this.Validate();
        }
    }
}
