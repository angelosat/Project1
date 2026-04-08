using Project1.Core.Entities.Actors;
using Project1.Core.Resources;

namespace Project1.Core.Systems.Magic;

public sealed class SpellWorker_Healing : SpellWorker
{
    public override void Cast(Actor caster, InteractionTarget target)
    {
        var targetActor = target.Object as Actor;
        targetActor.Resources.ApplyDelta(ResourceDefOf.Health, 50);
        caster.Resources.ApplyDelta(ResourceDefOf.Mana, -10);
    }
}

public abstract class SpellWorker
{
    public abstract void Cast(Actor Caster, InteractionTarget Target);
}
