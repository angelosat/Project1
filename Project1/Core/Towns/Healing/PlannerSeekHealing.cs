using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using Project1.Core.Resources;
using Project1.Core.Systems.Magic;

namespace Project1.Core.Towns.Healing;

sealed class InteractionContext_Healing : InteractionContext
{
    internal TownComp_Spells Manager => field ??= this.Actor.Map.Town.SpellManager;
    internal SpellRequest RequestByTarget => field ??= this.Manager.GetRequestbyTargetOrDefault(this.Actor);
    internal SpellRequest RequestByCaster => field ??= this.Manager.GetRequestbyCasterOrDefault(this.Actor);
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
        Spell(i.Context).Worker.Cast(i.Actor, i.Target);
    }
}

sealed class InteractionHealingWaitCaster : InteractionLogic
{
    protected override InteractionContext_Healing CreateContextInt() => new();
    static SpellRequest Request(InteractionContext ctx) => ((InteractionContext_Healing)ctx).RequestByCaster;
    internal override bool HasSucceeded(Interaction i)
        => Request(i.Context).IsReady;
}
//sealed class InteractionHealingWaitTarget : InteractionLogic
//{
//    protected override InteractionContext_Healing CreateContextInt() => new();
//    internal override bool HasSucceeded(Interaction i)
//    {
//        var typedCtx = (InteractionContext_Healing)i.Context;
//        var threshold = .5f;
//        if (i.Actor.Resources.GetPercentage(ResourceDefOf.Health) < threshold)
//            return false;
//        typedCtx.Manager.MarkSucceeded(i.Actor);
//        return true;
//    }
//}
sealed class InteractionHealingSeek : InteractionLogic
{
    protected override InteractionContext_Healing CreateContextInt() => new();
    internal override void OnStart(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var typedCtx = (InteractionContext_Healing)i.Context;
        typedCtx.RequestByTarget.MarkReady();
    }
    internal override void OnTick(Interaction i)
        => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);
    internal override bool HasSucceeded(Interaction i)
    {
        var typedCtx = (InteractionContext_Healing)i.Context;
        var threshold = .5f;
        //if (!typedCtx.RequestByCaster.IsReady)
        //    return false;
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
internal class PlannerOfferHealing : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsHauling)
            return null;
        var map = actor.Map;
        var manager = map.Town.SpellManager;
        if(manager.TryGetRequestByCaster(actor, out var existing))
        {
            if (existing.IsDisposed)
                return null;
            var target = map.World.Get<Actor>(existing.TargetId);
            if (!actor.CanReach(target))
                return null;
            if (existing.IsReady)
                return new Plan(SpellDefOf.PlanCastSpell, target) { Spell = SpellDefOf.Healing };
            return new Plan(HealingDefOf.PlanHealingWaitCaster);
        }
        var allRequests = manager.PendingRequests;
        foreach(var req in allRequests)
        {
            var target = map.World.Get<Actor>(req.TargetId);
            if (!actor.CanReach(target))
                continue;
            manager.MarkAccepted(req, actor);
            return new Plan(HealingDefOf.PlanHealingWaitCaster);
            //return null;
            //return plan to wait for the target to come over
        }
        return null;
    }
}
internal class PlannerSeekHealing : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsHauling)
            return null;
        var healthPerc = actor.Resources.GetPercentage(ResourceDefOf.Health);
        var threshold = .5f;
        if (healthPerc > threshold)
            return null;
        var map = actor.Map;
        var manager = map.Town.SpellManager;
        if(manager.TryGetRequestByTarget(actor, out var existing))
        {
            if(!existing.IsAccepted)
                return null;
            return new Plan(HealingDefOf.PlanHealingSeek, map.World.Get<Actor>(existing.CasterId));
        }
        manager.Request(actor, SpellDefOf.Healing);
        return null;
    }
}
