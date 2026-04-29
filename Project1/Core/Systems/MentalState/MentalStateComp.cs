using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Resources;
using System;

namespace Project1.Core.Systems.MentalState;

internal class MentalStateComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.MentalState;

    IResourceView Patience => field ??= this.Owner.Resources.View(ResourceDefOf.Patience);

    Action<float> TickSocial => field ??= ((Actor)this.Owner).Needs.GetAccumulatorCallback(NeedDefOf.Social);

    public override string Name => "Mental State";

    float Regen = 1f / Ticks.PerGameMinute;

    public override void Tick()
    {
        if (this.Owner.Net.IsClient)
            return;
        this.Patience.ApplyAccumulatorDelta(this.Regen);
    }
}
