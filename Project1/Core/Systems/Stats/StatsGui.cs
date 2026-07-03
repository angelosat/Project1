using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.UI;
using Project1.Framework.UI;
using System.Linq;

namespace Project1.Core.Stats;

class StatsGuiNew : SelectionBoundControl
{
    PanelLabeledNew PanelAttributes;
    PanelLabeledNew PanelStats;
    PanelLabeledNew PanelStatsNew;

    protected internal override void OnBind(ISelectable selectable)
    {
        this.Build(selectable as Actor);
    }

    protected void Build(Actor actor)
    {
        this.Name = "Stats";

        this.PanelAttributes = new PanelLabeledNew("Attributes") { AutoSize = true };
        this.PanelStats = new PanelLabeledNew("Stats") { AutoSize = true };
        this.ClearControls();

        this.PanelAttributes.Client.ClearControls();
        //PanelAttributes.Client.AddControls(actor.Attributes.GetGui());
        PanelAttributes.Client.AddControls(actor.Attributes.CreateControl());
        this.AddControlsTopRight(this.PanelAttributes);

        this.PanelStats.Client.ClearControls();
        var comp = actor.GetComponent<StatsComp>();
        //comp.GetInterface(actor, this.PanelStats.Client);
        this.PanelStats.Client.AddControls(comp.CreateControl());
        this.AddControlsBottomLeft(this.PanelStats);

        this.PanelStatsNew = new PanelLabeledNew("StatsNew") { AutoSize = true };
        var gui = new Table<StatDef>()
           .AddColumn("name", 96, a => new Label(a.LabelReadable), 1)
           .AddColumn("divider", 8, s => new Label(""))
           .AddColumn("value", 96, a => new Label(() => a.CalculateFor(actor).ToString()));
        gui.AddItems(actor.Components.Values.SelectMany(StatSystem.GetStatsFor));
        this.PanelStatsNew.Client.AddControls(gui);
        this.AddControlsBottomLeft(this.PanelStatsNew);

        //var listnew = new ListBoxNoScroll();
        //foreach(var c in actor.Components.Values)
        //{

        //}
    }
}
