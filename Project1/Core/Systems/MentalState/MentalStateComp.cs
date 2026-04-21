using Project1.Core.Entities;
using Project1.Core.Resources;

namespace Project1.Core.Systems.MentalState;

internal class MentalStateComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.MentalState;

    public override string Name => "Mental State";

    float Regen = 1f / Ticks.PerGameMinute;

    public override void Tick()
    {
        if (this.Owner.Net.IsClient)
            return;
        this.Owner.Resources.ApplyAccumulatorDelta(ResourceDefOf.Patience, this.Regen);
    }
}
