using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Simulation;
using Project1.Core.Stats;
using Project1.Core.Systems.Effects;
using System;

namespace Project1.Core.Systems.Magic;

public sealed class Effect_Flee : EntityEffectWorker
{
    public override EffectDef Def => throw new NotImplementedException();

    protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
    {
        //actor.Stats.ModifyBonus(StatDefOf.AvoidCombatEncounter, 100f);
        actor.World.Events.Post(new ActorStatBonusModifiedEvent((StaticWorld)actor.World,
                                                                actor.RefId,
                                                                StatDefOf.AvoidCombatEncounter,
                                                                100f));
    }
    protected override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
    {
        //actor.Stats.ModifyBonus(StatDefOf.AvoidCombatEncounter, -1f);
        actor.World.Events.Post(new ActorStatBonusModifiedEvent((StaticWorld)actor.World,
                                                                actor.RefId,
                                                                StatDefOf.AvoidCombatEncounter,
                                                                -100f));
    }
}
