using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Interactions;
using Project1.Core.Resources;

namespace Project1.Core.Towns;

sealed class InteractionWithdrawCashOverflow : InteractionLogic
{
    internal override void OnFinish(Interaction i)
    {
        var actor = i.Actor;
        if (actor.Net.IsClient)
            return;
        var map = actor.Map;
        var be = map.GetBlockEntity(i.Target.Global);
        var comp = be.GetComp<BlockResourcesComp>();
        var overflow = (int)comp.GetOverflow(ResourceDefOf.Cash);
        var coins = ItemDefOf.Coins.Create(null, overflow);
        map.World.Register(coins);
        actor.Inventory.HaulNew(coins, coins.StackSize);
        comp.SetToMax(ResourceDefOf.Cash);
    }
}
