using Project1.Core.Interactions;
using Project1.Core.Systems.Trading;

namespace Project1.Core.Towns.Services.Healing;

sealed class InteractionHealingWaitPay : InteractionLogic
{
    protected override InteractionContext_Trade CreateContextInt() => new();
    static TradeRuntime Trade(InteractionContext ctx) => ((InteractionContext_Trade)ctx).Trade;
    internal override bool HasSucceeded(Interaction i)
        => Trade(i.Context).IsOffered;
}
