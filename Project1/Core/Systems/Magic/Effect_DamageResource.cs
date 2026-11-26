using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.Magic;

public sealed class Effect_DamageResource : EntityEffectWorker
{
    public override EffectDef Def => EffectDefOf.SpellDamage;

    protected override void OnStart(Actor actor, EntityEffectRuntime wrapper)
    {
        var resource = ResourceDefOf.Health;
        actor.Resources.ApplyDelta(resource, -(int)wrapper.Budget.Value);
    }

    internal override Color GetTint(Def target)
    => ((ResourceDef)target).Color;

    internal override string GetString(Def target, int magnitude)
        => $"Damages {base.GetString(target, magnitude)} instantly";
}
