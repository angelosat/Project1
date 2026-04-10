using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Stockpiles;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Towns.Shops;
internal sealed class ShopsComp : TownComp
{
    public override string Name => "Shops";
    List<ServiceRequest_Shop> _transactions = [];

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
        
        public ShoppingList ShoppingList
            => actor.Map.Town.ShopManager.GetShoppingListEmpty(actor);
    }

    extension(Entity item)
    {
        public bool IsForSale()
            => item.Map.Town.ZoneManager.GetZoneAt<Stockpile>(item.Cell.Below)?.ForSale ?? false;

        public bool IsInvolvedInExistingTransaction()
            => item.Map.Town.ShopManager.TryGetTransactionByItem(item, out _);
    }
}
