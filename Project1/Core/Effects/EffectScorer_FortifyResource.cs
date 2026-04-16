using Project1.Core.Entities.Actors;

namespace Project1.Core.Effects;

internal sealed class EffectScorer_FortifyResource : EffectScorer
{
    internal override EffectDef Effect => EffectDefOf.FortifyResource;

    internal override int Score(Actor actor, Def target)
    {
        if (!actor.Effects.TryGet(this.Effect, target, out var ticksRemaining))
            return 50;// 100;
        var minutes = ticksRemaining * Ticks.PerGameMinute;
        if (minutes > 100)
            return 0;
        var score = 1 - minutes / 100f;
        score *= score;
        score *= 50;
        return (int)score;
    }
}
