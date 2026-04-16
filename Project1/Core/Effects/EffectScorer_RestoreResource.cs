using Project1.Core.Entities.Actors;
using Project1.Core.Resources;

namespace Project1.Core.Effects;

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
