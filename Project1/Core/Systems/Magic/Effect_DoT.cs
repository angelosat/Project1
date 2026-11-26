using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.Magic;

public sealed class Effect_DoT : EntityEffectWorker
{
    public override EffectDef Def => EffectDefOf.Burning;

    protected override void OnStart(Actor actor, EntityEffectRuntime wrapper)
    {

    }

    protected override void OnTick(Actor actor, EntityEffectRuntime runtime)
    {
        // TODO run the value through combatsystem or spellsystem to apply resistances
        actor.Resources.ApplyDelta(ResourceDefOf.Health, -(int)runtime.Budget.Value);
    }

    internal override Color GetTint(Def target)
    => ((SpellSchoolDef)this.Def.Context).Color;

    internal override string GetString(Def target, int magnitude)
        => $"{base.GetString(target, magnitude)} damage over time";
}
