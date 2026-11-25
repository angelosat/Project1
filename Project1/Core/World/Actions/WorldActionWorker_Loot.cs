using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;
using System;
using System.Linq;

namespace Project1.Core.World.Actions;

internal sealed record WorldActionData_Loot(Actor Actor) : WorldActionData(Actor)
{
    internal override WorldActionDef Def => WorldActionDefOf.Loot;

    internal required EntityRefId[] Loot;

    public override void Write(IDataWriter w)
    {
        w.Write(Loot);
    }

    public override ISerializable Read(IDataReader r)
    {
        Loot = r.ReadArrayEntityRefId();
        return this;
    }
}

internal sealed class WorldActionWorker_Loot : WorldActionWorker
{
    internal override Type DataType => typeof(WorldActionData_Loot);

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
        => (actor.World as StaticWorld).Actions.Begin(new WorldActionData_Loot(actor) { Loot = loot });
}
