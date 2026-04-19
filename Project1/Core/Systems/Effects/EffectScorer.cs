using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.Effects;
internal abstract class EffectScorer
{
    internal abstract EffectDef Effect { get; }
    internal abstract int Score(Actor actor, Def target);
}
