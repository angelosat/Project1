using Project1.Framework.UI;

namespace Project1.Core.UI.Blocks
{
    public class GuiConstructionsBrowser : Window
    {
        readonly BlockBrowserConstruction Browser;

        public GuiConstructionsBrowser()
        {
            this.Title = "Constructions Browser";
            this.AutoSize = true;
            this.Movable = true;
            this.Browser = new BlockBrowserConstruction();
            this.Client.Controls.Add(this.Browser);
        }
        public override bool Hide()
        {
            this.Browser.Hide();
            return base.Hide();
        }
    }
}
