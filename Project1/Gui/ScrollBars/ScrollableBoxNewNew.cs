using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.UI
{
    [Obsolete]
    public class ScrollableBoxNewNew : GroupBox
    {
        private const int buttonSize = 16;
        readonly ScrollbarV VScroll;
        readonly ScrollbarH HScroll;
        public GroupBox Client;
        public override Rectangle ClientSize => this.Client.ClientSize;
        ScrollModes Mode;
        public override Control SetOpacity(float value, bool children, params Control[] exclude)
        {
            base.SetOpacity(value, children, exclude);
            this.VScroll.SetOpacity(value, true);
            this.HScroll.SetOpacity(value, true);
            return this;
        }
        
        public static ScrollableBoxNewNew FromClientSize(int width, int height, ScrollModes mode = ScrollModes.Both)
        {
            var modeFactor = new IntVec2((mode & ScrollModes.Vertical) == ScrollModes.Vertical ? 1 : 0, (mode & ScrollModes.Horizontal) == ScrollModes.Horizontal ? 1 : 0);
            return new ScrollableBoxNewNew(width + buttonSize * modeFactor.X, height + buttonSize * modeFactor.Y, mode);
        }

        public ScrollableBoxNewNew(int width, int height, ScrollModes mode = ScrollModes.Both)
            : base(width, height)
        {
            this.Mode = mode;
            var modeFactor = new IntVec2((mode & ScrollModes.Vertical) == ScrollModes.Vertical ? 1 : 0, (mode & ScrollModes.Horizontal) == ScrollModes.Horizontal ? 1 : 0);
            this.Client = new GroupBox(width - buttonSize * modeFactor.X, height - buttonSize * modeFactor.Y) { AutoSize = false };
            this.VScroll = new ScrollbarV(new Vector2(this.Client.Width, 0), this.Client.Height, this.Client);
            this.HScroll = new ScrollbarH(new Vector2(0, this.Client.Height), this.Client.Width, this.Client);
            this.Controls.Add(this.Client);
        }

        internal override void OnControlResized(Control control)
        {
            base.OnControlResized(control);
            if (control == this.Client)
                this.UpdateClientSize();
        }
        public override Control AddControls(params Control[] controls)
        {
            foreach (var ctrl in controls)
                this.Client.Controls.Add(ctrl);
            this.UpdateClientSize();
            return this;
        }
        public override void RemoveControls(params Control[] controls)
        {
            foreach (var ctrl in controls)
                this.Client.Controls.Remove(ctrl);
            this.UpdateClientSize();
        }
        public override void ClearControls()
        {
            this.Client.ClearControls();
        }

        protected virtual void UpdateScrollbars()
        {
            var prefsize = this.Client.PreferredClientSize;
            var prefw = prefsize.Width;
            if ((this.Mode & ScrollModes.Horizontal) == ScrollModes.Horizontal && this.Client.Width < prefw)
            {
                if (!this.Controls.Contains(this.HScroll))
                    this.Controls.Add(this.HScroll);
            }
            else
                this.Controls.Remove(this.HScroll);

            var prefh = prefsize.Height;
            if ((this.Mode & ScrollModes.Vertical) == ScrollModes.Vertical && this.Client.Height < prefh)
            {
                if (!this.Controls.Contains(this.VScroll))
                    this.Controls.Add(this.VScroll);
            }
            else
                this.Controls.Remove(this.VScroll);
            this.HScroll.Width = this.Client.Width;
            this.VScroll.Height = this.Client.Height;
        }
        public override void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.HitTest()) // why hittest again? i hittest during update. just check if has focus
                return;
            e.Handled = true;
            if (this.Client.ClientSize.Height <= this.Client.Height)
                /// if nothing to scroll, dont move the client container. added this after chat lines where moved from the bottom to the top when turning the mousewheel once,
                /// even when their height was smaller than the chat window
                return;
            int step = UIManager.LineHeight;
            this.Client.ClientLocation.Y = Math.Min(0, Math.Max(this.Client.Height - this.Client.ClientSize.Height, this.Client.ClientLocation.Y + step * e.Delta));
            //this.Client.ClientLocation.Y = Math.Max(0, Math.Min(this.Client.Height - this.Client.ClientSize.Height, this.Client.ClientLocation.Y + step * e.Delta));
        }

        public override void AlignTopToBottom(int spacing = 0)
        {
            this.Client.Controls.AlignVertically(spacing);
            this.Client.ClientSize = this.Client.PreferredClientSize;
            this.Client.ConformToClientSize();
            this.UpdateClientSize();
        }

        protected void UpdateClientSize()
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
