namespace Project1.Core.World.Actions;

internal sealed class WorldActionWorker_LootTreasure : WorldActionWorker
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
        if (!actor.AI.ItemPreferences.Wants(treasure))
        {
            actor.AI.State.Log.Write($"I found [{treasure.Name}] but I already have something better");
            return;
        }
        actor.Inventory.InsertAndClaim(treasure);
        actor.AI.State.Log.Write($"Treasure found! [{treasure.Name}]");
    }
}
