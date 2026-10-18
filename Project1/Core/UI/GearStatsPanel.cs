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

        var statsCache = this.Gear.GetCachedStatsNew();
        foreach (var (stat, value) in statsCache)
            AddControls(new LabelNew($"{stat.LabelReadable}: {StatSystem.Format(stat, value)}"));
        AlignTopToBottom();
    }

    public override void Update()
    {
        base.Update();
        if (this.Gear is null)
            return;
        if (this.Gear.StatsDirty)
            Build(Gear.Owner as Actor);
    }
}