using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Panel : Control
    {
        public override void OnPaint(SpriteBatch sb)
        {
            Color tint = this.Color;
            this.BackgroundStyle.Draw(sb, this.Size, tint);
        }
        public static readonly BackgroundStyle DefaultStyle = BackgroundStyle.Window;
        public override Color Tint { get => UIManager.TintSecondary; set => base.Tint = value; }
        public override Rectangle ContainerSize => this.ClientSize;
        public static Color DefaultColor = Color.DarkSlateGray;// Color.Black; //Color.DarkSlateGray;// 
        public Rectangle GetClientSize()
        {
            return new Rectangle(0, 0, this.Width - 2 * UIManager.BorderPx, this.Height - 2 * UIManager.BorderPx);
        }
        public override Vector2 Dimensions
        {
            get => base.Dimensions;
            set
            {
                base.Dimensions = value;
                this.ClientSize = new Rectangle(0, 0, this.Width - 2 * UIManager.BorderPx, this.Height - 2 * UIManager.BorderPx);
            }
        }
        public override int Padding => this.BackgroundStyle.Border;
        public Panel(Vector2 location)
            : base(location)
        {
            this.ClientLocation = new Vector2(UIManager.BorderPx);
            this.BackgroundStyle = DefaultStyle;
            //this.Color = DefaultColor;
        }
        public Panel(Vector2 location, Vector2 size)
            : base(location, size)
        {
            this.ClientLocation = new Vector2(UIManager.BorderPx);
            this.ClientSize = new Rectangle(0, 0, this.Width - 2 * UIManager.BorderPx, this.Height - 2 * UIManager.BorderPx);
            this.BackgroundStyle = DefaultStyle;
            //this.Color = DefaultColor;
        }
        public Panel(int size) : this(new Rectangle(0, 0, size, size))
        {

        }
        public Panel(Rectangle rect)
            : base(new Vector2(rect.X, rect.Y), new Vector2(rect.Width, rect.Height))
        {
            this.ClientLocation = new Vector2(UIManager.BorderPx);
            this.ClientSize = new Rectangle(0, 0, this.Width - 2 * UIManager.BorderPx, this.Height - 2 * UIManager.BorderPx);
            this.BackgroundStyle = DefaultStyle;
            this.Width = rect.Width;
            this.Height = rect.Height;
            //this.Color = DefaultColor;
        }
        public Panel()
        {
            this.ClientLocation = new Vector2(UIManager.BorderPx);
            this.BackgroundStyle = DefaultStyle;
            //this.Color = DefaultColor;
        }
        public Panel(int x, int y, int w, int h) : this(new Rectangle(x, y, w, h)) { }

        public static Panel FromClientSize(int w, int h)
        {
            var panel = new Panel(Vector2.Zero, new(w + 2 * UIManager.BorderPx, h + 2 * UIManager.BorderPx));
            //panel.ClientSize = new Rectangle(0, 0, w, h);

            //panel.Width = w + 2 * panel.BackgroundStyle.Border;
            //panel.Height = h + 2 * panel.BackgroundStyle.Border;

            return panel;
        }

        public static int GetClientWidth(int totalWidth)
        {
            return totalWidth - UIManager.BorderPx - UIManager.BorderPx;
        }
        public static int GetClientHeight(int totalHeight)
        {
            return totalHeight - UIManager.BorderPx - UIManager.BorderPx;
        }

        public static Vector2 GetClientSize(Vector2 boundsSize)
        {
            return boundsSize - 2 * new Vector2(UIManager.BorderPx);
        }
        public static int GetClientLength(int totalLength)
        {
            return totalLength - 2 * UIManager.BorderPx;
        }

        public override Rectangle ClientSize
        {
            get => base.ClientSize;
            set => base.ClientSize = value;
        }
        public Panel SetClientDimensions(Rectangle clientSize)
        {
            this.ClientSize = clientSize;
            this.Width = this.ClientSize.Width + 2 * this.BackgroundStyle.Border;
            this.Height = this.ClientSize.Height + 2 * this.BackgroundStyle.Border;
            return this;
        }
        public Panel SetClientDimensions(Vector2 clientDimensions)
        {
            this.ClientSize = new Rectangle(0, 0, (int)clientDimensions.X, (int)clientDimensions.Y);
            this.Width = this.ClientSize.Width + 2 * this.BackgroundStyle.Border;
            this.Height = this.ClientSize.Height + 2 * this.BackgroundStyle.Border;
            return this;
        }
        public Panel SetClientDimensions(int w, int h)
        {
            this.ClientSize = new Rectangle(0, 0, w, h);
            this.Width = this.ClientSize.Width + 2 * this.BackgroundStyle.Border;
            this.Height = this.ClientSize.Height + 2 * this.BackgroundStyle.Border;
            return this;
        }
    }
}
