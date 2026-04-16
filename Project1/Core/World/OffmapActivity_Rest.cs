using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.World;

internal sealed class OffmapActivity_Rest : OffmapActivity
{
    internal override void Tick(FrontierWrapper frontier, Actor actor)
    {
        actor.AI.State.Log.Write($"I have rested a little bit");
        actor.Needs.ApplyAccumulatorDelta(NeedDefOf.Energy, +5);
    }
}
