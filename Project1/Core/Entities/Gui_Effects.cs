using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Effects;
using Project1.Core.UI;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.Entities;

sealed class Gui_Effects : SelectionBoundControl
{
    readonly Table<EntityEffectRuntime> Table;

    EffectsComp Comp;

    public Gui_Effects()
    {
        Table = new Table<EntityEffectRuntime>()
            //.AddColumn("name", 128, e => new LabelNew(e.Def.LabelReadable))
            .AddColumn("name", 128, e => new LabelNew(e.Def.Worker.GetString(e.Target, 0)))
            .AddColumn("remaining", 128, e => new Label(() =>
            {
                var span = e.RemainingTimespan(Comp.Owner.World.CurrentTick);
                if (!span.HasValue)
                    return string.Empty;
                //return $"{span.Hours} m {span.Minutes} s";
                return span.Value.Hours > 0 ? $"{span.Value.Hours} h" : $"{span.Value.Minutes} s";
            }));
        var scrollbox = ScrollableBoxNewNewNew.FromWidth(Table, Table.RowWidth, UIManager.DefaultLabelHeight * 16);
        Controls.Add(scrollbox.ToPanelLabeled("Active Effects"));
    }

    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is not Actor actor)
            return;

        Comp?.Changed -= Comp_Changed;
        Comp = actor.Effects;
        Comp.Changed += Comp_Changed;
        Table.ClearControls();
        Table.AddItems(actor.Effects.Active);
    }

    private void Comp_Changed((IEnumerable<EntityEffectRuntime> added, IEnumerable<EntityEffectRuntime> removed) e)
    {
        Table.RemoveItems(e.removed);
        Table.AddItems(e.added);
    }
}
