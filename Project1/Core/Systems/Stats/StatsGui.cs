using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.UI;
using Project1.Framework.Helpers;
using Project1.Framework.UI;

namespace Project1.Core.Stats;

sealed class StatsGuiNew : SelectionBoundControl
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
        this.ClearControls();

        this.PanelAttributes.Client.ClearControls();
        PanelAttributes.Client.AddControls(actor.Attributes.CreateControl());
        this.AddControlsTopRight(this.PanelAttributes);

        this.PanelStats.Client.ClearControls();
        var comp = actor.GetComponent<StatsComp>();
        this.PanelStats.Client.AddControls(comp.CreateControl());

        var gui = new Table<StatDef>()
            .AddColumn("name", 120, a => new LabelNew(a.LabelReadable), 1)
            .AddColumn("divider", 8, s => new LabelNew(string.Empty))
            .AddColumn("value", 64, a => new LabelNew(() => $"{actor.Stats.ResolveAndFormat(a)}")
            { HoverFunc = () => actor.Stats.Breakdown(a)}.InvalidateOn(actor.Stats.GetNotifier(a)));
        var statsCache = actor.Stats.GetAllStatDefs();
        gui.AddItems(statsCache);

        var gearStatsPanel = new ScrollableBoxNewNewNew(gui, gui.Width, 150, mode: ScrollModes.Vertical)
            .ToPanelLabeled("Stats");

        this.AddControlsBottomLeft(gearStatsPanel);
    }
}
