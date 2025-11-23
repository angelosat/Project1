using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class PanelLabeledScrollable : Panel
    {
        public Label Label;
        public ScrollableBoxTest Client;
        public PanelLabeledScrollable(string label, int width, int height, ScrollModes mode = ScrollModes.Both)
            : base(0, 0, width, height)
        {
            this.Name = label;
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Client = new(width - 2 * this.Padding, height - 2 * this.Padding - Label.DefaultHeight, mode) { Location = this.Label.BottomLeft };
            this.AddControls(this.Label, this.Client);
        }
        public PanelLabeledScrollable(Func<string> label, int width, int height, ScrollModes mode = ScrollModes.Both)
            : base(0, 0, width, height)
        {
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Client = new(width - 2 * this.Padding, height - 2 * this.Padding - Label.DefaultHeight, mode) { Location = this.Label.BottomLeft };
            this.AddControls(this.Label, this.Client);
        }
        internal override void OnControlAdded(Control control)
        {
            base.OnControlAdded(control);
            //this.Label.Validate();
        }
    }
}
