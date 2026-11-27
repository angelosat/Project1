using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.Magic;

public sealed class Effect_ResourceDelta : EntityEffectWorker<EffectData_ResourceDelta>
{
    internal override Color Tint(EffectData_ResourceDelta dto)
       => dto.Resource.Color;

    internal override string Description(EffectData_ResourceDelta dto)
        => $"Restore {dto.Delta} {dto.Resource.LabelReadable}";

    protected override void OnPerform(Actor actor, EffectData_ResourceDelta dto)
        => actor.Resources.ApplyDelta(dto.Resource, dto.Delta);
}
