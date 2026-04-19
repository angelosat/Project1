using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.Magic;

public sealed class Effect_FortifyResource : EntityEffectWorker
{
    public override EffectDef Def => EffectDefOf.FortifyResource;

    protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
    {
        var resource = (ResourceDef)wrapper.Target;
        var max = actor.Resources.GetMax(resource);
        actor.Resources.SetMax(resource, max + wrapper.RemainingBudget.Value);
    }
    protected override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
    {
        var resource = (ResourceDef)wrapper.Target;
        var max = actor.Resources.GetMax(resource);
        actor.Resources.SetMax(resource, max - wrapper.RemainingBudget.Value);
    }

    internal override Color GetTint(Def target)
        => ((ResourceDef)target).Color;
}
