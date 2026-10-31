namespace Project1.Core.World.Actions;

internal class WorldActionWorker_FindTreasure : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var actor = data.Actor;
        var rand = actor.World.Random;
        if (!data.Frontier.TryFindTreasure(rand, actor, out var treasure))
        {
            actor.AI.State.Log.Write($"Searched for treasure but found nothing");
            return;
        }
        actor.Inventory.Insert(treasure);
        actor.AI.State.Log.Write($"Treasure found! [{treasure.Name}]");
    }
}
