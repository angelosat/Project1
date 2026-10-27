using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.World;

internal sealed class OffmapActivity_FindTreasure : OffmapActivity
{
    protected override void TickInt(FrontierRuntime frontier, Actor actor)
    {
        var rand = actor.World.Random;
        if (!frontier.TryFindTreasure(rand, actor, out var treasure))
        {
            actor.AI.State.Log.Write($"Searched for treasure but found nothing");
            return;
        }
        actor.Inventory.Insert(treasure);
        actor.AI.State.Log.Write($"Treasure found! [{treasure.Name}]");
    }

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
    {
        return frontier.Treasures.Count;
    }
}
