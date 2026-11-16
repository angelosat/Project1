using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.World.Frontiers;
using Project1.Framework.Serialization;
using System.Linq;

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
        if (!actor.AI.ItemPreferences.Wants(treasure))
        {
            actor.AI.State.Log.Write($"I found [{treasure.Name}] but I already have something better");
            return;
        }
        actor.Inventory.InsertAndClaim(treasure);
        actor.AI.State.Log.Write($"Treasure found! [{treasure.Name}]");
    }
}

internal sealed record WorldActionData_Loot(FrontierRuntime Frontier, Actor Actor) : WorldActionData(Frontier, Actor)
{
    internal required EntityRefId[] Loot;

    public override void Write(IDataWriter w)
    {
        w.Write(this.Loot);
    }

    public override ISerializable Read(IDataReader r)
    {
        this.Loot = r.ReadArrayEntityRefId();
        return this;
    }
}

internal class WorldActionWorker_Loot : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var actor = data.Actor;
        var typed = (WorldActionData_Loot)data;
        var items = actor.World.Resolve(typed.Loot);
        foreach (var treasure in items)
            //ItemTransferSystem.Loot(actor, treasure);
            actor.Inventory.InsertAndClaim(treasure);
        actor.AI.State.Log.Write($"Treasure found! {string.Join(",", items.Select(i=>$"[{i.Name}]"))}");
    }
}