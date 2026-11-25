using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Effects;
using Project1.Core.World.Combat;

namespace Project1.Core.Systems.Magic;

public sealed class Effect_DamageResource : EntityEffectWorker
{
    public override EffectDef Def => EffectDefOf.SpellDamage;

    protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
    {
        var resource = (ResourceDef)wrapper.Target;
        actor.Resources.ApplyDelta(resource, -(int)wrapper.Budget.Value);
        //CombatSystem.ResolveMagicHit(actor)
    }

    internal override Color GetTint(Def target)
    => ((ResourceDef)target).Color;

    internal override string GetString(Def target, int magnitude)
        => $"Damages {base.GetString(target, magnitude)} instantly";
}
