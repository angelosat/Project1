using Microsoft.Xna.Framework;
using System;
using System.Windows.Forms;

namespace Start_a_Town_.UI
{
    class ScrollbarVNew : GroupBox
    {
        public const int DefaultWidth = 16;
        readonly PictureBox Thumb;
        readonly IconButton Up, Down;
        readonly GroupBox Area;
        private int ThumbClickOrigin;
        private readonly int MaxValue;
        private readonly int ThumbRange;
        bool ThumbMoving;
        readonly Action<int> ThumbValueChangeFunc;
        readonly int SmallStep = 1, LargeStep;
        readonly Func<int> ValueFunc;

        public ScrollbarVNew(int areaheight, int maxValue, int step, int largeStep, int thumbRange, Func<int> valueFunc, Func<float> percentageFunc, Func<int> positionFunc, Action<int> thumbValueChangeFunc)
        {
            this.ThumbValueChangeFunc = thumbValueChangeFunc;
            this.MaxValue = maxValue;
            this.ThumbRange = thumbRange;
            this.ValueFunc = valueFunc;
            this.BackgroundColor = Color.Black * 0.5f;
            this.SmallStep = step;
            this.LargeStep = largeStep;
            areaheight = Math.Max(areaheight, maxValue + UIManager.Icon16Background.Height);
            var height = areaheight + 2 * UIManager.Icon16Background.Height;
            this.AutoSize = true;
            this.Width = DefaultWidth;
            this.Up = new IconButton(Icon.ArrowUp) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = StepUp };
            this.Thumb = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), Alignment.Horizontal.Left, Alignment.Vertical.Top);
            this.Area = new GroupBox() { MouseThrough = false, AutoSize = false, Size = new Rectangle(0, 0, this.Width, areaheight), Location = this.Up.BottomLeft };
            this.Area.AddControls(this.Thumb);
            this.Down = new IconButton(Icon.ArrowDown) { BackgroundTexture = UIManager.Icon16Background, Location = this.Area.BottomLeft, LeftClickAction = StepDown };
            this.AddControls(this.Up, this.Down, this.Area);
        }

        void StepUp()
        {
            this.ThumbValueChangeFunc(this.ValueFunc() - this.SmallStep);
        }
        void StepDown()
        {
            this.ThumbValueChangeFunc(this.ValueFunc() + this.SmallStep);
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
                this.ThumbClickOrigin = (int)(UIManager.Mouse.Y - this.Thumb.Location.Y);
                this.ThumbMoving = true;
                e.Handled = true;
            }
            else if (this.Area.IsTopMost)
            {
                e.Handled = true;
                if (UIManager.Mouse.Y < this.Thumb.ScreenLocation.Y)
                    this.ThumbValueChangeFunc(this.ValueFunc() - this.LargeStep);
                else
                    this.ThumbValueChangeFunc(this.ValueFunc() + this.LargeStep);
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
            float percentage = this.ThumbRange / (float)this.MaxValue;
            this.ResizeThumb(DefaultWidth);
            var currentval = this.ValueFunc();
            var thumbH = this.Thumb.Height;
            float pos = (this.Area.Height - thumbH) * currentval / (float)this.MaxValue;
            if (this.ThumbMoving)
            {
                this.Thumb.Location.Y = Math.Max(0, Math.Min(this.Size.Height - 32 - thumbH, (UIManager.Mouse.Y - this.ThumbClickOrigin) / UIManager.Scale));
                var val = this.MaxValue * (this.Thumb.Location.Y / (this.Area.Height - thumbH));
                this.ThumbValueChangeFunc((int)val);
            }
            else
                this.Thumb.Location.Y = pos;
            base.Update();
        }
        void ResizeThumb(int height)
        {
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
        void Thumb_Click(object sender, HandledMouseEventArgs e)
        {
            if (this.Tag is null)
                return;
            this.ThumbMoving = true;
            this.ThumbClickOrigin = (int)(DefaultWidth + UIManager.Mouse.Y - this.Thumb.Location.Y);
            e.Handled = true;
        }

        void Down_Click(object sender, HandledMouseEventArgs e)
        {
            if (this.Tag is not Control scr)
                return;
            scr.ClientLocation.Y -= this.SmallStep;
            scr.ClientLocation.Y = Math.Max(scr.Size.Height - scr.ClientSize.Height, scr.ClientLocation.Y);
            e.Handled = true;
        }

        void Up_Click(object sender, HandledMouseEventArgs e)
        {
            if (this.Tag is not Control scr)
                return;

            scr.ClientLocation.Y += this.SmallStep;
            scr.ClientLocation.Y = Math.Min(0, scr.ClientLocation.Y);
            e.Handled = true;
        }
    }
}
