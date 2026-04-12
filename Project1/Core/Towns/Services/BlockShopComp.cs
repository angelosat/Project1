using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Screens;
using Project1.Framework.Events;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns.Services;

sealed class TownServiceAssignmentGui : GroupBox
{
    BlockShopComp Comp;
    readonly Table<TownServiceDef> Table;
    readonly Lazy<IEnumerable<TownServiceDef>> allDefs = new(Def.Get<TownServiceDef>);
    public TownServiceAssignmentGui()
    {
        var defs = Def.Get<TownServiceDef>();
        this.Table = new Table<TownServiceDef>()
            .AddColumn("label", 128, def => new Label(def))
            .AddColumn("tick", CheckBoxFinalNew.DefaultBounds.Width, def => new CheckBoxFinalNew(() => toggle(def), () => this.Comp.Service == def).InvalidateOn(this.Comp.Notifier));
        this.Controls.Add(this.Table);
    }

    internal void Bind(BlockShopComp comp)
    {
        this.Comp = comp;
        this.Table.ClearControls();
        this.Table.AddItems(allDefs.Value);
    }

    private void toggle(TownServiceDef def)
    {
        Ingame.Instance.Events.Post(new PlayerAssignedServiceToCounterEvent(this.Comp, def));
    }
}

internal class BlockShopComp : BlockComp
{
    public new class Spec : BlockComp.Spec
    {
        public override Type CompType => typeof(BlockShopComp);

        public override BlockShopComp CreateComp() => new();// { Service = this.Service };

    }
    public override BlockCompDef CompDef => BlockCompDefOf.Shop;

    public TownServiceDef Service
    {
        get => field;
        set
        {
            field = value;
            this.Notifier.Notify();
        }
    }

    public int CashFloat = 100;
    public readonly ChangeNotifier Notifier = new();

    public BlockResourcesComp _resourcesComp => field ??= this.Parent.GetComp<BlockResourcesComp>();
    
    internal override void ResolveReferences()
    {
        this._resourcesComp.SetValue(ResourceDefOf.Cash, 0);
        this._resourcesComp.SetMax(ResourceDefOf.Cash, 500);
        this._resourcesComp.SetOverflowMax(ResourceDefOf.Cash, ItemDefOf.Coins.StackCapacity - CashFloat);
    }
   
    internal override bool TryConsume(Entity item)
    {
        if (item.Def != ItemDefOf.Coins)
            return false;
        if (this._resourcesComp is null)
            return false;
        if (!this._resourcesComp.TryApplyDelta(ResourceDefOf.Cash, item.StackSize))
            return false;
        item.Consume(item.StackSize);
        return true;
    }

    internal override IEnumerable<Control> GetInspectorControls()
    {
        yield return new LabelNew(() => $"Service: {this.Service?.LabelReadable ?? "<unassigned>"}").InvalidateOn(this.Notifier);
    }
}
