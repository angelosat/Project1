using Project1.Core.Interactions;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables.Scrolls;
using Project1.Core.Systems.Magic;
using Project1.Core.Systems.Trading;

namespace Project1.Core.Towns.Services.Healing;

sealed class InteractionContext_Healing : InteractionContext
{
    internal TownComp_Spells Manager => field ??= this.Actor.Map.Town.Spells;
    internal ServiceRequest_Spell RequestByTarget => field ??= this.Manager.GetRequestbyTargetOrDefault(this.Actor);
    internal ServiceRequest_Spell RequestByCaster => field ??= this.Manager.GetRequestbyCasterOrDefault(this.Actor);
    internal IResourceView Patience => field ??= this.Actor.Resources.View(ResourceDefOf.Patience);
    internal override float GetPercentage(Interaction i) => this.Patience.Percentage;
}
sealed class InteractionCastSpell : InteractionLogic
{
    static SpellDef Spell(InteractionContext ctx) => ctx.Actor.CurrentPlan.Spell;

    internal override void OnStart(Interaction i)
    {
        var spell = Spell(i.Context);
        i.Progress.SetMax((int)Ticks.FromSeconds(spell.DurationSeconds));
    }

    internal override bool HasSucceeded(Interaction i)
        => i.Progress.IsFinished;

    internal override void OnFinish(Interaction i)
    {
        var spell = Spell(i.Context);
        spell.Worker.Cast(i.Actor, i.Target);
        i.Actor.Map.Events.Post(new EntitySpellEvent(i.Target, spell));
    }
}
sealed class InteractionHealingWaitPay : InteractionLogic
{
    protected override InteractionContext_Trade CreateContextInt() => new();
    static TradeRuntime Trade(InteractionContext ctx) => ((InteractionContext_Trade)ctx).Trade;
    internal override bool HasSucceeded(Interaction i)
        => Trade(i.Context).IsOffered;
}
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
sealed class InteractionHealingSeek : InteractionLogic
{
    protected override InteractionContext_Healing CreateContextInt() => new();

    internal override void OnTick(Interaction i)
        => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);
    internal override void OnStart(Interaction i)
    {
        var typedCtx = (InteractionContext_Healing)i.Context;
        typedCtx.Manager.MarkTargetReady(i.Actor);
    }
    internal override bool HasSucceeded(Interaction i)
    {
        var typedCtx = (InteractionContext_Healing)i.Context;
        var threshold = .5f;
        if (i.Actor.Resources.GetPercentage(ResourceDefOf.Health) < threshold)
            return false;
        typedCtx.Manager.MarkSucceeded(i.Actor);

        return true;
    }
    internal override bool HasFailed(Interaction i)
    {
        if (i.Actor.Resources.GetPercentage(ResourceDefOf.Patience) <= 0)
            return true;
        return false;
    }
}
