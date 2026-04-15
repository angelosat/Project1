using Project1.Core.Interactions;

namespace Project1.Core.Towns.Services.Spells;

sealed class InteractionHealingWaitCaster : InteractionLogic
{
    protected override InteractionContext_Healing CreateContextInt() => new();
    static ServiceRequest_Spell Request(InteractionContext ctx) => ((InteractionContext_Healing)ctx).RequestByCaster;
    internal override void OnStart(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var typedCtx = (InteractionContext_Healing)i.Context;
        typedCtx.Manager.MarkCasterReady(i.Actor);
    }
    internal override bool HasSucceeded(Interaction i)
        => Request(i.Context).IsTargetReady;
}
