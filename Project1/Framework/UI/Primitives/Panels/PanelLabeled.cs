using Microsoft.Xna.Framework;

namespace Project1.Framework.UI
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
