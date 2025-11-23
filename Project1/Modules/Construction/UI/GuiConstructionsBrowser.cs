using Start_a_Town_.UI;
using Start_a_Town_.Core;

namespace Start_a_Town_
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
