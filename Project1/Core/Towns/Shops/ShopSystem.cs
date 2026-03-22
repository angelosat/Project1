using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Stockpiles;
using Project1.Framework;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Towns.Shops;

sealed class ShopTransaction(Actor buyer, Entity item, IntVec3 counter)
{
    bool _cancelled;
    public readonly EntityRefId Buyer = buyer.RefId;
    public EntityRefId Seller = EntityRefId.Null;
    public readonly EntityRefId Item = item.RefId;
    public readonly IntVec3 Counter = counter;
    double TicksRemaining = Ticks.FromHours(1);
    internal bool IsCancelled => this._cancelled;
    public bool TimedOut => this.TicksRemaining <= 0;

    internal void Cancel()
        => this._cancelled = true;
    internal void Tick()
    {
        if (this.TicksRemaining <= 0)
            return;
        this.TicksRemaining--;
    }
    internal void RefreshTimer()
        => this.TicksRemaining = Ticks.FromHours(1);
}
internal sealed class ShopsComp : TownComponent
{
    public override string Name => "Shops";
    List<ShopTransaction> _transactions = [];

    internal bool TryBeginTransaction(Actor actor, Entity item)
    {
        return false;
    }
}
internal static class ShopSystem
{
    extension(Actor actor)
    {
        public int GetMoneyAmount()
            => actor.Inventory.Count(ItemDefOf.Coins);
        public bool CanAfford(Entity item)
        {
            var itemValue = item.GetValueTotal();
            var actorMoney = actor.GetMoneyAmount();
            return actorMoney >= itemValue;
        }
        
    }

    extension(Entity item)
    {
        public bool IsForSale()
            => item.Map.Town.ZoneManager.GetZoneAt<Stockpile>(item.Cell.Below)?.ForSale ?? false;
    }
}
