using Project1.Core.Interactions;
using Project1.Core.Resources;

namespace Project1.Core.Towns.Services.Spells;

sealed class Interaction_Spell_Customer : InteractionLogic
{
    protected override InteractionContext_Healing CreateContextInt() => new();

    internal override void OnTick(Interaction i)
        => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);
    internal override void OnStart(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var typedCtx = (InteractionContext_Healing)i.Context;
        typedCtx.Manager.MarkTargetReady(i.Actor);
    }
    //internal override bool HasSucceeded(Interaction i)
    //{
    //    var typedCtx = (InteractionContext_Healing)i.Context;
    //    var threshold = .5f;
    //    if (i.Actor.Resources.GetPercentage(ResourceDefOf.Health) < threshold)
    //        return false;
    //    typedCtx.Manager.MarkSucceeded(i.Actor);

    //    return true;
    //}
    internal override bool HasFailed(Interaction i)
    {
        if (i.Actor.Resources.GetPercentage(ResourceDefOf.Patience) <= 0)
            return true;
        return false;
    }
}
