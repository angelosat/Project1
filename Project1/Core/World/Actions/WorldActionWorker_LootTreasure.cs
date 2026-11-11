namespace Project1.Core.World.Actions;

internal class WorldActionWorker_LootTreasure : WorldActionWorker
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
        if (!actor.AI.ItemPreferences.ShouldPickUp(treasure))
        {
            actor.AI.State.Log.Write($"I found [{treasure.Name}] but I already have something better");
            return;
        }
        actor.Inventory.Insert(treasure);
        actor.AI.State.Log.Write($"Treasure found! [{treasure.Name}]");
    }
}
