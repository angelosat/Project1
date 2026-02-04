using System;
using Microsoft.Xna.Framework;
using Project1.Framework.UI;

namespace Start_a_Town_.UI
{
    class PanelLabeled : Panel
    {
        public Label Label { get; set; }
        public PanelLabeled(string label)
        {
            this.AutoSize = true;
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Controls.Add(this.Label);
        }
    }
}
