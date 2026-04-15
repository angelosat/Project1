using Project1.Core.Effects;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;

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
        return 100;
    }
}
internal abstract class EffectScorer
{
    internal abstract EffectDef Effect { get; }
    internal abstract int Score(Actor actor, Def target);
}
