using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Stats;
using Project1.Core.Systems.Effects;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Magic;

public sealed class Effect_Flee : EntityEffectWorker
{
    public override EffectDef Def => EffectDefOf.Fled;

    protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
    {
        //actor.World.Events.Post(new ActorStatBonusModifiedEvent((StaticWorld)actor.World,
        //                                                        actor.RefId,
        //                                                        StatDefOf.AvoidCombatEncounter,
        //                                                        100f));
    }
    //protected override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
    //{
    //    actor.World.Events.Post(new ActorStatBonusModifiedEvent((StaticWorld)actor.World,
    //                                                            actor.RefId,
    //                                                            StatDefOf.AvoidCombatEncounter,
    //                                                            -100f));
    //}

    internal override IEnumerable<(StatDef statDef, float value)> GetStatModifiers(Actor actor/*, EntityEffectWrapper runtime*/)
    {
        yield return (StatDefOf.AvoidCombatEncounter, 100f);
    }
}
