using Project1.Core.Entities.Stats;
using Project1.Core.Stats;
using Project1.Core.Systems.Effects;
using Project1.Framework.Serialization;
using System.Collections.Generic;

namespace Project1.Core.Systems.Magic;

public sealed record EffectConfig_Flee : EffectConfig
{
    public override EffectDef Def => EffectDefOf.Fled;

    public override EffectData CreateData()
        => new EffectData_Flee();

    public override string GetString(float scaling)
        => $"Recently fled combat";
}

public sealed class EffectData_Flee : EffectData<EffectConfig_Flee>
{
    public override EffectDef Def => EffectDefOf.Fled;

    public override ISerializable Read(IDataReader r)
        => this;
    public override void Write(IDataWriter w) { }
}

public sealed class Effect_Flee
    : EntityEffectWorker<EffectConfig_Flee, EffectData_Flee>
{

    //protected override void OnStart(Actor actor, EntityEffectRuntime wrapper)
    //{
    //    //actor.World.Events.Post(new ActorStatBonusModifiedEvent((StaticWorld)actor.World,
    //    //                                                        actor.RefId,
    //    //                                                        StatDefOf.AvoidCombatEncounter,
    //    //                                                        100f));
    //}
    //protected override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
    //{
    //    actor.World.Events.Post(new ActorStatBonusModifiedEvent((StaticWorld)actor.World,
    //                                                            actor.RefId,
    //                                                            StatDefOf.AvoidCombatEncounter,
    //                                                            -100f));
    //}

    internal override IEnumerable<(StatDef statDef, float value)> GetStatModifiers(/*Actor actor*//*, EntityEffectWrapper runtime*/)
    {
        yield return (StatDefOf.AvoidCombatEncounter, 100f);
    }
}
