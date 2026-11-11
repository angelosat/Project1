using Project1.Core.World.Actions;

#nullable enable

namespace Project1.Core.World.Encounters;

internal sealed class WorldActionWorker_FindTreasure : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var actor = data.Actor;
        var frontier = data.Frontier;
        var rand = actor.World.Random;
        if (!frontier.TryFindTreasure(rand, actor, out var treasure))
        {
            actor.AI.State.Log.Write($"Searched for treasure but found nothing");
            return;
        }
        actor.Inventory.Insert(treasure);
        actor.AI.State.Log.Write($"Treasure found! [{treasure.Name}]");
    }
}
