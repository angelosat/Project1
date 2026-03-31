using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Towns.Shops;
using System.Diagnostics;

namespace Project1.Core.Towns.Reputation;

public sealed class ReputationSourceCustomer : ReputationSourceWorker
{
    const int BaseValue = 5;
    public override void HookTo(MapBase map)
    {
        map.Events.ListenTo<ShopTransactionFinishedEvent>(HandleShopTransactionFinished);
    }

    private void HandleShopTransactionFinished(ShopTransactionFinishedEvent e)
    {
        var map = e.Map;
        var transaction = e.Transaction;
        var actor = map.World.Get<Actor>(transaction.Buyer);
        var patienceRemaining = actor.Resources.GetPercentage(ResourceDefOf.Patience);
        if (transaction.IsSucceeded)
            map.Town.Reputation.ApplyDelta(transaction.Buyer, (int)(BaseValue * (1 + patienceRemaining)));
        else if (transaction.IsFailed)
            map.Town.Reputation.ApplyDelta(transaction.Buyer, -BaseValue);
        else
            throw new UnreachableException();
    }
}
