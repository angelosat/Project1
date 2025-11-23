using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class PanelTitled : GroupBox
    {
        public Label Label;
        public Panel Client;

        public PanelTitled(string label)
        {
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Client = new Panel() { Location = this.Label.BottomLeft, AutoSize = true };
            this.Controls.Add(this.Label, this.Client);
        }
        public PanelTitled(string label, int width, int height)
        {
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Client = new Panel(new Rectangle(0, 0, width, height)) { Location = this.Label.BottomLeft };
            this.Controls.Add(this.Label, this.Client);
        }
        public override Control AddControls(params Control[] controls)
        {
            this.Client.AddControls(controls);
            return this;
        }
        public override void RemoveControls(params Control[] controls)
        {
            foreach (var c in controls)
                this.Client.Controls.Remove(c);
        }
        public override void ClearControls()
        {
            this.Client.ClearControls();
        }
        static public int GetClientLength(int totallength)
        {
            return totallength - BackgroundStyle.Window.Border - BackgroundStyle.Window.Border;
        }
    }
}
