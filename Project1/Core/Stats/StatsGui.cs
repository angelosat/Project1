using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.Stats
{
    class StatsGuiNew : SelectionBoundControl// GroupBox, ISelectionBound
    {
        PanelLabeledNew PanelAttributes;
        PanelLabeledNew PanelStats;


        protected internal override void OnBind(ISelectable selectable)
        {
            this.Build(selectable as Actor);
        }

        protected void Build(Actor actor)
        {
            this.Name = "Stats";

            this.PanelAttributes = new PanelLabeledNew("Attributes") { AutoSize = true };
            this.PanelStats = new PanelLabeledNew("Stats") { AutoSize = true };
            var comp = actor.GetComponent<StatsComponent>();
            this.ClearControls();

            this.PanelAttributes.Client.ClearControls();
            PanelAttributes.Client.AddControls(actor.Attributes.GetGui());
            this.AddControlsTopRight(this.PanelAttributes);

            this.PanelStats.Client.ClearControls();
            comp.GetInterface(actor, this.PanelStats.Client);
            this.AddControlsBottomLeft(this.PanelStats);
        }
    }
}
