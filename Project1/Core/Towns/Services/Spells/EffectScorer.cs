using Project1.Core.Effects;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using System;

namespace Project1.Core.Towns.Services.Spells;

internal sealed class EffectScorer_RestoreResource : EffectScorer
{
    internal override EffectDef Effect => EffectDefOf.RestoreResource;

    internal override int Score(Actor actor, Def target)
    {
        var resource = (ResourceDef)target;
        var deficit = (int)actor.Resources.GetDeficit(resource);
        return deficit;
    }
}
internal sealed class EffectScorer_FortifyResource : EffectScorer
{
    internal override EffectDef Effect => EffectDefOf.FortifyResource;

    internal override int Score(Actor actor, Def target)
    {
        if(!actor.Effects.TryGet(this.Effect, target, out var ticksRemaining))
            return 100;
        var minutes = ticksRemaining * Ticks.PerGameMinute;
        if (minutes > 100)
            return 0;
        var score = 1 - minutes / 100f;
        score *= score;
        score *= 100;
        return (int)score;
    }
}
internal abstract class EffectScorer
{
    internal abstract EffectDef Effect { get; }
    internal abstract int Score(Actor actor, Def target);
}
