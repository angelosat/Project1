using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Framework.UI;

namespace Project1.Core.UI;

class GearStatsPanel : SelectionBoundControl
{
    GearComp Gear;

    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is not Actor actor)
            return;
        Build(actor);
    }

    private void Build(Actor actor)
    {
        this.Controls.Clear();
        this.Gear = actor.GetComponent<GearComp>();

        ////var statsCache = this.Gear.GetCachedStatsNew();
        //var statsCache = actor.StatTracker.GetAllStats();
        //foreach (var (stat, value) in statsCache)
        //    AddControls(new LabelNew($"{stat.LabelReadable}: {StatSystem.Format(stat, value)}"));
        var statsCache = actor.StatTracker.GetAllStatRuntime();
        foreach (var stat in statsCache)
            //AddControls(new LabelNew($"{stat.Def.LabelReadable}: {StatSystem.Format(stat.Def, stat.Value)} + {stat.Bonus}"));
            AddControls(new LabelNew($"{stat.Def.LabelReadable}: {StatSystem.Format(stat.Def, stat.Resolve(actor))} + {stat.Bonus}"));
        AlignTopToBottom();
    }

    public override void Update()
    {
        base.Update();
        if (this.Gear is null)
            return;
        //if (this.Gear.StatsDirty)
        if ((this.Gear.Owner as Actor).StatTracker.IsStatsDirty)
                Build(Gear.Owner as Actor);
    }
}