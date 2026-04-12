using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Towns.Services.Shops;

namespace Project1.Core.Towns.Reputation;

public sealed class ReputationSourceCustomer : ReputationSourceWorker
{
    const int BaseValue = 1;
    public override void HookTo(MapBase map)
    {
        map.Events.ListenTo<TownServiceCompleteEvent>(HandleShopTransactionFinished);
    }

    private void HandleShopTransactionFinished(TownServiceCompleteEvent e)
    {
        var map = e.Map;
        var transaction = e.Transaction;
        var actor = map.World.Get<Actor>(transaction.Customer);
        var patienceCurrent = (int)actor.Resources.GetValue(ResourceDefOf.Patience);
        var patienceMax = (int)actor.Resources.GetMax(ResourceDefOf.Patience);
        var patienceConsumed = transaction.PatienceInitial - patienceCurrent;
        var patienceConsumedNormalized = (float)patienceConsumed / patienceMax;
        if (transaction.IsSucceeded)
            map.Town.Reputation.ApplyDelta(actor, 1 + (int)(BaseValue * (1 - patienceConsumedNormalized)));
        else if (transaction.IsFailed)
            map.Town.Reputation.ApplyDelta(actor, -BaseValue);
    }
}
