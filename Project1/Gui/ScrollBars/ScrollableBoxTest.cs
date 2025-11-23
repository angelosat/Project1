using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Start_a_Town_.UI
{
    public class ScrollableBoxTest : GroupBox
    {
        private const int buttonSize = 16;
        readonly ScrollbarV VScroll;
        readonly ScrollbarH HScroll;
        public GroupBox Viewport;
        public readonly GroupBox Client;
        public override Rectangle ClientSize => this.Viewport.ClientSize;
        readonly ScrollModes Mode;
        public override Control SetOpacity(float value, bool children, params Control[] exclude)
        {
            base.SetOpacity(value, children, exclude);
            this.VScroll.SetOpacity(value, true);
            this.HScroll.SetOpacity(value, true);
            return this;
        }
        public override int Height
        {
            get => base.Height;
            set
            {
                base.Height = value;
                if (this.Viewport is null)
                    return;
                this.Viewport.Height = value - buttonSize * ModeFactor(this.Mode).Y;
                this.VScroll.Height = this.Viewport.Height; 
                this.UpdateScrollbars();
            }
        }
        public int SmallStep
        {
            set => this.VScroll.SmallStep = value;
        }
        static public ScrollableBoxTest FromContentsSize(int w, int h, ScrollModes mode = ScrollModes.Both)
        {
            var mf = ModeFactor(mode);
            return new ScrollableBoxTest(w + buttonSize * mf.X, h + buttonSize * mf.Y, mode);
        }
        public ScrollableBoxTest(int width, int height, ScrollModes mode = ScrollModes.Both) : this(new(), width, height, mode)
        {
        }
        public ScrollableBoxTest(GroupBox container, int width, int height, ScrollModes mode = ScrollModes.Both)
            : base(width, height)
        {
            this.Client = container;
            this.Mode = mode;
            var modeFactor = new IntVec2((mode & ScrollModes.Vertical) == ScrollModes.Vertical ? 1 : 0, (mode & ScrollModes.Horizontal) == ScrollModes.Horizontal ? 1 : 0);
            this.Viewport = new GroupBox(width - buttonSize * modeFactor.X, height - buttonSize * modeFactor.Y) { AutoSize = false };
            this.Viewport.AddControls(this.Client);
            this.VScroll = new ScrollbarV(this.Viewport, this.Client) { Location = new Vector2(this.Viewport.Width, 0) };//, this.Client.Height, this.Client);
            this.HScroll = new ScrollbarH(this.Viewport, this.Client) { Location = new Vector2(0, this.Viewport.Height) };//, this.Client.Height, this.Client);
            this.Controls.Add(this.Viewport);
            this.UpdateScrollbars();
        }
        static IntVec2 ModeFactor(ScrollModes mode) => new((mode & ScrollModes.Vertical) == ScrollModes.Vertical ? 1 : 0, (mode & ScrollModes.Horizontal) == ScrollModes.Horizontal ? 1 : 0);
        public override Control AddControls(params Control[] controls)
        {
            this.Client.AddControls(controls);
            this.UpdateScrollbars();
            return this;
        }
        public override void RemoveControls(params Control[] controls)
        {
            this.Client.RemoveControls(controls);
            this.UpdateScrollbars();
        }
        public override void ClearControls()
        {
            this.Client.ClearControls();
        }
        internal override void OnControlResized(Control control)
        {
            base.OnControlResized(control);
            if (control == this.Viewport)
            {
                this.UpdateScrollbars();
                this.EnsureClientWithinBounds();
            }
        }

        protected virtual void UpdateScrollbars()
        {
            if (this.Client == null)
                return;
            var containerSize = this.Client.Size;
            var containerW = containerSize.Width;
            if ((this.Mode & ScrollModes.Horizontal) == ScrollModes.Horizontal && this.Viewport.Width < containerW)
            {
                if (!this.Controls.Contains(this.HScroll))
                {
                    this.HScroll.Width = this.Viewport.Width;
                    this.Controls.Add(this.HScroll);
                }
            }
            else
                this.Controls.Remove(this.HScroll);

            var containerH = containerSize.Height;
            if ((this.Mode & ScrollModes.Vertical) == ScrollModes.Vertical && this.Viewport.Height < containerH)
            {
                if (!this.Controls.Contains(this.VScroll))
                {
                    this.VScroll.Height = this.Viewport.Height;
                    this.Controls.Add(this.VScroll);
                }
            }
            else
                this.Controls.Remove(this.VScroll);
            this.HScroll.Width = this.Viewport.Width;
            this.VScroll.Height = this.Viewport.Height;
        }
        public override void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.HitTest()) // why hittest again? i hittest during update. just check if has focus
                return;
            e.Handled = true;
            if (this.Client.Height <= this.Viewport.Height)
                /// if nothing to scroll, dont move the client container. added this after chat lines where moved from the bottom to the top when turning the mousewheel once,
                /// even when their height was smaller than the chat window
                return;
            int step = this.VScroll.SmallStep;
            this.Client.Location.Y = Math.Min(0, Math.Max(this.Viewport.Height - this.Client.Height, this.Client.Location.Y + step * e.Delta));
        }

        protected void EnsureClientWithinBounds()
        {
            this.Client.Location.Y = Math.Max(this.Client.Location.Y, Math.Min(0, this.Viewport.Size.Height - this.Client.Height));
        }
        
        class ScrollbarV : GroupBox
        {
            public const int DefaultWidth = 16;
            readonly PictureBox Thumb;
            readonly IconButton Up, Down;
            readonly GroupBox Area;
            private int ThumbClickOrigin;
            bool ThumbMoving;
            public int SmallStep = Label.DefaultHeight;// 1;
            GroupBox Container, Client;
            public override int Height
            {
                get => base.Height; set
                {
                    base.Height = value;
                    this.Area.Height = value - 2 * DefaultWidth;
                    this.Down.Location = this.Area.BottomLeft;
                }
            }
            public ScrollbarV(GroupBox client, GroupBox container)
            {
                this.Container = container;
                this.Client = client;
                this.BackgroundColor = Color.Black * 0.5f;
                this.AutoSize = true;
                this.Width = DefaultWidth;
                var areaheight = client.Height - 2 * DefaultWidth;
                this.Up = new IconButton(Icon.ArrowUp) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = StepUp };
                this.Thumb = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), Alignment.Horizontal.Left, Alignment.Vertical.Top);
                this.Area = new GroupBox() { MouseThrough = false, AutoSize = false, Size = new Rectangle(0, 0, this.Width, areaheight), Location = this.Up.BottomLeft };
                this.Area.AddControls(this.Thumb);
                this.Down = new IconButton(Icon.ArrowDown) { BackgroundTexture = UIManager.Icon16Background, Location = this.Area.BottomLeft, LeftClickAction = StepDown };
                this.AddControls(this.Up, this.Down, this.Area);
            }
            
            void StepUp()
            {
                //this.Container.Location.Y = Math.Min(0, this.Container.Location.Y + this.SmallStep);
                this.MoveContainer(this.Container.Location.Y + this.SmallStep);
            }
            void StepDown()
            {
                //this.Container.Location.Y = Math.Max(this.Client.Height - this.Container.Height, this.Container.Location.Y - this.SmallStep);
                this.MoveContainer(this.Container.Location.Y - this.SmallStep);
            }
            void MoveContainer(float newPos)
            {
                this.Container.Location.Y = Math.Min(0, Math.Max(this.Client.Height - this.Container.Height, newPos));
            }
            public override void HandleLButtonUp(HandledMouseEventArgs e)
            {
                this.ThumbMoving = false;
                base.HandleLButtonUp(e);
            }
            public override void HandleLButtonDown(HandledMouseEventArgs e)
            {
                if (e.Handled)
                    return;
                if (this.WindowManager.ActiveControl == this.Thumb)
                {
                    this.ThumbClickOrigin = (int)(UIManager.MouseTrue.Y - this.Thumb.ScreenLocation.Y);
                    this.ThumbMoving = true;
                    e.Handled = true;
                }
                else if (this.Area.IsTopMost)
                {
                    e.Handled = true;
                    if (UIManager.MouseTrue.Y < this.Thumb.ScreenLocation.Y)
                        this.MoveContainer(this.Container.Location.Y + this.Client.Height);
                    else
                        this.MoveContainer(this.Container.Location.Y - this.Client.Height);
                }
                else
                    base.HandleLButtonDown(e);
            }
            public override void HandleLButtonDoubleClick(HandledMouseEventArgs e)
            {
                this.HandleLButtonDown(e);
            }
            public override void Update()
            {
                float max = this.Container.Height - this.Client.Height;
                this.ResizeThumb();
                var thumbH = this.Thumb.Height;
                if (this.ThumbMoving)
                {
                    this.Thumb.Location.Y = Math.Max(0, Math.Min(this.Size.Height - 32 - thumbH, UIManager.MouseTrue.Y - this.Area.ScreenLocation.Y  - this.ThumbClickOrigin));
                    var val = max * (this.Thumb.Location.Y / (this.Area.Height - thumbH));
                    this.Container.Location.Y = -(int)val;
                }
                else
                {
                    var currentval = Math.Min(0, Math.Max(this.Client.Height - this.Container.Height, this.Container.Location.Y));
                    float pos = (this.Area.Height - thumbH) * currentval / max;
                    this.Thumb.Location.Y = -pos;
                }
                base.Update();
            }
            
            void ResizeThumb()
            {
                float percentage = this.Client.Height / (float)this.Container.Height;
                var height = (int)(this.Area.Height * percentage);
                if (this.Thumb.Height == height)
                    return;
                var newSize = new Rectangle(0, 0, DefaultWidth, height);
                this.Thumb.Size = newSize;
                this.Thumb.Invalidate();
            }

            public void Reset()
            {
                this.Thumb.Location = Vector2.Zero;
            }
        }

        class ScrollbarH : GroupBox
        {
            public const int DefaultHeight = 16;
            readonly PictureBox Thumb;
            readonly IconButton BtnLeft, BtnRight;
            readonly GroupBox Area;
            private int ThumbClickOrigin;
            bool ThumbMoving;
            public int SmallStep = Label.DefaultHeight;// 1;
            readonly GroupBox Container, Client;
            
            public ScrollbarH(GroupBox client, GroupBox container)
            {
                this.Container = container;
                this.Client = client;
                this.BackgroundColor = Color.Black * 0.5f;
                this.AutoSize = true;
                this.Height = DefaultHeight;
                var arealength = client.Width - 2 * DefaultHeight;
                this.BtnLeft = new IconButton(Icon.ArrowUp) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = StepUp };
                this.Thumb = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), Alignment.Horizontal.Left, Alignment.Vertical.Top);
                this.Area = new GroupBox() { MouseThrough = false, AutoSize = false, Size = new Rectangle(0, 0, arealength, this.Height), Location = this.BtnLeft.TopRight };
                this.Area.AddControls(this.Thumb);
                this.BtnRight = new IconButton(Icon.ArrowDown) { BackgroundTexture = UIManager.Icon16Background, Location = this.Area.TopRight, LeftClickAction = StepDown };
                this.AddControls(this.BtnLeft, this.BtnRight, this.Area);
            }

            void StepUp()
            {
                this.MoveContainer(this.Container.Location.X + this.SmallStep);
            }
            void StepDown()
            {
                this.MoveContainer(this.Container.Location.X - this.SmallStep);
            }
            void MoveContainer(float newPos)
            {
                this.Container.Location.X = Math.Min(0, Math.Max(this.Client.Width - this.Container.Width, newPos));
            }
            public override void HandleLButtonUp(HandledMouseEventArgs e)
            {
                this.ThumbMoving = false;
                base.HandleLButtonUp(e);
            }
            public override void HandleLButtonDown(HandledMouseEventArgs e)
            {
                if (e.Handled)
                    return;
                if (this.WindowManager.ActiveControl == this.Thumb)
                {
                    this.ThumbClickOrigin = (int)(UIManager.MouseTrue.X - this.Thumb.ScreenLocation.X);
                    this.ThumbMoving = true;
                    e.Handled = true;
                }
                else if (this.Area.IsTopMost)
                {
                    e.Handled = true;
                    if (UIManager.MouseTrue.X < this.Thumb.ScreenLocation.X)
                        this.MoveContainer(this.Container.Location.X + this.Client.Width);
                    else
                        this.MoveContainer(this.Container.Location.X - this.Client.Width);
                }
                else
                    base.HandleLButtonDown(e);
            }
            public override void HandleLButtonDoubleClick(HandledMouseEventArgs e)
            {
                this.HandleLButtonDown(e);
            }
            public override void Update()
            {
                float max = this.Container.Width - this.Client.Width;
                this.ResizeThumb();
                var thumbH = this.Thumb.Width;
                if (this.ThumbMoving)
                {
                    this.Thumb.Location.X = Math.Max(0, Math.Min(this.Size.Width - 32 - thumbH, UIManager.MouseTrue.X - this.Area.ScreenLocation.X - this.ThumbClickOrigin));
                    var val = max * (this.Thumb.Location.X / (this.Area.Width - thumbH));
                    this.Container.Location.X = -val;
                }
                else
                {
                    var currentval = Math.Min(0, Math.Max(this.Client.Width - this.Container.Width, this.Container.Location.X));
                    float pos = (this.Area.Width - thumbH) * currentval / max;
                    this.Thumb.Location.X = -pos;
                }
                base.Update();
            }

            void ResizeThumb()
            {
                float percentage = this.Client.Width / (float)this.Container.Width;
                var w = (int)(this.Area.Width * percentage);
                if (this.Thumb.Width == w)
                    return;
                var newSize = new Rectangle(0, 0, w, DefaultHeight);
                this.Thumb.Size = newSize;
                this.Thumb.Invalidate();
            }

            public void Reset()
            {
                this.Thumb.Location = Vector2.Zero;
            }
        }
    }
}
