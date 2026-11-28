using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Effects;
using System;

namespace Project1.Core.Systems.Magic;

public sealed class Effect_ResourceDelta 
    : EntityEffectWorker<EffectConfig_ResourceDelta, EffectData_ResourceDelta>
{
    internal override Color Tint(EffectData_ResourceDelta dto)
       => dto.Resource.Color;

    internal override string Description(EffectData_ResourceDelta dto)
        => $"Restore {dto.Delta} {dto.Resource.LabelReadable}";

    protected override void OnPerform(Actor actor, EffectData_ResourceDelta dto)
        => actor.Resources.ApplyDelta(dto.Resource, dto.Delta);

    protected override float Score(Actor actor, EffectConfig_ResourceDelta dto)
    {
        var delta = dto.Delta;
        if (delta > 0f)
        {
            var payload = dto.Delta;
            var resource = actor.Resources.View(dto.Resource);
            var urgency = resource.Percentage;
            var deficit = resource.Deficit;
            var overshoot = Math.Max(0, payload - deficit);
            var targetRemanining = Math.Max(0, deficit - payload);
            var score = deficit - (urgency * overshoot + (1 - urgency) * targetRemanining);
            return score;
        }

        if (delta < 0f)
        {
            var available = actor.Resources.GetValue(dto.Resource);
            return -Math.Min(available, -delta);
        }

        return 0f;
    }

    protected override string GetString(EffectData_ResourceDelta dto)
        => string.Join(' ', [
            $"{dto.Delta}", 
            $"{(dto.Delta >= 0 ? "healing" : "damage")}"
            ]);
}
