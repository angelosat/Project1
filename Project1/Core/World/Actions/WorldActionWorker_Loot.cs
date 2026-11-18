using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using System.Linq;

namespace Project1.Core.World.Actions;

internal sealed class WorldActionWorker_Loot : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var actor = data.Actor;
        var typed = (WorldActionData_Loot)data;
        var items = actor.World.Resolve(typed.Loot);
        foreach (var treasure in items)
            actor.Inventory.InsertAndClaim(treasure);
        actor.AI.State.Log.Write($"Treasure found! {string.Join(",", items.Select(i => $"[{i.Name}]"))}");
    }

    static public void Begin(Actor actor, EntityRefId[] loot)
        => (actor.World as StaticWorld).Actions.Begin(WorldActionDefOf.Loot, new WorldActionData_Loot(actor) { Loot = loot });
}
