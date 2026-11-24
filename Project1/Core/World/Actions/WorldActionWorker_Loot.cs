using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;
using System;
using System.Linq;

namespace Project1.Core.World.Actions;

internal sealed class WorldActionWorker_Loot : WorldActionWorker
{
    sealed record Data(Actor Actor) : WorldActionData(Actor)
    {
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

    internal override Type DataType => typeof(Data);

    internal override void Resolve(WorldActionData data)
    {
        var actor = data.Actor;
        var typed = (Data)data;
        var items = actor.World.Resolve(typed.Loot);
        foreach (var treasure in items)
            actor.Inventory.InsertAndClaim(treasure);
        actor.AI.State.Log.Write($"Treasure found! {string.Join(",", items.Select(i => $"[{i.Name}]"))}");
    }

    static public void Begin(Actor actor, EntityRefId[] loot)
        => (actor.World as StaticWorld).Actions.Begin(WorldActionDefOf.Loot, new Data(actor) { Loot = loot });
}
