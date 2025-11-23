using SharpDX.Direct3D9;
using System;
using System.Reflection.Metadata;

namespace Start_a_Town_.UI
{
    public class PanelScrollable : Panel
    {
        public ScrollableBoxTest Client;
        public PanelScrollable(int width, int height, ScrollModes mode = ScrollModes.Both)
            : base(0, 0, width, height)
        {
            this.Client = new(width - 2 * this.Padding, Math.Min(UIManager.Height, height - 2 * this.Padding - Label.DefaultHeight), mode);
            this.AddControls(this.Client);
        }
        public PanelScrollable(Control content, ScrollModes mode = ScrollModes.Both)
            : base()
        {
            this.AutoSize = true;
            this.Client = ScrollableBoxTest.FromContentsSize(content.Width, Math.Min(UIManager.Height - 2 * this.Padding, content.Height), mode);
            this.Client.AddControls(content);
            this.AddControls(this.Client);
        }
        
        public override void Refresh()
        {
            base.Refresh();
            if (this.Client is null)
                return;
            var newHeight = Math.Min(UIManager.Height, this.Client.Client.Height + 2 * this.Padding);
            if (newHeight != this.Height)
            {
                this.Height = newHeight;
                this.Client.Height = newHeight - 2 * this.Padding;
                this.Client.Client.Location.Y = 0;
                //this.Invalidate();
            }
        }
    }
}
