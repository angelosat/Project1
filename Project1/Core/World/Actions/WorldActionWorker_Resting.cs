using Project1.Core.Needs;
using Project1.Core.Resources;
using Project1.Core.World.Encounters;
using Project1.Framework;

namespace Project1.Core.World.Actions;

internal class WorldActionWorker_Resting : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var actor = data.Actor;
        actor.AI.State.Log.Write($"I have rested a little bit");
        actor.Needs.ApplyAccumulatorDelta(NeedDefOf.Energy, +5);
    }
}
