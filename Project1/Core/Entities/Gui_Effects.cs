using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Effects;
using Project1.Core.UI;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.Entities;

sealed class Gui_Effects : SelectionBoundControl
{
    readonly Table<EntityEffectWrapper> Table;

    EffectsComp Comp;

    public Gui_Effects()
    {
        Table = new Table<EntityEffectWrapper>()
            .AddColumn("name", 128, e => new LabelNew(e.Def.LabelReadable))
            .AddColumn("remaining", 128, e => new Label(() => e.RemainingTimespan(Comp.Owner.World.CurrentTick).ToString()));
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

    private void Comp_Changed((IEnumerable<EntityEffectWrapper> added, IEnumerable<EntityEffectWrapper> removed) e)
    {
        Table.RemoveItems(e.removed);
        Table.AddItems(e.added);
    }
}
