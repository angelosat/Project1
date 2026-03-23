using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Stockpiles;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Towns.Shops;
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
