using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.World.Actions;

internal sealed class WorldActionWorker_Equip : WorldActionWorker
{
    sealed record Data(Actor Actor) : WorldActionData(Actor)
    {
        internal required EquipStatusDecision Decision;

        public override void Write(IDataWriter w)
        {
            w.Write(this.Decision.ItemId);
            w.Write(this.Decision.Delta);
        }

        public override ISerializable Read(IDataReader r)
        {
            this.Decision.ItemId = r.ReadEntityRefId();
            this.Decision.Delta = r.ReadInt32();
            return this;
        }
    }

    internal override Type DataType => typeof(Data);

    public static (WorldActionDef, WorldActionData) Create(Actor actor, EquipStatusDecision decision)
        => (WorldActionDefOf.Equip, new Data(actor) { Decision = decision });

    internal override void Resolve(WorldActionData data)
    {
        var typed = (Data)data;
        var item = data.Actor.World.Resolve(typed.Decision.ItemId);
        if (typed.Decision.IsEquip)
            data.Actor.Gear.EquipToggle(item);
        else if (typed.Decision.IsUnequip)
            data.Actor.Inventory.Unequip(item);
    }
}

