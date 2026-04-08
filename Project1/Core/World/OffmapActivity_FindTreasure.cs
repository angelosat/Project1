using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.World;

internal sealed class OffmapActivity_FindTreasure : OffmapActivity
{
    internal override void Tick(FrontierWrapper frontier, Actor actor)
    {
        var rand = actor.World.Random;
        if (!frontier.TryFindTreasure(rand, actor, out var treasure))
        {
            actor.AI.State.Log.Write($"I searched for treasure.");
            return;
        }
        actor.Inventory.Insert(treasure);
        actor.AI.State.Log.Write($"I've found treasure! [{treasure.Name}]");
    }
}
