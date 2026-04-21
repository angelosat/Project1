using Project1.Core.Entities;
using Project1.Core.Resources;

namespace Project1.Core.Systems.Abilities;

internal class AbilitiesComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Abilities;

    public override string Name => "Abilities";

    IResourceView Resource => field ??= this.Owner.Resources.View(ResourceDefOf.Mana);
    float Regen = 1f / Ticks.PerGameMinute;
    public override void Tick()
    {
        if (this.Owner.Net.IsClient)
            return;
        this.Resource.ApplyAccumulatorDelta(this.Regen);
    }
}
