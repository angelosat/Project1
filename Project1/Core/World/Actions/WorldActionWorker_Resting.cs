using Project1.Core.Needs;
using System;

namespace Project1.Core.World.Actions;

internal class WorldActionWorker_Resting : WorldActionWorker
{
    internal override Type DataType => typeof(WorldActionData);

    internal override void Resolve(WorldActionData data)
    {
        var actor = data.Actor;
        actor.AI.State.Log.Write($"I have rested a little bit");
        actor.Needs.ApplyAccumulatorDelta(NeedDefOf.Energy, +5);
    }
}
