using Project1.Core.Entities.Actors;
using Project1.Core.Needs;

namespace Project1.Core.AI.Personality;

internal sealed class TraitWorker_Introvert : TraitWorker
{
    internal override void Tick(Actor actor, Trait trait)
    {
        var traitvalue = trait.Value;
        //var normal = (1 + traitvalue) / 2f;
        var normal = traitvalue / 100f;
        actor.Needs.ApplyAccumulatorDelta(NeedDefOf.Social, normal / this.Rate);
    }
}
public class TraitWorker
{
    protected int Rate = Ticks.FromMinutes(10);
    internal virtual void Tick(Actor actor, Trait trait) { }
}
