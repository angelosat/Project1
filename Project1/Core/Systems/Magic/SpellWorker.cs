using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.Magic;
public sealed class SpellWorker_RestoreHealth() : SpellWorker_RestoreResource(ResourceDefOf.Health);
public abstract class SpellWorker_RestoreResource(ResourceDef resource) : SpellWorker
{
    readonly ResourceDef Resource = resource;
    public override void Cast(Actor caster, InteractionTarget target)
    {
        var targetActor = target.Object as Actor;
        var placeholderMagnitude = 50;
        var placeholderManaCost = 10;
        targetActor.Resources.ApplyDelta(this.Resource, placeholderMagnitude);
        caster.Resources.ApplyDelta(this.Resource, -placeholderManaCost);
    }
}
public sealed class SpellWorker_FortifyHealth() : SpellWorker_FortifyResource(ResourceDefOf.Health);
public abstract class SpellWorker_FortifyResource(ResourceDef resource) : SpellWorker
{
    readonly ResourceDef Resource = resource;
    public override void Cast(Actor caster, InteractionTarget target)
    {
        var targetActor = target.Object as Actor;
        var effect = new EntityEffectWrapper(EffectDefOf.FortifyResource, ResourceDefOf.Health, Ticks.FromHours(1), 1);
        targetActor.Effects.Apply(effect);
    }
}


public sealed class SpellWorker_Null : SpellWorker
{
    public override void Cast(Actor caster, InteractionTarget target)
    {
        
    }
}

public abstract class SpellWorker
{
    public abstract void Cast(Actor Caster, InteractionTarget Target);


}
