using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Effects;

#nullable enable

namespace Project1.Core.Systems.Consumables;

internal sealed class Effect_ConsumableCooldown : EntityEffectWorker
{
    public override EffectDef Def => EffectDefOf.ConsumableCooldown;

    protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
    {
    }

    internal override string Label(Def target)
        => EffectDefOf.ConsumableCooldown.LabelReadable;

    internal override string GetString(Def? target, int magnitude)
        //=> $"Prevent potion consumption for {magnitude} m";
        => $"{target.LabelReadable} lockout";
}
