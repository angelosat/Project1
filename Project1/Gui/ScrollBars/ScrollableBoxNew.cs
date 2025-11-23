using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.UI
{
    [Obsolete]
    public class ScrollableBoxNew : Control
    {
        readonly ScrollbarV VScroll;
        readonly ScrollbarH HScroll;
        public Control Client;

        public override Control SetOpacity(float value, bool children, params Control[] exclude)
        {
            base.SetOpacity(value, children, exclude);
            this.VScroll.SetOpacity(value, true);
            this.HScroll.SetOpacity(value, true);
            return this;
        }
        public override void OnPaint(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.OnPaint(sb);
        }

        public override bool AutoSize
        {
            get => base.AutoSize;
            set
            {
                base.AutoSize = value;
                this.Client.AutoSize = value;
            }
        }
        public ScrollableBoxNew(int width, int height, ScrollModes mode = ScrollModes.Both)
            : this(new Rectangle(0, 0, width, height), mode)
        {

        }
        public ScrollableBoxNew(Rectangle viewportBounds, ScrollModes mode = ScrollModes.Both)
            : base(new Vector2(viewportBounds.X, viewportBounds.Y), new Vector2(viewportBounds.Width, viewportBounds.Height))
        {
            var modeFactor = new IntVec2((mode & ScrollModes.Vertical) == ScrollModes.Vertical ? 1 : 0, (mode & ScrollModes.Horizontal) == ScrollModes.Horizontal ? 1 : 0);
            var buttonSize = 16;
            this.Client = new GroupBox() { Size = new Rectangle(0, 0, viewportBounds.Width - buttonSize * modeFactor.X, viewportBounds.Height - buttonSize * modeFactor.Y) };
            this.Client.AutoSize = false;
            this.VScroll = new ScrollbarV(new Vector2(this.Client.Width, 0), this.Client.Height, this.Client);
            this.HScroll = new ScrollbarH(new Vector2(0, this.Client.Height), this.Client.Width, this.Client);
            this.Size = new Rectangle(viewportBounds.X, viewportBounds.Y, viewportBounds.Width, viewportBounds.Height);
            this.Controls.Add(this.Client);
        }

        public virtual void Add(params Control[] controls)
        {
            foreach (var ctrl in controls)
                this.Client.Controls.Add(ctrl);
            this.Remeasure();
        }
        public void Remove(Control control)
        {
            this.Client.Controls.Remove(control);
            this.Remeasure();
        }
        public override Control AddControls(params Control[] controls)
        {
            foreach (var ctrl in controls)
                this.Client.Controls.Add(ctrl);
            this.Remeasure();
            return this;
        }
        public override void RemoveControls(params Control[] controls)
        {
            foreach (var ctrl in controls)
                this.Client.Controls.Remove(ctrl);
            this.Remeasure();
        }
        public override void ClearControls()
        {
            this.Client.ClearControls();
        }
        public virtual void Remeasure()
        {
            var clientDesiredSize = this.Client.PreferredClientSize;
            var prefw = clientDesiredSize.Width;
            if (this.Width < prefw)
            {
                this.Controls.Add(this.HScroll);
                this.Client.Height = this.Height - 16;
            }
            else
            {
                this.Controls.Remove(this.HScroll);
                this.Client.Height = this.Height;
            }

            var prefh = clientDesiredSize.Height;
            if (this.Height < prefh)
            {
                this.Controls.Remove(this.VScroll);
                this.Controls.Add(this.VScroll);
                this.Client.Width = this.Width - 16;
            }
            else
            {
                this.Controls.Remove(this.VScroll);
                this.Client.Width = this.Width;
            }
            this.HScroll.Width = this.Client.Width;
            this.VScroll.Height = this.Client.Height;
        }
        protected virtual void UpdateScrollbars()
        {
            var prefsize = this.Client.PreferredClientSize;
            var prefw = prefsize.Width;
            if (this.Client.Width < prefw)
            {
                if (!this.Controls.Contains(this.HScroll))
                    this.Controls.Add(this.HScroll);
            }
            else
            {
                this.Controls.Remove(this.HScroll);
            }

            var prefh = prefsize.Height;
            if (this.Client.Height < prefh)
            {
                if (!this.Controls.Contains(this.VScroll))
                    this.Controls.Add(this.VScroll);
            }
            else
            {
                this.Controls.Remove(this.VScroll);
            }
            this.HScroll.Width = this.Client.Width;
            this.VScroll.Height = this.Client.Height;
        }

        public override void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.HitTest())
                return;

            int step = UIManager.LineHeight;
            this.Client.ClientLocation.Y = Math.Min(0, Math.Max(this.Client.Height - this.Client.ClientSize.Height, this.Client.ClientLocation.Y + step * e.Delta));
            e.Handled = true;
        }

        public override void AlignTopToBottom(int spacing = 0)
        {
            this.Client.Controls.AlignVertically(spacing);
            this.Client.ClientSize = this.Client.PreferredClientSize;
            this.Client.ConformToClientSize();
            this.Remeasure();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        public void UpdateClientSize()
        {
            this.Client.ClientSize = this.Client.PreferredClientSize;
            this.EnsureClientWithinBounds(); // should i do this here?
            this.UpdateScrollbars(); // should i do this here?
        }

        protected void EnsureLocationVisible(float lowestVisiblePoint)
        {
            this.Client.ClientLocation.Y = Math.Min(this.Client.ClientLocation.Y, -lowestVisiblePoint + this.Client.Size.Height);
        }
        protected void EnsureLocationVisible(int lowestVisiblePoint)
        {
            this.Client.ClientLocation.Y = Math.Min(this.Client.ClientLocation.Y, -lowestVisiblePoint + this.Client.Size.Height);
        }
        protected void EnsureClientWithinBounds()
        {
            this.Client.ClientLocation.Y = Math.Max(this.Client.ClientLocation.Y, Math.Min(0, this.Client.Size.Height - this.Client.ClientSize.Height));
        }
    }
}
