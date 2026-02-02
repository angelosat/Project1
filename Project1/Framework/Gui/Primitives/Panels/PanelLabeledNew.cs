using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class PanelLabeledNew : Panel
    {
        public Label Label;
        public GroupBox Client;
        public PanelLabeledNew(string label)
        {
            this.Name = label;
            this.AutoSize = true;
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Client = new GroupBox() { AutoSize = true, Location = this.Label.BottomLeft };
            this.AddControls(this.Label, this.Client);
        }
        public PanelLabeledNew(Func<string> label)
        {
            this.AutoSize = true;
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Client = new GroupBox() { AutoSize = true, Location = this.Label.BottomLeft };
            this.AddControls(this.Label, this.Client);
        }
    }
}
