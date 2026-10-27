using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.World;

internal sealed class OffmapActivity_RecoverHealth : OffmapActivity
{
    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
        => 0;// (int)((1-actor.Resources.GetPercentage(ResourceDefOf.Health)) * 10);

    protected override void TickInt(FrontierRuntime frontier, Actor actor)
    {
        actor.Resources.ApplyDelta(ResourceDefOf.Health, 5);
    }
}
