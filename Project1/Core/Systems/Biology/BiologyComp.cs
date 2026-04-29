using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Resources;
using Project1.Framework.Events;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Biology;

internal class BiologyComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Biology;

    public override string Name => "Biology";

    IResourceView Health => field ??= this.Owner.Resources.View(ResourceDefOf.Health);
    Action<float> ApplyMetabolism => field ??= ((Actor)this.Owner).Needs.GetAccumulatorCallback(NeedDefOf.Hunger);
    Action<float> ApplyFatigue => field ??= ((Actor)this.Owner).Needs.GetAccumulatorCallback(NeedDefOf.Energy);
    public bool IsIncapacitated => this.Health.Value <= 0;
    bool WasIncapacitated;
    float Regen = 1f / Ticks.FromMinutes(1);
    float Metabolism = 1f / Ticks.FromMinutes(10);
    public override void Tick()
    {
        if (this.Owner.Net.IsClient)
            return;
        this.Health.ApplyAccumulatorDelta(this.Regen);
        var now = this.IsIncapacitated;
        if (now && !this.WasIncapacitated)
            this.World.Events.Post(new ActorIncapacitatedEvent(this.Owner as Actor));
        else if (!now && this.WasIncapacitated)
            this.World.Events.Post(new ActorRecoveredEvent(this.Owner as Actor));
        this.WasIncapacitated = now;

        this.ApplyMetabolism(-this.Metabolism);
        this.ApplyFatigue(-this.Metabolism);
    }
    public override void TickOffMap()
    {
        this.Tick();
    }

    internal override IEnumerable<Control> GetTooltipControls()
    {
        if (this.IsIncapacitated)
            yield return new LabelNew(() => "Incapacitated");
    }
}

internal record struct ActorIncapacitatedEvent(Actor Actor) : IEventPayload;
internal record struct ActorRecoveredEvent(Actor Actor) : IEventPayload;