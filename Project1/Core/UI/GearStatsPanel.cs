using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using Project1.Framework.UI;
using System;

namespace Project1.Core.UI;

[Obsolete]
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

        var statsCache = actor.Stats.GetAllStatRuntimes();
        foreach (var stat in statsCache)
        {
            //var value = stat.Resolve(actor);
            //var formatted = stat.Format();
            //AddControls(new LabelNew(() => $"{stat.Def.LabelReadable}: {stat.Format()} + {stat.Bonus}").InvalidateOn(stat.Notifier));
            AddControls(new LabelNew(() => $"{stat.Def.LabelReadable}: {stat.ResolveAndFormat(actor)} + {stat.Bonus}").InvalidateOn(stat.Notifier));
        }
        AlignTopToBottom();
    }
}