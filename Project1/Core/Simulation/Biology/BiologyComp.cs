using Project1.Core.Entities;
using Project1.Core.Resources;

namespace Project1.Core.Simulation.Biology;

internal class BiologyComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Biology;

    public override string Name => "Biology";

    IResourceView Health => field ??= this.Owner.Resources.View(ResourceDefOf.Health);
    float Regen = 1f / Ticks.FromMinutes(1);
    public override void Tick()
    {
        if (this.Owner.Net.IsClient)
            return;
        this.Health.ApplyAccumulatorDelta(this.Regen);
    }
}
