using Microsoft.Xna.Framework;
using System;
using System.Windows.Forms;

namespace Start_a_Town_.UI
{
    class ScrollbarH : Control
    {
        public static int TextureHeight = 16;
        readonly PictureBox Thumb, Btn_Right, Btn_Left;
        readonly GroupBox Area;
        readonly float Step = 3 * Label.DefaultHeight;
        int ThumbOffset;
        bool ThumbMoving;

        public override int Width
        {
            get => base.Width;
            set
            {
                base.Width = value;
                this.Btn_Right.Location.X = value - 16;
            }
        }

        public ScrollbarH(Vector2 location, int width, object tag = null)
            : base(location)
        {
            this.BackgroundColor = Color.Black * 0.5f;
            this.Tag = tag;
            this.AutoSize = true;
            TextureHeight = 16;
            this.Btn_Left = new PictureBox(Vector2.Zero, UIManager.DefaultHScrollbarSprite, new Rectangle(0, 0, 16, 16), Alignment.Horizontal.Left, Alignment.Vertical.Top);
            this.Btn_Right = new PictureBox(new Vector2(width - 16, 0), UIManager.DefaultHScrollbarSprite, new Rectangle(32, 0, 16, 16), Alignment.Horizontal.Left, Alignment.Vertical.Top);
            this.Thumb = new PictureBox(new Vector2(16, 0), UIManager.DefaultHScrollbarSprite, new Rectangle(16, 0, 16, 16), Alignment.Horizontal.Left, Alignment.Vertical.Top);

            //this.Btn_Left.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(this.Left_Click);
            //this.Btn_Right.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(this.Right_Click);
            //this.Thumb.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(this.Thumb_Click);
            this.Btn_Left.LeftClickAction = this.Left_Click;
            this.Btn_Right.LeftClickAction = this.Right_Click;
            this.Thumb.LeftClickAction = this.Thumb_Click;

            this.Area = new GroupBox() { Size = new Rectangle(0, 0, width - 32, TextureHeight), Location = this.Btn_Left.TopRight, MouseThrough = false };
            //this.Area.MouseLeftPress += this.Area_MouseLeftPress;
            this.Area.MouseLBAction = this.Area_MouseLeftPress;
            this.Controls.Add(this.Btn_Left, this.Btn_Right, this.Area, this.Thumb);
        }

        void Area_MouseLeftPress()
        {
            Control scr = this.Tag as Control;
            if (this.Tag is null)
                return;

            if (UIManager.Mouse.X < this.Thumb.ScreenLocation.X)
            {
                scr.ClientLocation.X += scr.Size.Width;
                scr.ClientLocation.X = Math.Min(0, scr.ClientLocation.X);
            }
            else if (UIManager.Mouse.X > this.Thumb.ScreenLocation.X + this.Thumb.Width)
            {
                scr.ClientLocation.X -= scr.Size.Width;
                scr.ClientLocation.X = Math.Max(scr.Size.Width - scr.ClientSize.Width, scr.ClientLocation.X);
            }
        }

        public override void HandleLButtonUp(HandledMouseEventArgs e)
        {
            this.ThumbMoving = false;
        }

        public override void Update()
        {
            Control scr = this.Tag as Control;
            if (scr is not null)
            {
                float percentage = scr.Size.Width / (float)scr.ClientSize.Width;
                //this.Thumb.Size = new Rectangle(0, 0, (int)((this.Size.Width - 32) * percentage), 16);
                float pos = -scr.ClientLocation.X / scr.ClientSize.Width;

                this.Thumb.Width = (int)((this.Size.Width - 32) * percentage);
                if (this.ThumbMoving)
                {
                    this.Thumb.Location.X = 16 + Math.Max(0, Math.Min(this.Size.Width - 32 - this.Thumb.Width, (UIManager.Mouse.X - this.ThumbOffset)));
                    scr.ClientLocation.X = -(scr.ClientSize.Width * (this.Thumb.Location.X - 16) / (this.Size.Width - 32));
                }
                else
                {
                    this.Thumb.Location.X = 16 + ((pos) * (this.Size.Width - 32));
                }
            }
            base.Update();
        }

        void Thumb_Click()
        {
            if (this.Tag is not null)
            {
                this.ThumbMoving = true;
                this.ThumbOffset = (int)(16 + UIManager.Mouse.X - this.Thumb.Location.X);
            }
        }

        void Right_Click()
        {
            Control scr = this.Tag as Control;
            if (this.Tag is null)
                return;

            scr.ClientLocation.X -= this.Step;
            scr.ClientLocation.X = Math.Max(scr.Size.Width - scr.ClientSize.Width, scr.ClientLocation.X);
        }

        void Left_Click()
        {
            Control scr = this.Tag as Control;
            if (this.Tag is null)
                return;

            scr.ClientLocation.X += this.Step;
            scr.ClientLocation.X = Math.Min(0, scr.ClientLocation.X);
        }
    }
}
