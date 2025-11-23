using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Start_a_Town_.UI
{
    public class Window : Control
    {
        struct OuterPadding
        {
            public static Rectangle
                TopLeft = new(0, 0, 11, 11),
                TopRight = new(12, 0, 11, 11),
                BottomLeft = new(0, 12, 11, 11),
                BottomRight = new(12, 12, 11, 11),
                Top = new(12, 0, 1, 11),
                Left = new(0, 12, 11, 1),
                Right = new(12, 11, 11, 1),
                Bottom = new(11, 12, 1, 11),
                Center = new(11, 11, 1, 1);
        }
        public override string ToString()
        {
            return "Window: " + this._title;
        }
        private string _title = "<undefined>";
        readonly bool isResizable = false;
        public bool FocusLast = false, isMouseOverCurrent = false, isMouseOverPrevious = false;
        public bool Movable, IsDragged;
        protected Rectangle ScreenClientBounds = new();
        protected bool _closable = true;
        public bool Closable
        {
            get => this._closable;
            set
            {
                bool oldClosable = this.Closable;
                this._closable = value;
                if (this._closable != oldClosable)
                {
                    if (this._closable)
                        this.Controls.Add(this.CloseButton);
                    else
                        this.Controls.Remove(this.CloseButton);

                    if (this.AutoSize)
                        this.ClientSize = this.PreferredClientSize;
                }
            }
        }

        public override void PreparingPaint()
        {
        }

        public override void OnPaint(SpriteBatch sb)
        {
            this.DrawOuterPadding(sb, Vector2.Zero);
        }
        public override Control AddControls(params Control[] controls)
        {
            foreach (var ctrl in controls)
            {
                this.Client.Controls.Add(ctrl);
            }

            return this;
        }
        public Label Label_Title;
        public GroupBox Client;
        protected IconButton CloseButton;
        protected override void OnClientSizeChanged()
        {
            this.Size = new Rectangle(0, 0, this.ClientSize.Width + 2 * UIManager.BorderPx, this.ClientSize.Height + 2 * UIManager.BorderPx + (int)UIManager.Font.MeasureString(this.Title).Y);
            base.OnClientSizeChanged();
        }
        protected virtual void OnClosed()
        {
        }

        public virtual bool Close()
        {
            return this.Hide();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public Vector2 CenterMouseOnControl(Control control)
        {
            this.Location = Vector2.Zero;
            Vector2 controlLoc = control.GetLocation();
            return UIManager.Mouse - controlLoc - new Vector2(control.Width, control.Height) / 2;
        }

        public Window(Control child, bool closable = true) : this("", child, closable)
        {
        }
        public Window(string name, Control child, bool closable = true) : this()
        {
            this.Title = name;
            this.Movable = true;
            this.AutoSize = true;
            this.Closable = closable;
            this.Client.AddControls(child);
        }
        public Window()
        {
            //this.Padding = UIManager.BorderPx;
            //this.Color = UIManager.Tint;
            //this.ColorFunc = () => UIManager.Tint;
            this.TintFunc = () => UIManager.TintPrimary;
            this.ClientLocation = new Vector2(UIManager.BorderPx, UIManager.BorderPx);
            this.Client = new GroupBox() { Name = "Window client area", MouseThrough = true, Size = ClientSize,
                Location = new Vector2(0, Label.DefaultHeight)
            };
            this.CloseButton = IconButton.CreateCloseButton();
            this.CloseButton.LocationFunc = () => new Vector2(this.Width - this.CloseButton.Width - UIManager.BorderPx - this.ClientLocation.X, UIManager.BorderPx - this.ClientLocation.Y);
            this.CloseButton.LeftClickAction = () => this.Hide();
            this.Label_Title = new Label() { Font = UIManager.FontBold, MouseThrough = true };
            this.AutoSize = true;
            this.Controls.Add(
                this.Label_Title, 
                this.CloseButton, this.Client);
            this.AutoSize = false;
            this.Select();
        }

        public override void Update()
        {
            base.Update();
            if (this.TitleFunc is not null)
            {
                var nowTitle = this.TitleFunc();
                if (this.LastTitle != nowTitle)
                    this.OnTitleChanged();
                this.LastTitle = nowTitle;
            }
            this.Drag();
            this.ScreenClientBounds = new Rectangle((int)this.ScreenClientLocation.X - UIManager.BorderPx, (int)this.ScreenClientLocation.Y - UIManager.BorderPx, this.ClientSize.Width + 2 * UIManager.BorderPx, this.ClientSize.Height + 2 * UIManager.BorderPx);
            this.FocusLast = this.Focused;
        }

        private void Drag()
        {
            if (this.IsDragged)
            {
                this.Location.X = Math.Max(0, Math.Min(UIManager.Width - this.Width, UIManager.MouseTrue.X - (int)this.MouseOffset.X));
                this.Location.Y = Math.Max(0, Math.Min(UIManager.Height - this.Height, UIManager.MouseTrue.Y - (int)this.MouseOffset.Y));
            }
        }
        public override bool MouseThrough
        {
            get
            {
                return base.MouseThrough;
            }
            set
            {
                base.MouseThrough = value;
                this.Client.MouseThrough = value;
            }
        }
        protected static Vector2 TitleLocation = new Vector2(UIManager.BorderPx * 2, UIManager.BorderPx);
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        private void DrawOuterPadding(SpriteBatch sb, Vector2 loc)
        {
            var color = this.Color;
            sb.Draw(UIManager.FrameSprite, loc, OuterPadding.TopLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.FrameSprite, loc + new Vector2(-11 + this.Width, 0), OuterPadding.TopRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.FrameSprite, loc + new Vector2(0, -11 + this.Height), OuterPadding.BottomLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.FrameSprite, loc + new Vector2(-11 + this.Width, -11 + this.Height), OuterPadding.BottomRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(UIManager.FrameSprite, new Rectangle((int)loc.X + 11, (int)loc.Y, this.Width - 22, 11), OuterPadding.Top, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.FrameSprite, new Rectangle((int)loc.X, (int)loc.Y + 11, 11, this.Height - 22), OuterPadding.Left, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.FrameSprite, new Rectangle((int)loc.X + this.Width - 11, (int)loc.Y + 11, 11, this.Height - 22), OuterPadding.Right, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.FrameSprite, new Rectangle((int)loc.X + 11, (int)loc.Y - 11 + this.Height, this.Width - 22, 11), OuterPadding.Bottom, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(UIManager.FrameSprite, new Rectangle((int)loc.X + 11, (int)loc.Y + 11, this.Width - 22, this.Height - 22), OuterPadding.Center, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }

        internal Window SetTitle(string text)
        {
            this.Title = text;
            return this;
        }

        string LastTitle;
        public Func<string> TitleFunc;
        public string Title
        {
            get { return this.TitleFunc != null ? this.TitleFunc() : this._title; }
            set
            {
                this._title = value;
                this.OnTitleChanged();
            }
        }
        public bool IsResizable
        { get { return this.isResizable; } }


        public void SizeToControl(Control control)
        {
            this.Size = new Rectangle(0, 0, control.Width + 2 * UIManager.BorderPx, control.Height + 2 * UIManager.BorderPx + (int)UIManager.Font.MeasureString(this.Title).Y);
        }
        public void OnTitleChanged()
        {
            if (this.Title.Length == 0)
            {
                this.Controls.Remove(this.Label_Title);
                this.Client.Location = Vector2.Zero;
            }
            else
            {
                this.Label_Title.Text = this.Title;
                if (!this.Controls.Contains(this.Label_Title))
                {
                    this.Controls.Add(this.Label_Title);
                    this.Client.Location = this.Label_Title.BottomLeft;
                }

            }
            this.ClientSize = this.PreferredClientSize;
        }

        public override Vector2 Dimensions
        {
            get => base.Dimensions;
            set
            {
                base.Dimensions = value;
                this.Client.Dimensions = new Vector2(this.Width, this.Height) - new Vector2(UIManager.BorderPx * 2) - new Vector2(0, (int)UIManager.Font.MeasureString(this.Title).Y);
            }
        }

        public virtual bool ToggleDialog()
        {
            if (this.ShowDialog())
                return true;
            else
                return !this.Hide();
        }
        public override bool Show()
        {
            this.ConformToScreen();
            return base.Show();
        }
        public override bool Hide()
        {
            if (this.WindowManager.RemoveWindow(this))
            {
                this.OnHidden();
                //this.Previous?.Show(this.WindowManager);
                this.Client.OnWindowHidden(this);
                return true;
            }
            return false;
        }

        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            e.Handled = true;
            this.StartDragging();
            base.OnMouseLeftPress(e);
        }

        private void StartDragging()
        {
            this.IsDragged = this.Movable;
            this.MouseOffset = UIManager.MouseTrue - this.ScreenLocation;
        }

        public override Rectangle PreferredClientSize
        {
            get
            {
                int width = 0;
                int height = 0;
                foreach (var control in this.Client.Controls)
                {
                    //width = Math.Max(width, (int)control.Location.X + control.Width - (int)control.Origin.X);
                    //height = Math.Max(height, (int)control.Location.Y + control.Height - (int)control.Origin.Y);
                    width = Math.Max(width, (int)control.TopLeft.X + control.Width - (int)control.Origin.X);
                    height = Math.Max(height, (int)control.TopLeft.Y + control.Height - (int)control.Origin.Y);
                }
                return new Rectangle(0, 0, width, height + 2); //monogame hack
            }
        }
        public void AlignToMouse(Vector2 loc)
        {
            this.Location = UIManager.Mouse - this.ClientLocation - loc;
        }

        internal Window Transparent()
        {
            this.SetOpacity(0);
            this.SetMousethrough(true);
            this.Label_Title.ShowOnParentFocus(true);
            this.Label_Title.MouseThrough = false;
            this.Label_Title.BackgroundColor = Color.Black * .5f;
            this.Label_Title.Active = true;
            this.CloseButton.ShowOnParentFocus(true);
            return this;
        }

        private void StopDragging()
        {
            this.IsDragged = false;
        }

        protected override void OnMouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.StopDragging();
            base.OnMouseLeftUp(e);
        }

        internal override void OnControlResized(Control control)
        {
            base.OnControlResized(control);
            this.EnsureWithinScreenBounds();
        }

        private void EnsureWithinScreenBounds()
        {
            if (this.Location.X + this.Width > UIManager.Width && this.Width <= UIManager.Width)
                this.Location.X = UIManager.Width - this.Width;
            if (this.Location.Y + this.Height > UIManager.Height && this.Height <= UIManager.Height)
                this.Location.Y = UIManager.Height - this.Height;
        }
    }
}
