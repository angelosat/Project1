using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.Magic;

public sealed class Effect_RestoreResource : EntityEffectWorker
{
    public override EffectDef Def => EffectDefOf.RestoreResource;

    protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
    {
        var resource = (ResourceDef)wrapper.Target;
        actor.Resources.ApplyDelta(resource, wrapper.Budget.Value);
    }

    internal override Color GetTint(Def target)
    => ((ResourceDef)target).Color;
}
