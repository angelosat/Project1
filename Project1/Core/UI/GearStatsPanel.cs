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

        var statsCache = actor.Stats.GetAllStatRuntime();
        foreach (var stat in statsCache)
        {
            var value = stat.Resolve(actor);
            var formatted = StatSystem.Format(stat.Def, value);
            AddControls(new LabelNew($"{stat.Def.LabelReadable}: {formatted} + {stat.Bonus}").InvalidateOn(stat.Notifier));
        }
        AlignTopToBottom();
    }
}