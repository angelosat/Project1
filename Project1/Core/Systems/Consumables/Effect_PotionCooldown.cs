using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.Consumables;

internal sealed class Effect_PotionCooldown : EntityEffectWorker
{
    public override EffectDef Def => EffectDefOf.PotionCooldown;

    protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
    {
    }

    internal override string Label(Def target)
        => EffectDefOf.PotionCooldown.LabelReadable;
}
