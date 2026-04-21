using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Resources;

namespace Project1.Core.Simulation.Biology;

internal class BiologyComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Biology;

    public override string Name => "Biology";

    IResourceView Health => field ??= this.Owner.Resources.View(ResourceDefOf.Health);
    float Regen = 1f / Ticks.FromMinutes(1);
    readonly Accumulator HealthRegen = new();
    public override void Tick()
    {
        if (this.Owner.Net.IsClient)
            return;
        if(this.HealthRegen.AddAndTryFlush(this.Regen, out var delta))
            this.Health.ApplyDelta(delta);
    }
}
