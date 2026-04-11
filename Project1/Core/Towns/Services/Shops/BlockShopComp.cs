using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Resources;
using System;

namespace Project1.Core.Towns.Services.Shops;

internal class BlockShopComp : BlockComp
{
    public new class Spec : BlockComp.Spec
    {
        public override Type CompType => typeof(BlockShopComp);

        public override BlockShopComp CreateComp() => new();// { Service = this.Service };

        //internal TownServiceDef Service;
        //public Spec(TownServiceDef service)
        //{
        //    this.Service = service;
        //}
    }
    public override BlockCompDef CompDef => BlockCompDefOf.Shop;

    //public TownServiceDef Service;

    public int CashFloat = 100;

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
}
